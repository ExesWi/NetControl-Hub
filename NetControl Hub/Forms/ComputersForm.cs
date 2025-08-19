using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using NetControl_Hub.Models;
using NetControl_Hub.Services;

namespace NetControl_Hub.Forms
{
    public partial class ComputersForm : Form
    {
        private readonly User _currentUser;
        private readonly ComputerService _computerService;
        private ListView listViewComputers;
        private Button btnRefresh;
        private Button btnEnableAccess;
        private Button btnDisableAccess;
        private Button btnViewFiles;
        private Button btnClose;
        private Panel mainPanel;
        private Panel buttonPanel;

        public ComputersForm(User currentUser, ComputerService computerService)
        {
            _currentUser = currentUser;
            _computerService = computerService;
            InitializeComponent();
            LoadComputers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Управление компьютерами сети";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Main Panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(20)
            };

            // Computers ListView
            listViewComputers = new ListView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            listViewComputers.Columns.Add("Компьютер", 150);
            listViewComputers.Columns.Add("IP Адрес", 120);
            listViewComputers.Columns.Add("Пользователь", 120);
            listViewComputers.Columns.Add("Роль", 100);
            listViewComputers.Columns.Add("Статус", 80);
            listViewComputers.Columns.Add("Удаленный доступ", 120);
            listViewComputers.Columns.Add("Последний вход", 150);

            // Button Panel
            buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            btnRefresh = new Button
            {
                Text = "Обновить",
                Size = new Size(100, 35),
                Location = new Point(10, 12),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnEnableAccess = new Button
            {
                Text = "Включить доступ",
                Size = new Size(120, 35),
                Location = new Point(120, 12),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            btnDisableAccess = new Button
            {
                Text = "Отключить доступ",
                Size = new Size(120, 35),
                Location = new Point(250, 12),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            btnViewFiles = new Button
            {
                Text = "Просмотр файлов",
                Size = new Size(120, 35),
                Location = new Point(380, 12),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            btnClose = new Button
            {
                Text = "Закрыть",
                Size = new Size(100, 35),
                Location = new Point(510, 12),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            buttonPanel.Controls.AddRange(new Control[] { btnRefresh, btnEnableAccess, btnDisableAccess, btnViewFiles, btnClose });

            // Add controls to main panel
            mainPanel.Controls.Add(listViewComputers);
            mainPanel.Controls.Add(buttonPanel);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Event handlers
            btnRefresh.Click += BtnRefresh_Click;
            btnEnableAccess.Click += BtnEnableAccess_Click;
            btnDisableAccess.Click += BtnDisableAccess_Click;
            btnViewFiles.Click += BtnViewFiles_Click;
            btnClose.Click += BtnClose_Click;
            listViewComputers.SelectedIndexChanged += ListViewComputers_SelectedIndexChanged;

            this.ResumeLayout(false);
        }

        private void LoadComputers()
        {
            try
            {
                listViewComputers.Items.Clear();
                var computers = _computerService.GetAllComputers();

                foreach (var computer in computers)
                {
                    var item = new ListViewItem(computer.Name)
                    {
                        Tag = computer,
                        SubItems = {
                            computer.IpAddress,
                            computer.UserName,
                            GetRoleDisplayName(computer.UserRole),
                            computer.IsOnline ? "Онлайн" : "Офлайн",
                            computer.RemoteAccessEnabled ? "Включен" : "Отключен",
                            computer.LastSeen.ToString("dd.MM.yyyy HH:mm")
                        }
                    };

                    // Цветовая индикация статуса
                    if (computer.IsOnline)
                    {
                        item.BackColor = Color.FromArgb(0, 100, 0);
                    }
                    else
                    {
                        item.BackColor = Color.FromArgb(100, 0, 0);
                    }

                    listViewComputers.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка загрузки компьютеров: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetRoleDisplayName(string role)
        {
            switch (role)
            {
                case "Developer": return "Разработчик";
                case "Director": return "Директор";
                case "Employee": return "Сотрудник";
                default: return role;
            }
        }

        private void ListViewComputers_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = listViewComputers.SelectedItems.Count > 0;
            btnEnableAccess.Enabled = hasSelection;
            btnDisableAccess.Enabled = hasSelection;
            btnViewFiles.Enabled = hasSelection;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadComputers();
        }

        private void BtnEnableAccess_Click(object sender, EventArgs e)
        {
            if (listViewComputers.SelectedItems.Count > 0)
            {
                var selectedItem = listViewComputers.SelectedItems[0];
                var computer = selectedItem.Tag as Computer;

                if (computer != null)
                {
                    var result = MessageBox.Show(
                        string.Format("Включить удаленный доступ к компьютеру '{0}'?\n\nЭто позволит просматривать все файлы на компьютере.", computer.Name),
                        "Подтверждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (_computerService.EnableRemoteAccess(computer.Id, _currentUser.Username))
                        {
                            MessageBox.Show("Удаленный доступ включен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadComputers();
                        }
                    }
                }
            }
        }

        private void BtnDisableAccess_Click(object sender, EventArgs e)
        {
            if (listViewComputers.SelectedItems.Count > 0)
            {
                var selectedItem = listViewComputers.SelectedItems[0];
                var computer = selectedItem.Tag as Computer;

                if (computer != null)
                {
                    var result = MessageBox.Show(
                        string.Format("Отключить удаленный доступ к компьютеру '{0}'?", computer.Name),
                        "Подтверждение",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (_computerService.DisableRemoteAccess(computer.Id))
                        {
                            MessageBox.Show("Удаленный доступ отключен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadComputers();
                        }
                    }
                }
            }
        }

        private void BtnViewFiles_Click(object sender, EventArgs e)
        {
            if (listViewComputers.SelectedItems.Count > 0)
            {
                var selectedItem = listViewComputers.SelectedItems[0];
                var computer = selectedItem.Tag as Computer;

                if (computer != null)
                {
                    if (!computer.RemoteAccessEnabled)
                    {
                        MessageBox.Show("Удаленный доступ к этому компьютеру не включен!", "Ошибка", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!computer.IsOnline)
                    {
                        MessageBox.Show("Компьютер не в сети!", "Ошибка", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Открываем форму просмотра файлов
                    var fileExplorerForm = new RemoteFileExplorerForm(computer, _currentUser);
                    fileExplorerForm.Show();
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
