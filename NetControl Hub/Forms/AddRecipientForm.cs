using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetControl_Hub.Forms
{
    public partial class AddRecipientForm : Form
    {
        public string Email { get; private set; }
        public string RecipientName { get; private set; }

        public AddRecipientForm()
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
            this.Text = "Добавить получателя";
            this.Size = new Size(400, 250); // Увеличили высоту с 200 до 250
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = backgroundColor;
            this.Font = new Font("Segoe UI", 9);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Заголовок
            var lblTitle = new Label
            {
                Text = "Добавить получателя",
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

            // Поле "Имя" (необязательное)
            var lblName = new Label
            {
                Text = "Имя (необязательно):",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(20, 20),
                Size = new Size(150, 20)
            };

            var txtName = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Поле "Email" (обязательное)
            var lblEmail = new Label
            {
                Text = "Email:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(20, 85), // Увеличили отступ с 80 до 85
                Size = new Size(80, 20)
            };

            var txtEmail = new TextBox
            {
                Location = new Point(20, 110), // Увеличили отступ с 105 до 110
                Size = new Size(350, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Кнопки
            var btnOK = new Button
            {
                Text = "Добавить",
                Location = new Point(200, 160), // Увеличили отступ с 150 до 160
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
                Location = new Point(290, 160), // Увеличили отступ с 150 до 160
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
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Введите email адрес!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                if (!txtEmail.Text.Contains("@"))
                {
                    MessageBox.Show("Введите корректный email адрес!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                this.RecipientName = txtName.Text.Trim();
                this.Email = txtEmail.Text.Trim();
            };

            // Фокус на поле email при открытии
            this.Load += (s, e) => txtEmail.Focus();

            this.ResumeLayout(false);
        }
    }
}
