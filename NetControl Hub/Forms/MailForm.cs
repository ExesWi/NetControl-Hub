using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;
using NetControl_Hub.Models;

namespace NetControl_Hub.Forms
{
    public partial class MailForm : Form
    {
        private readonly User _currentUser;
        private List<EmailAccount> _emailAccounts;
        private List<EmailContact> _contacts;
        
        // Ссылки на элементы управления
        private ComboBox _cboFrom;
        private ListBox _lstRecipients;
        private TextBox _txtSubject;
        private TextBox _txtMessage;

        public MailForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadEmailAccounts();
            LoadContacts();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.DoubleBuffered = true;

            // Основные цвета
            Color primaryColor = Color.FromArgb(70, 130, 180);
            Color secondaryColor = Color.FromArgb(224, 224, 224);
            Color backgroundColor = Color.FromArgb(250, 250, 252);
            Color textColor = Color.FromArgb(55, 71, 79);

            // Настройка формы
            this.Text = "Почта - NetControl Hub";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = backgroundColor;
            this.Font = new Font("Segoe UI", 9);

            // Заголовок
            var lblTitle = new Label
            {
                Text = "📧 Отправка почты",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = primaryColor,
                Dock = DockStyle.Top,
                Height = 50,
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

            // Панель отправителя
            var senderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 0, 0, 10)
            };

            var lblFrom = new Label
            {
                Text = "От кого:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(10, 15),
                Size = new Size(80, 20)
            };

