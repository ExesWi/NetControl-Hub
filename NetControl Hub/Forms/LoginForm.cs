using System;
using System.Drawing;
using System.Windows.Forms;
using NetControl_Hub.Services;
using NetControl_Hub.Models;

namespace NetControl_Hub.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private Panel mainPanel;

        public LoginForm()
        {
            _authService = new AuthService();
            InitializeComponent();
            SetupForm();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Main Panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Title Label
            lblTitle = new Label
            {
                Text = "NetControl Hub",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };

            // Username Label
            lblUsername = new Label
            {
                Text = "Имя пользователя:",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                Location = new Point(50, 120),
                Size = new Size(200, 25)
            };

            // Username TextBox
            txtUsername = new TextBox
            {
                Location = new Point(50, 150),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password Label
            lblPassword = new Label
            {
                Text = "Пароль:",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                Location = new Point(50, 200),
                Size = new Size(200, 25)
            };

            // Password TextBox
            txtPassword = new TextBox
            {
                Location = new Point(50, 230),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            // Login Button
            btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(150, 290),
                Size = new Size(150, 40),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // Event handlers
            btnLogin.Click += BtnLogin_Click;
            txtPassword.KeyPress += TxtPassword_KeyPress;
            txtUsername.KeyPress += TxtUsername_KeyPress;

            // Add controls to panel
            mainPanel.Controls.AddRange(new Control[] 
            { 
                lblTitle, lblUsername, txtUsername, lblPassword, txtPassword, btnLogin 
            });

            // Form properties
            this.Controls.Add(mainPanel);
            this.Text = "NetControl Hub - Вход в систему";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);

            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Focus on username field
            txtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            AttemptLogin();
        }

        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                AttemptLogin();
                e.Handled = true;
            }
        }

        private void TxtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtPassword.Focus();
                e.Handled = true;
            }
        }

        private void AttemptLogin()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя и пароль.", 
                    "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var user = _authService.AuthenticateUser(username, password);

                if (user != null)
                {
                    // Успешный вход
                    MessageBox.Show(string.Format("Добро пожаловать, {0}!", user.DisplayName), 
                        "Успешный вход", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Открываем главную форму
                    var mainForm = new MainForm(user);
                    this.Hide();
                    mainForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль.", 
                        "Ошибка аутентификации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Произошла ошибка при входе: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
