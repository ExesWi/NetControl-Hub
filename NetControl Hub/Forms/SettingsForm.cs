using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetControl_Hub.Models;
using NetControl_Hub.Services;

namespace NetControl_Hub.Forms
{
    public class SettingsForm : Form
    {
        private readonly AuthService _authService;
        private ListView _listUsers;
        private Button _btnAdd;
        private Button _btnDelete;
        private Label _lblTotal;
        private Label _lblLogins;

        public SettingsForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Настройки - Пользователи";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            _listUsers = new ListView
            {
                Dock = DockStyle.Top,
                Height = 420,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = true,
                CheckBoxes = false
            };
            _listUsers.Columns.Add("Логин", 160);
            _listUsers.Columns.Add("Имя", 180);
            _listUsers.Columns.Add("Роль", 120);
            _listUsers.Columns.Add("Последний вход", 180);
            _listUsers.Columns.Add("Входов", 80);

            _btnAdd = new Button
            {
                Text = "Добавить",
                Size = new Size(120, 36),
                Location = new Point(20, 450),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnAdd.FlatAppearance.BorderSize = 0;
            _btnAdd.Click += (s, e) => AddUser();

            _btnDelete = new Button
            {
                Text = "Удалить выбранные",
                Size = new Size(140, 36),
                Location = new Point(160, 450),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnDelete.FlatAppearance.BorderSize = 0;
            _btnDelete.Click += (s, e) => DeleteSelectedUsers();

            _lblTotal = new Label
            {
                Text = "Пользователей: 0",
                Location = new Point(20, 510),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 71, 79),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            _lblLogins = new Label
            {
                Text = "Всего входов: 0",
                Location = new Point(220, 510),
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 71, 79),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var lblHint = new Label
            {
                Text = "💡 Подсказка: Ctrl+Click для выбора нескольких пользователей",
                Location = new Point(20, 540),
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 100, 100),
                Font = new Font("Segoe UI", 9)
            };

            this.Controls.Add(_listUsers);
            this.Controls.Add(_btnAdd);
            this.Controls.Add(_btnDelete);
            this.Controls.Add(_lblTotal);
            this.Controls.Add(_lblLogins);
            this.Controls.Add(lblHint);

            this.ResumeLayout(false);
        }

        private void LoadUsers()
        {
            _listUsers.Items.Clear();
            var users = _authService.GetAllUsers();
            foreach (var u in users)
            {
                var item = new ListViewItem(u.Username);
                item.SubItems.Add(u.DisplayName);
                item.SubItems.Add(u.Role.ToString());
                item.SubItems.Add(u.LastLogin == DateTime.MinValue ? "—" : u.LastLogin.ToString("dd.MM.yyyy HH:mm"));
                item.SubItems.Add(u.LoginCount.ToString());
                _listUsers.Items.Add(item);
            }

            _lblTotal.Text = "Пользователей: " + users.Count;
            _lblLogins.Text = "Всего входов: " + users.Sum(x => x.LoginCount);
        }

        private void AddUser()
        {
            using (var dlg = new AddUserForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var newUser = dlg.ResultUser;
                    if (!_authService.AddUser(newUser))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    LoadUsers();
                }
            }
        }

        private void DeleteSelectedUsers()
        {
            if (_listUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите пользователей для удаления", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedUsernames = new List<string>();
            foreach (ListViewItem item in _listUsers.SelectedItems)
            {
                selectedUsernames.Add(item.Text);
            }

            string message;
            if (selectedUsernames.Count == 1)
            {
                message = string.Format("Удалить пользователя '{0}'?", selectedUsernames[0]);
            }
            else
            {
                message = string.Format("Удалить {0} пользователей?\n\n{1}", 
                    selectedUsernames.Count, 
                    string.Join("\n", selectedUsernames.Take(5)) + (selectedUsernames.Count > 5 ? "\n..." : ""));
            }

            if (MessageBox.Show(message, "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int deletedCount = 0;
                int failedCount = 0;
                
                foreach (string username in selectedUsernames)
                {
                    if (_authService.DeleteUser(username))
                    {
                        deletedCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }

                if (failedCount > 0)
                {
                    MessageBox.Show(string.Format("Удалено: {0}\nНе удалось удалить: {1}", deletedCount, failedCount), 
                        "Результат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(string.Format("Успешно удалено пользователей: {0}", deletedCount), 
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                LoadUsers();
            }
        }
    }
}


