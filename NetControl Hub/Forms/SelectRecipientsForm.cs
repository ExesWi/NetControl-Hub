using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NetControl_Hub.Forms
{
    public partial class SelectRecipientsForm : Form
    {
        private readonly List<EmailContact> _allContacts;
        public List<EmailContact> SelectedContacts { get; private set; }

        public SelectRecipientsForm(List<EmailContact> contacts)
        {
            _allContacts = contacts;
            SelectedContacts = new List<EmailContact>();
            InitializeComponent();
            LoadContacts();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Основные цвета
            Color primaryColor = Color.FromArgb(70, 130, 180);
            Color backgroundColor = Color.FromArgb(250, 250, 252);
            Color textColor = Color.FromArgb(55, 71, 79);

            // Настройка формы
            this.Text = "Выбор получателей";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = backgroundColor;
            this.Font = new Font("Segoe UI", 9);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Заголовок
            var lblTitle = new Label
            {
                Text = "Выберите получателей",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = primaryColor,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White
            };

            // Основная панель
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backgroundColor,
                Padding = new Padding(20)
            };

            // Поиск
            var lblSearch = new Label
            {
                Text = "Поиск:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(10, 10),
                Size = new Size(80, 20)
            };

            var txtSearch = new TextBox
            {
                Location = new Point(100, 7),
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Список контактов
            var lblContacts = new Label
            {
                Text = "Контакты:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(10, 45),
                Size = new Size(80, 20)
            };

            var lstContacts = new ListView
            {
                Location = new Point(10, 70),
                Size = new Size(460, 200),
                Font = new Font("Segoe UI", 10),
                View = View.Details,
                FullRowSelect = true,
                CheckBoxes = true,
                GridLines = true
            };

            lstContacts.Columns.Add("Имя", 200);
            lstContacts.Columns.Add("Email", 260);

            // Кнопки
            var btnSelectAll = new Button
            {
                Text = "Выбрать все",
                Location = new Point(10, 280),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };

            var btnClearAll = new Button
            {
                Text = "Снять выбор",
                Location = new Point(120, 280),
                Size = new Size(100, 30),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };

            var btnOK = new Button
            {
                Text = "OK",
                Location = new Point(300, 280),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(390, 280),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                DialogResult = DialogResult.Cancel
            };

            // Сборка интерфейса
            mainPanel.Controls.AddRange(new Control[] { 
                lblSearch, txtSearch, lblContacts, lstContacts, 
                btnSelectAll, btnClearAll, btnOK, btnCancel 
            });

            this.Controls.Add(mainPanel);
            this.Controls.Add(lblTitle);

            // Обработчики событий
            btnSelectAll.Click += (s, e) =>
            {
                foreach (ListViewItem item in lstContacts.Items)
                {
                    item.Checked = true;
                }
            };

            btnClearAll.Click += (s, e) =>
            {
                foreach (ListViewItem item in lstContacts.Items)
                {
                    item.Checked = false;
                }
            };

            txtSearch.TextChanged += (s, e) =>
            {
                FilterContacts(txtSearch.Text);
            };

            btnOK.Click += (s, e) =>
            {
                SelectedContacts.Clear();
                foreach (ListViewItem item in lstContacts.Items)
                {
                    if (item.Checked)
                    {
                        var contact = item.Tag as EmailContact;
                        if (contact != null)
                        {
                            SelectedContacts.Add(contact);
                        }
                    }
                }
            };

            this.ResumeLayout(false);
        }

        private void LoadContacts()
        {
            var lstContacts = this.Controls.Find("lstContacts", true).FirstOrDefault() as ListView;
            if (lstContacts != null)
            {
                lstContacts.Items.Clear();
                foreach (var contact in _allContacts)
                {
                    var item = new ListViewItem(contact.Name);
                    item.SubItems.Add(contact.Email);
                    item.Tag = contact;
                    lstContacts.Items.Add(item);
                }
            }
        }

        private void FilterContacts(string searchText)
        {
            var lstContacts = this.Controls.Find("lstContacts", true).FirstOrDefault() as ListView;
            if (lstContacts != null)
            {
                lstContacts.Items.Clear();
                var filteredContacts = _allContacts.Where(c => 
                    c.Name.ToLower().Contains(searchText.ToLower()) || 
                    c.Email.ToLower().Contains(searchText.ToLower())).ToList();

                foreach (var contact in filteredContacts)
                {
                    var item = new ListViewItem(contact.Name);
                    item.SubItems.Add(contact.Email);
                    item.Tag = contact;
                    lstContacts.Items.Add(item);
                }
            }
        }
    }
}
