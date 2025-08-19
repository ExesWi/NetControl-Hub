using System;
using System.Drawing;
using System.Windows.Forms;
using NetControl_Hub.Models;

namespace NetControl_Hub.Forms
{
    public class AddUserForm : Form
    {
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private TextBox _txtDisplayName;
        private ComboBox _cbRole;
        private Button _btnOk;
        private Button _btnCancel;

        public User ResultUser { get; private set; }

        public AddUserForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Добавить пользователя";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            var lblU = new Label { Text = "Логин:", Location = new Point(20, 20), AutoSize = true };
            _txtUsername = new TextBox { Location = new Point(150, 16), Width = 220 };

            var lblP = new Label { Text = "Пароль:", Location = new Point(20, 60), AutoSize = true };
            _txtPassword = new TextBox { Location = new Point(150, 56), Width = 220, UseSystemPasswordChar = true };

            var lblN = new Label { Text = "Имя:", Location = new Point(20, 100), AutoSize = true };
            _txtDisplayName = new TextBox { Location = new Point(150, 96), Width = 220 };

            var lblR = new Label { Text = "Роль:", Location = new Point(20, 140), AutoSize = true };
            _cbRole = new ComboBox { Location = new Point(150, 136), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _cbRole.Items.AddRange(new object[] { UserRole.Developer, UserRole.Director, UserRole.Employee });
            _cbRole.SelectedIndex = 2;

            _btnOk = new Button { Text = "Добавить", Location = new Point(150, 200), Size = new Size(100, 36), BackColor = Color.FromArgb(70, 130, 180), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Click += (s, e) => OnOk();

            _btnCancel = new Button { Text = "Отмена", Location = new Point(270, 200), Size = new Size(100, 36), BackColor = Color.FromArgb(224, 224, 224), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblU);
            this.Controls.Add(_txtUsername);
            this.Controls.Add(lblP);
            this.Controls.Add(_txtPassword);
            this.Controls.Add(lblN);
            this.Controls.Add(_txtDisplayName);
            this.Controls.Add(lblR);
            this.Controls.Add(_cbRole);
            this.Controls.Add(_btnOk);
            this.Controls.Add(_btnCancel);

            this.ResumeLayout(false);
        }

        private void OnOk()
        {
            if (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ResultUser = new User
            {
                Username = _txtUsername.Text.Trim(),
                Password = _txtPassword.Text,
                DisplayName = string.IsNullOrWhiteSpace(_txtDisplayName.Text) ? _txtUsername.Text.Trim() : _txtDisplayName.Text.Trim(),
                Role = (UserRole)_cbRole.SelectedItem,
                LastLogin = DateTime.MinValue,
                LoginCount = 0
            };
            this.DialogResult = DialogResult.OK;
        }
    }
}


