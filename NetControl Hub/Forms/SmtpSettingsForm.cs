using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace NetControl_Hub.Forms
{
    public partial class SmtpSettingsForm : Form
    {
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string SmtpServer { get; private set; }
        public int SmtpPort { get; private set; }
        public bool UseSsl { get; private set; }

        // Ссылки на элементы управления
        private TextBox _txtEmail;
        private TextBox _txtPassword;
        private TextBox _txtSmtpServer;
        private TextBox _txtPort;
        private CheckBox _chkUseSsl;
        private CheckBox _chkRememberPassword;

        // Путь к файлу с настройками
        private static readonly string NetworkSettingsPath = Path.Combine(
            @"\\DESKTOP-56FV0G8\Users\Exest\3D Objects\Server\Shared\Net Control Hub\Settings",
            "smtp_settings.txt");
        
        private static readonly string LocalSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NetControl Hub",
            "smtp_settings.txt");
        
        private string SettingsPath => NetworkSettingsPath; // Всегда используем сетевую папку
        
        private bool IsNetworkPathAvailable()
        {
            try
            {
                string networkFolder = Path.GetDirectoryName(NetworkSettingsPath);
                return Directory.Exists(networkFolder);
            }
            catch
            {
                return false;
            }
        }

        public SmtpSettingsForm(string email = "")
        {
            InitializeComponent();
            
            // Показываем пользователю, что настройки будут сохраняться в сетевую папку
            Console.WriteLine($"Настройки SMTP будут сохраняться в сетевую папку: {NetworkSettingsPath}");
            
            // Загружаем сохраненные настройки
            LoadSavedSettings();
            
            if (!string.IsNullOrEmpty(email))
            {
                _txtEmail.Text = email;
                UpdateSmtpSettings();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Основные цвета
            Color primaryColor = Color.FromArgb(70, 130, 180);
            Color backgroundColor = Color.FromArgb(250, 250, 252);
            Color textColor = Color.FromArgb(55, 71, 79);

            // Настройка формы
            this.Text = "Настройки SMTP";
            this.Size = new Size(450, 420); // Увеличили высоту с 380 до 420
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = backgroundColor;
            this.Font = new Font("Segoe UI", 9);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Заголовок
            var lblTitle = new Label
            {
                Text = "Настройки SMTP сервера",
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

            // Email
            var lblEmail = new Label
            {
                Text = "Email:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(20, 20),
                Size = new Size(80, 20)
            };

            var txtEmail = new TextBox
            {
                Name = "txtEmail",
                Location = new Point(20, 45),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Пароль
            var lblPassword = new Label
            {
                Text = "Пароль:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(20, 80),
                Size = new Size(80, 20)
            };

            var txtPassword = new TextBox
            {
                Name = "txtPassword",
                Location = new Point(20, 105),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            // Запомнить пароль
            var chkRememberPassword = new CheckBox
            {
                Name = "chkRememberPassword",
                Text = "Запомнить пароль (для корпоративного использования)",
                Font = new Font("Segoe UI", 9),
                ForeColor = textColor,
                Location = new Point(20, 135),
                Size = new Size(400, 20),
                Checked = true
            };

            // SMTP сервер
            var lblSmtpServer = new Label
            {
                Text = "SMTP сервер:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(20, 170),
                Size = new Size(100, 20)
            };

            var txtSmtpServer = new TextBox
            {
                Name = "txtSmtpServer",
                Location = new Point(20, 195),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };

            // Порт
            var lblPort = new Label
            {
                Text = "Порт:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(20, 230),
                Size = new Size(80, 20)
            };

            var txtPort = new TextBox
            {
                Name = "txtPort",
                Location = new Point(20, 255),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };

            // SSL
            var chkUseSsl = new CheckBox
            {
                Name = "chkUseSsl",
                Text = "Использовать SSL/TLS",
                Font = new Font("Segoe UI", 10),
                ForeColor = textColor,
                Location = new Point(140, 255),
                Size = new Size(150, 25),
                Checked = true,
                Enabled = false
            };

            // Кнопки
            var btnOK = new Button
            {
                Text = "Сохранить",
                Location = new Point(250, 300),
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
                Location = new Point(340, 300),
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
                lblEmail, txtEmail, 
                lblPassword, txtPassword, chkRememberPassword,
                lblSmtpServer, txtSmtpServer,
                lblPort, txtPort, chkUseSsl,
                btnOK, btnCancel 
            });

            this.Controls.Add(mainPanel);
            this.Controls.Add(lblTitle);

            // Сохраняем ссылки на элементы управления
            _txtEmail = txtEmail;
            _txtPassword = txtPassword;
            _txtSmtpServer = txtSmtpServer;
            _txtPort = txtPort;
            _chkUseSsl = chkUseSsl;
            _chkRememberPassword = chkRememberPassword;

            // Обработчики событий
            txtEmail.TextChanged += (s, e) => UpdateSmtpSettings();
            btnOK.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtEmail.Text))
                {
                    MessageBox.Show("Введите email!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtEmail.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtPassword.Text))
                {
                    MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Focus();
                    return;
                }

                if (!_txtEmail.Text.Contains("@"))
                {
                    MessageBox.Show("Введите корректный email!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtEmail.Focus();
                    return;
                }

                this.Email = _txtEmail.Text.Trim();
                this.Password = _txtPassword.Text;
                this.SmtpServer = _txtSmtpServer.Text;
                this.SmtpPort = int.Parse(_txtPort.Text);
                this.UseSsl = _chkUseSsl.Checked;

                // Сохраняем настройки если отмечен чекбокс
                if (_chkRememberPassword.Checked)
                {
                    SaveSettings();
                }
            };

            // Фокус на поле email при открытии
            this.Load += (s, e) => _txtEmail.Focus();

            this.ResumeLayout(false);
        }

        private void LoadSavedSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string content = File.ReadAllText(SettingsPath);
                    
                    // Проверяем, является ли это JSON-подобным форматом
                    if (content.Contains("\"smtp_settings\""))
                    {
                        LoadJsonFormatSettings(content);
                    }
                    else
                    {
                        // Старый формат (для обратной совместимости)
                        LoadLegacyFormatSettings(content);
                    }
                    
                    // Показываем откуда загружены настройки
                    Console.WriteLine($"Настройки SMTP загружены из сетевой папки: {SettingsPath}");
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки загрузки настроек
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настроек SMTP: {ex.Message}");
            }
        }
        
        private void LoadJsonFormatSettings(string content)
        {
            try
            {
                // Простой парсер для JSON-подобного формата
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    if (trimmedLine.Contains("\"email\":"))
                    {
                        _txtEmail.Text = ExtractValue(trimmedLine);
                    }
                    else if (trimmedLine.Contains("\"password\":"))
                    {
                        _txtPassword.Text = ExtractValue(trimmedLine);
                    }
                    else if (trimmedLine.Contains("\"smtp_server\":"))
                    {
                        _txtSmtpServer.Text = ExtractValue(trimmedLine);
                    }
                    else if (trimmedLine.Contains("\"port\":"))
                    {
                        _txtPort.Text = ExtractValue(trimmedLine);
                    }
                    else if (trimmedLine.Contains("\"use_ssl\":"))
                    {
                        _chkUseSsl.Checked = bool.Parse(ExtractValue(trimmedLine));
                    }
                }
                
                _chkRememberPassword.Checked = true;
                Console.WriteLine("Настройки SMTP загружены в новом JSON-формате");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка парсинга JSON-формата: {ex.Message}");
            }
        }
        
        private void LoadLegacyFormatSettings(string content)
        {
            try
            {
                string[] lines = content.Split('\n');
                if (lines.Length >= 5)
                {
                    _txtEmail.Text = lines[0].Trim();
                    _txtPassword.Text = lines[1].Trim();
                    _txtSmtpServer.Text = lines[2].Trim();
                    _txtPort.Text = lines[3].Trim();
                    _chkUseSsl.Checked = bool.Parse(lines[4].Trim());
                    _chkRememberPassword.Checked = true;
                }
                Console.WriteLine("Настройки SMTP загружены в старом формате");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка парсинга старого формата: {ex.Message}");
            }
        }
        
        private string ExtractValue(string line)
        {
            try
            {
                int startIndex = line.IndexOf(':') + 1;
                string value = line.Substring(startIndex).Trim();
                
                // Убираем запятую в конце
                if (value.EndsWith(","))
                    value = value.Substring(0, value.Length - 1);
                
                // Убираем кавычки
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value.Substring(1, value.Length - 2);
                
                return value;
            }
            catch
            {
                return "";
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Принудительно создаем сетевую папку Settings
                string networkFolder = @"\\DESKTOP-56FV0G8\Users\Exest\3D Objects\Server\Shared\Net Control Hub\Settings";
                
                if (!Directory.Exists(networkFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(networkFolder);
                        Console.WriteLine($"Создана сетевая папка Settings: {networkFolder}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось создать сетевую папку Settings: {ex.Message}\n\nУбедитесь, что у вас есть права доступа к сетевой папке.", 
                            "Ошибка создания папки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Создаем красивый JSON-подобный формат
                string jsonContent = $@"{{
    ""smtp_settings"": {{
        ""email"": ""{_txtEmail.Text.Trim()}"",
        ""password"": ""{_txtPassword.Text}"",
        ""smtp_server"": ""{_txtSmtpServer.Text}"",
        ""port"": {_txtPort.Text},
        ""use_ssl"": {_chkUseSsl.Checked.ToString().ToLower()},
        ""last_updated"": ""{DateTime.Now:yyyy-MM-dd HH:mm:ss}"",
        ""created_by"": ""NetControl Hub"",
        ""version"": ""1.0""
    }}
}}";

                File.WriteAllText(SettingsPath, jsonContent);
                
                // Показываем пользователю где сохранены настройки
                MessageBox.Show($"Настройки SMTP успешно сохранены в сетевой папке!\n\nПуть: {SettingsPath}", 
                    "Настройки сохранены", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек в сетевую папку: {ex.Message}\n\nУбедитесь, что сетевая папка доступна и у вас есть права на запись.", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateSmtpSettings()
        {
            if (_txtEmail == null || string.IsNullOrEmpty(_txtEmail.Text) || !_txtEmail.Text.Contains("@"))
                return;

            string domain = _txtEmail.Text.Split('@')[1].ToLower();

            switch (domain)
            {
                case "gmail.com":
                    _txtSmtpServer.Text = "smtp.gmail.com";
                    _txtPort.Text = "587";
                    _chkUseSsl.Checked = true;
                    break;
                
                case "mail.ru":
                    _txtSmtpServer.Text = "smtp.mail.ru";
                    _txtPort.Text = "587";
                    _chkUseSsl.Checked = true;
                    break;
                
                case "yandex.ru":
                case "yandex.com":
                    _txtSmtpServer.Text = "smtp.yandex.ru";
                    _txtPort.Text = "587";
                    _chkUseSsl.Checked = true;
                    break;
                
                case "outlook.com":
                case "hotmail.com":
                    _txtSmtpServer.Text = "smtp-mail.outlook.com";
                    _txtPort.Text = "587";
                    _chkUseSsl.Checked = true;
                    break;
                
                default:
                    _txtSmtpServer.Text = "smtp.gmail.com";
                    _txtPort.Text = "587";
                    _chkUseSsl.Checked = true;
                    break;
            }
        }
    }
}
