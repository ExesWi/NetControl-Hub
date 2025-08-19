using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetControl_Hub.Forms
{
    public partial class AddEmailAccountForm : Form
    {
        public string Email { get; private set; }
        public string AccountName { get; private set; }

        public AddEmailAccountForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Основные цвета
            Color primaryColor = Color.FromArgb(70, 130, 180);
            Color backgroundColor = Color.FromArgb(250, 250, 252);
            Color textColor = Color.FromArgb(55, 71, 79);

            // Настройка формы
            this.Text = "Добавить аккаунт";
            this.Size = new Size(400, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = backgroundColor;
            this.Font = new Font("Segoe UI", 9);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Заголовок
            var lblTitle = new Label
            {
                Text = "Добавить новый email аккаунт",
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

            // Поле "Имя"
            var lblName = new Label
            {
                Text = "Имя:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(20, 20),
                Size = new Size(80, 20)
            };

            var txtName = new TextBox
            {
                Location = new Point(110, 17),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Поле "Email"
            var lblEmail = new Label
            {
                Text = "Email:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(20, 55),
                Size = new Size(80, 20)
            };

            var txtEmail = new TextBox
            {
                Location = new Point(110, 52),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Кнопки
            var btnOK = new Button
            {
                Text = "OK",
                Location = new Point(200, 100),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 10),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(290, 100),
                Size = new Size(80, 30),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                DialogResult = DialogResult.Cancel
            };

            // Сборка интерфейса
            mainPanel.Controls.AddRange(new Control[] { lblName, txtName, lblEmail, txtEmail, btnOK, btnCancel });

            this.Controls.Add(mainPanel);
            this.Controls.Add(lblTitle);

            // Обработчики событий
            btnOK.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите имя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Введите email!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                if (!txtEmail.Text.Contains("@"))
                {
                    MessageBox.Show("Введите корректный email адрес!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                this.AccountName = txtName.Text.Trim();
                this.Email = txtEmail.Text.Trim();
            };

            this.ResumeLayout(false);
        }
    }
}