            var cboFrom = new ComboBox
            {
                Name = "cboFrom",
                Location = new Point(100, 12),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var btnAddSender = new Button
            {
                Text = "➕ Добавить",
                Location = new Point(420, 12),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };

            senderPanel.Controls.AddRange(new Control[] { lblFrom, cboFrom, btnAddSender });

            // Панель получателей
            var recipientsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 0, 0, 10)
            };

            var lblTo = new Label
            {
                Text = "Кому:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(10, 15),
                Size = new Size(80, 20)
            };

            var lstRecipients = new ListBox
            {
                Name = "lstRecipients",
                Location = new Point(100, 12),
                Size = new Size(300, 80),
                Font = new Font("Segoe UI", 10),
                SelectionMode = SelectionMode.MultiExtended
            };

            var btnAddRecipient = new Button
            {
                Text = "➕ Ввести email",
                Location = new Point(420, 12),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };

            var btnRemoveRecipient = new Button
            {
                Text = "➖ Удалить",
                Location = new Point(420, 45),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };

            recipientsPanel.Controls.AddRange(new Control[] { lblTo, lstRecipients, btnAddRecipient, btnRemoveRecipient });

            // Панель темы
            var subjectPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 0, 0, 10)
            };

            var lblSubject = new Label
            {
                Text = "Тема:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(10, 20),
                Size = new Size(80, 20)
            };

            var txtSubject = new TextBox
            {
                Name = "txtSubject",
                Location = new Point(100, 17),
                Size = new Size(420, 25),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            subjectPanel.Controls.AddRange(new Control[] { lblSubject, txtSubject });

            // Панель текста письма
            var messagePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 0, 0, 10)
            };

            var lblMessage = new Label
            {
                Text = "Текст письма:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(10, 10),
                Size = new Size(100, 20)
            };

            var txtMessage = new TextBox
            {
                Name = "txtMessage",
                Location = new Point(10, 35),
                Size = new Size(520, 200),
                Font = new Font("Segoe UI", 10),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            messagePanel.Controls.AddRange(new Control[] { lblMessage, txtMessage });

            // Панель кнопок
            var buttonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            var btnSend = new Button
            {
                Text = "📤 Отправить",
                Location = new Point(350, 15),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };

            var btnCancel = new Button
            {
                Text = "❌ Отмена",
                Location = new Point(480, 15),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };

            buttonsPanel.Controls.AddRange(new Control[] { btnSend, btnCancel });

            // Сборка интерфейса
            mainPanel.Controls.Add(buttonsPanel);
            mainPanel.Controls.Add(messagePanel);
            mainPanel.Controls.Add(subjectPanel);
            mainPanel.Controls.Add(recipientsPanel);
            mainPanel.Controls.Add(senderPanel);

            this.Controls.Add(mainPanel);
            this.Controls.Add(lblTitle);

            // Обработчики событий
            btnAddSender.Click += BtnAddSender_Click;
            btnAddRecipient.Click += BtnAddRecipient_Click;
            btnRemoveRecipient.Click += BtnRemoveRecipient_Click;
            btnSend.Click += BtnSend_Click;
            btnCancel.Click += BtnCancel_Click;

            // Сохраняем ссылки на элементы управления для доступа
            _cboFrom = cboFrom;
            _lstRecipients = lstRecipients;
            _txtSubject = txtSubject;
            _txtMessage = txtMessage;

            this.ResumeLayout(false);
        }

        private void LoadEmailAccounts()
        {
            _emailAccounts = new List<EmailAccount>
            {
                new EmailAccount { Email = "admin@company.com", Name = "Администратор", IsDefault = true },
                new EmailAccount { Email = "support@company.com", Name = "Поддержка", IsDefault = false },
                new EmailAccount { Email = "info@company.com", Name = "Информация", IsDefault = false }
            };

            // Добавляем аккаунт текущего пользователя
            if (_currentUser != null)
            {
                _emailAccounts.Add(new EmailAccount 
                { 
                    Email = $"{_currentUser.Username.ToLower()}@company.com", 
                    Name = _currentUser.DisplayName, 
                    IsDefault = false 
                });
            }

            // Заполняем комбобокс
            if (_cboFrom != null)
            {
                _cboFrom.Items.Clear();
                foreach (var account in _emailAccounts)
                {
                    _cboFrom.Items.Add($"{account.Name} <{account.Email}>");
                }
                
                // Выбираем аккаунт по умолчанию
                var defaultAccount = _emailAccounts.FirstOrDefault(a => a.IsDefault);
                if (defaultAccount != null)
                {
                    _cboFrom.SelectedItem = $"{defaultAccount.Name} <{defaultAccount.Email}>";
                }
                else if (_cboFrom.Items.Count > 0)
                {
                    _cboFrom.SelectedIndex = 0;
                }
            }
        }

        private void LoadContacts()
        {
            _contacts = new List<EmailContact>
            {
                new EmailContact { Email = "director@company.com", Name = "Директор" },
                new EmailContact { Email = "manager@company.com", Name = "Менеджер" },
                new EmailContact { Email = "hr@company.com", Name = "HR отдел" },
                new EmailContact { Email = "it@company.com", Name = "IT отдел" },
                new EmailContact { Email = "sales@company.com", Name = "Отдел продаж" },
                new EmailContact { Email = "support@company.com", Name = "Техподдержка" }
            };

            // Добавляем контакты сотрудников
            for (int i = 1; i <= 10; i++)
            {
                _contacts.Add(new EmailContact 
                { 
                    Email = $"employee{i}@company.com", 
                    Name = $"Сотрудник {i}" 
                });
            }
        }

        private void BtnAddSender_Click(object sender, EventArgs e)
        {
            using (var form = new AddEmailAccountForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // Показываем форму настройки SMTP
                    using (var smtpForm = new SmtpSettingsForm(form.Email))
                    {
                        if (smtpForm.ShowDialog() == DialogResult.OK)
                        {
                            var newAccount = new EmailAccount
                            {
                                Email = form.Email,
                                Name = form.AccountName,
                                IsDefault = false,
                                SmtpSettings = new SmtpSettings
                                {
                                    Server = smtpForm.SmtpServer,
                                    Port = smtpForm.SmtpPort,
                                    UseSsl = smtpForm.UseSsl,
                                    Username = smtpForm.Email,
                                    Password = smtpForm.Password
                                }
                            };
                            
                            _emailAccounts.Add(newAccount);
                            
                            if (_cboFrom != null)
                            {
                                _cboFrom.Items.Add($"{newAccount.Name} <{newAccount.Email}>");
                                _cboFrom.SelectedItem = $"{newAccount.Name} <{newAccount.Email}>";
                            }
                        }
                    }
                }
            }
        }

        private void BtnAddRecipient_Click(object sender, EventArgs e)
        {
            using (var form = new AddRecipientForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (_lstRecipients != null)
                    {
                        string displayText = form.Email;
                        if (!string.IsNullOrWhiteSpace(form.RecipientName))
                        {
                            displayText = $"{form.RecipientName} <{form.Email}>";
                        }
                        
                        if (!_lstRecipients.Items.Contains(displayText))
                        {
                            _lstRecipients.Items.Add(displayText);
                        }
                    }
                }
            }
        }

        private void BtnRemoveRecipient_Click(object sender, EventArgs e)
        {
            if (_lstRecipients != null && _lstRecipients.SelectedItems.Count > 0)
            {
                var selectedItems = new List<object>();
                foreach (var item in _lstRecipients.SelectedItems)
                {
                    selectedItems.Add(item);
                }
                
                foreach (var item in selectedItems)
                {
                    _lstRecipients.Items.Remove(item);
                }
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            // Проверяем заполнение полей
            if (_cboFrom?.SelectedItem == null)
            {
                MessageBox.Show("Выберите отправителя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_lstRecipients?.Items.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одного получателя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtSubject?.Text))
            {
                MessageBox.Show("Введите тему письма!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtMessage?.Text))
            {
                MessageBox.Show("Введите текст письма!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Формируем данные для отправки
            string from = _cboFrom.SelectedItem.ToString();
            var recipients = new List<string>();
            foreach (var item in _lstRecipients.Items)
            {
                recipients.Add(item.ToString());
            }
            string subject = _txtSubject.Text;
            string message = _txtMessage.Text;

            // Показываем предварительный просмотр
            string preview = $"От: {from}\n";
            preview += $"Кому: {string.Join(", ", recipients)}\n";
            preview += $"Тема: {subject}\n\n";
            preview += $"Сообщение:\n{message}";

            var result = MessageBox.Show(
                $"Предварительный просмотр письма:\n\n{preview}\n\nОтправить письмо?",
                "Подтверждение отправки",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Отправляем письмо
                    SendEmail(from, recipients, subject, message);
                    
                    MessageBox.Show(
                        "Письмо успешно отправлено!\n\n" +
                        $"От: {from}\n" +
                        $"Кому: {string.Join(", ", recipients)}\n" +
                        $"Тема: {subject}",
                        "Успех",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка отправки письма:\n{ex.Message}\n\nПроверьте настройки SMTP сервера.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void SendEmail(string from, List<string> recipients, string subject, string message)
        {
            // Извлекаем email отправителя из строки "Имя <email>"
            string senderEmail = ExtractEmailFromDisplayName(from);
            
            // Находим аккаунт с настройками SMTP
            var account = _emailAccounts.FirstOrDefault(a => a.Email == senderEmail);
            if (account?.SmtpSettings == null)
            {
                throw new InvalidOperationException("Настройки SMTP не найдены для данного email. Добавьте аккаунт заново.");
            }
            
            using (var client = new SmtpClient(account.SmtpSettings.Server, account.SmtpSettings.Port))
            {
                client.EnableSsl = account.SmtpSettings.UseSsl;
                client.Credentials = new NetworkCredential(account.SmtpSettings.Username, account.SmtpSettings.Password);
                client.Timeout = 10000; // 10 секунд

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderEmail, ExtractNameFromDisplayName(from));
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = false;

                // Добавляем получателей
                foreach (var recipient in recipients)
                {
                    string recipientEmail = ExtractEmailFromDisplayName(recipient);
                    string recipientName = ExtractNameFromDisplayName(recipient);
                    mailMessage.To.Add(new MailAddress(recipientEmail, recipientName));
                }

                client.Send(mailMessage);
            }
        }

        private string ExtractEmailFromDisplayName(string displayName)
        {
            // Извлекаем email из строки вида "Имя <email@domain.com>"
            int startIndex = displayName.IndexOf('<');
            int endIndex = displayName.IndexOf('>');
            
            if (startIndex >= 0 && endIndex > startIndex)
            {
                return displayName.Substring(startIndex + 1, endIndex - startIndex - 1);
            }
            
            // Если формат не найден, возвращаем как есть
            return displayName;
        }

        private string ExtractNameFromDisplayName(string displayName)
        {
            // Извлекаем имя из строки вида "Имя <email@domain.com>"
            int endIndex = displayName.IndexOf('<');
            
            if (endIndex > 0)
            {
                return displayName.Substring(0, endIndex).Trim();
            }
            
            // Если формат не найден, возвращаем пустую строку
            return string.Empty;
        }



        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EmailAccount
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public SmtpSettings SmtpSettings { get; set; }
    }

    public class EmailContact
    {
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
