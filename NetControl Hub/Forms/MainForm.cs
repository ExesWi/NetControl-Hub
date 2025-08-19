 using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using NetControl_Hub.Models;
using NetControl_Hub.Services;
using NetControl_Hub.Controls;

namespace NetControl_Hub.Forms
{
    public partial class MainForm : Form
    {
        private readonly User _currentUser;
        private readonly ProgramService _programService;
        private readonly ComputerService _computerService;
        private Label lblWelcome;
        private Label lblUserInfo;
        private Panel mainPanel;
        private Panel userInfoPanel;
        private Panel programsPanel;
        private FlowLayoutPanel flowPrograms;
        private List<ProgramCard> _programCards;
        
        // Navigation Drawer
        private Panel navigationDrawer;
        private Button btnToggleDrawer;
        private bool isDrawerOpen = false;
        private Timer drawerTimer;

        public MainForm(User user)
        {
            _currentUser = user;
            _programService = new ProgramService();
            _computerService = new ComputerService(_programService.ProgramsDataPath);
            _programCards = new List<ProgramCard>();
            
            // Регистрируем текущий компьютер
            try
            {
                _computerService.RegisterComputer(
                    Environment.MachineName,
                    _currentUser.Username,
                    _currentUser.Role.ToString()
                );
                Console.WriteLine($"Компьютер {Environment.MachineName} зарегистрирован успешно");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка регистрации компьютера: {ex.Message}");
                MessageBox.Show($"Ошибка регистрации компьютера: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            InitializeComponent();
            SetupForm();
            LoadPrograms();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.DoubleBuffered = true; // Включаем двойную буферизацию для плавной анимации

            // Инициализация таймера для анимации
            drawerTimer = new Timer();

            // Основные цвета новой схемы
            Color primaryColor = Color.FromArgb(70, 130, 180);    // Стальной синий
            Color secondaryColor = Color.FromArgb(224, 224, 224); // Светло-серый
            Color accentColor = Color.FromArgb(0, 150, 136);      // Акцентный бирюзовый
            Color backgroundColor = Color.FromArgb(250, 250, 252); // Фоновый белый
            Color textColor = Color.FromArgb(55, 71, 79);         // Текст темно-серый

            // Main Panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backgroundColor
            };

            // User Info Panel - полностью переработан
            userInfoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };

            // Toggle Drawer Button - улучшенный дизайн (иконка)
            btnToggleDrawer = new Button
            {
                Size = new Size(40, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = primaryColor,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Location = new Point(10, 15)
            };
            btnToggleDrawer.FlatAppearance.BorderSize = 0;
            btnToggleDrawer.FlatAppearance.MouseOverBackColor = secondaryColor;
            // Показываем текстовую иконку гамбургера, без зависимостей от ресурсов
            btnToggleDrawer.Text = "☰";
            btnToggleDrawer.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnToggleDrawer.TextAlign = ContentAlignment.MiddleCenter;

            // User Info Label - улучшенная типографика
            lblUserInfo = new Label
            {
                AutoSize = true,
                Text = $"{_currentUser.DisplayName} | {GetRoleDisplayName(_currentUser.Role)}",
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = textColor,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(this.Width - 250, 25)
            };

            // Убираем правый квадрат-аватар (не используем)

            // Navigation Drawer - полностью переработан
            navigationDrawer = new Panel
            {
                Width = 220,
                Height = this.Height,
                BackColor = Color.White,
                Location = new Point(-220, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(20, 20, 10, 20)
            };

            // Welcome Label - улучшенная типографика
            lblWelcome = new Label
            {
                Text = "Добро пожаловать в NetControl Hub",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = primaryColor,
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(30, 0, 0, 0),
                BackColor = Color.White
            };

            // Programs Panel - улучшенный дизайн
            programsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backgroundColor,
                Padding = new Padding(25, 20, 25, 20)
            };

            // Programs FlowLayoutPanel - добавлены отступы
            flowPrograms = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Добавление элементов
            userInfoPanel.Controls.Add(lblUserInfo);
            
            programsPanel.Controls.Add(flowPrograms);
            
            mainPanel.Controls.Add(programsPanel);
            mainPanel.Controls.Add(lblWelcome);
            mainPanel.Controls.Add(userInfoPanel);
            mainPanel.Controls.Add(navigationDrawer);
            mainPanel.Controls.Add(btnToggleDrawer); // Кнопка в mainPanel, а не в userInfoPanel
            
            // Убеждаемся, что кнопка toggle всегда поверх всех элементов
            btnToggleDrawer.BringToFront();

            // Настройка формы
            this.Controls.Add(mainPanel);
            this.Text = $"NetControl Hub • {_currentUser.DisplayName}";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = backgroundColor;
            this.Font = new Font("Segoe UI", 9);

            // Обработчики событий
            btnToggleDrawer.Click += BtnToggleDrawer_Click;
            this.Resize += MainForm_Resize;

            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Инициализация навигационного меню
            CreateModernNavigationMenu();
            
            // Стандартные обработчики
            this.FormClosing += MainForm_FormClosing;
            this.Load += MainForm_Load;
        }

        private void CreateModernNavigationMenu()
        {
            // Цвета для меню
            Color primaryColor = Color.FromArgb(70, 130, 180);
            Color hoverColor = Color.FromArgb(240, 248, 255); // AliceBlue
            
            // Контейнер меню
            var menuContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };

            // Заголовок меню
            var menuHeader = new Label
            {
                Text = "Меню управления",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = primaryColor,
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Разделитель
            var separator = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(240, 240, 240),
                Margin = new Padding(0, 5, 0, 15)
            };

                            // Элементы меню
                var menuItems = new List<(string icon, string text, bool visible)>
                {
                    ("🏠", "Главная", true),
                    ("➕", "Добавить программу", _currentUser.Role == UserRole.Developer),
                    ("🔄", "Обновить", true),
                    ("🧹", "Очистить дубликаты", _currentUser.Role == UserRole.Developer),
                    ("🖥️", "Управление компьютерами", _currentUser.Role == UserRole.Developer),
                    ("📧", "Почта", true),
                    ("⚙️", "Настройки", true),
                    ("🚪", "Выйти", true)
                };

            // Контейнер для элементов
            var itemsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = menuItems.Count,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(5, 0, 5, 20)
            };

            // Создание элементов меню
            for (int i = 0; i < menuItems.Count; i++)
            {
                var item = menuItems[i];
                if (!item.visible) continue;

                var menuItem = new Button
                {
                    Tag = i,
                    Text = "  " + item.text,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Height = 45,
                    Dock = DockStyle.Fill,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(90, 90, 90),
                    BackColor = Color.Transparent,
                    FlatAppearance = {
                        BorderSize = 0,
                        MouseOverBackColor = hoverColor
                    }
                };

                menuItem.Click += (s, e) => HandleMenuItemClick(Convert.ToInt32(menuItem.Tag));
                itemsPanel.Controls.Add(menuItem);
                itemsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            }

            // Сборка меню
            menuContainer.Controls.Add(itemsPanel);
            menuContainer.Controls.Add(separator);
            menuContainer.Controls.Add(menuHeader);
            
            navigationDrawer.Controls.Add(menuContainer);
        }

        private void HandleMenuItemClick(int index)
        {
            switch (index)
            {
                case 0: break; // Главная
                case 1: BtnInstallProgram_Click(this, EventArgs.Empty); break;
                case 2: BtnRefreshPrograms_Click(this, EventArgs.Empty); break;
                case 3: btnCleanupDuplicates_Click(this, EventArgs.Empty); break;
                case 4: BtnManageComputers_Click(this, EventArgs.Empty); break;
                case 5: BtnMail_Click(this, EventArgs.Empty); break;
                case 6:
                    using (var settings = new SettingsForm(new AuthService()))
                    {
                        settings.ShowDialog(this);
                    }
                    break;
                case 7: BtnLogout_Click(this, EventArgs.Empty); break;
            }
        }

        private void BtnToggleDrawer_Click(object sender, EventArgs e)
        {
            ToggleNavigationDrawer();
        }

        private void ToggleNavigationDrawer()
        {
            // Очищаем предыдущие обработчики таймера
            drawerTimer.Tick -= DrawerTimer_Tick;
            
            Console.WriteLine($"ToggleNavigationDrawer: isDrawerOpen={isDrawerOpen}, Left={navigationDrawer.Left}");
            
            if (isDrawerOpen)
            {
                // Анимация закрытия
                Console.WriteLine("Начинаем анимацию закрытия");
                drawerTimer.Interval = 15;
                drawerTimer.Tick += DrawerTimer_Tick;
                drawerTimer.Start();
            }
            else
            {
                // Анимация открытия
                Console.WriteLine("Начинаем анимацию открытия");
                navigationDrawer.BringToFront();
                drawerTimer.Interval = 15;
                drawerTimer.Tick += DrawerTimer_Tick;
                drawerTimer.Start();
            }
        }

        private void DrawerTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine($"DrawerTimer_Tick: Left={navigationDrawer.Left}, isDrawerOpen={isDrawerOpen}");
            
            // Убеждаемся, что кнопка toggle всегда видна
            btnToggleDrawer.BringToFront();
            
            // Определяем направление анимации по состоянию isDrawerOpen
            if (!isDrawerOpen)
            {
                // Открытие - двигаем вправо (из -220 к 0)
                navigationDrawer.Left += 25;
                Console.WriteLine($"Открытие: Left={navigationDrawer.Left}");
                if (navigationDrawer.Left >= 0)
                {
                    drawerTimer.Stop();
                    isDrawerOpen = true;
                    navigationDrawer.Left = 0; // Фиксируем позицию
                    Console.WriteLine("Drawer открыт");
                }
            }
            else
            {
                // Закрытие - двигаем влево (из 0 к -220)
                navigationDrawer.Left -= 25;
                Console.WriteLine($"Закрытие: Left={navigationDrawer.Left}");
                if (navigationDrawer.Left <= -220)
                {
                    drawerTimer.Stop();
                    isDrawerOpen = false;
                    navigationDrawer.Left = -220; // Фиксируем позицию
                    Console.WriteLine("Drawer закрыт");
                }
            }
        }

        private string GetRoleDisplayName(UserRole role)
        {
            switch (role)
            {
                case UserRole.Developer:
                    return "Разработчик";
                case UserRole.Director:
                    return "Директор";
                case UserRole.Employee:
                    return "Сотрудник";
                default:
                    return "Неизвестно";
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?", 
                "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Здесь можно добавить логику сохранения данных перед закрытием
            var result = MessageBox.Show("Вы уверены, что хотите закрыть приложение?", 
                "Подтверждение закрытия", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void LoadPrograms()
        {
            // Очищаем существующие карточки
            flowPrograms.Controls.Clear();
            
            // Получаем список программ
            var programs = _programService.GetAllPrograms();
            
            // Создаем карточки для каждой программы
            foreach (var program in programs)
            {
                var card = new ProgramCard(program, _currentUser);
                card.InstallRequested += OnInstallRequested;
                card.UpdateRequested += OnUpdateRequested;
                card.UninstallRequested += OnUninstallRequested;
                card.AboutRequested += OnAboutRequested;
                card.DeleteRequested += OnDeleteRequested;
                
                flowPrograms.Controls.Add(card);
            }
            
            // Обновляем статус сети
            ShowNetworkStatus();
        }

        private void OnInstallRequested(object sender, ProgramInfo program)
        {
            // Автоматическая установка программы на диск C:
            string installPath = @"C:\" + program.Name;
            
            try
            {
                // Показываем прогресс установки
                using (var progressForm = new Form())
                {
                    progressForm.Text = "Установка программы";
                    progressForm.Size = new Size(400, 150);
                    progressForm.StartPosition = FormStartPosition.CenterParent;
                    progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    progressForm.MaximizeBox = false;
                    progressForm.MinimizeBox = false;

                    var lblProgress = new Label
                    {
                        Text = $"Установка {program.Name} в корень диска C:...",
                        Location = new Point(20, 20),
                        Size = new Size(350, 20),
                        Font = new Font("Segoe UI", 10)
                    };

                    var progressBar = new ProgressBar
                    {
                        Location = new Point(20, 50),
                        Size = new Size(350, 25),
                        Style = ProgressBarStyle.Marquee,
                        MarqueeAnimationSpeed = 30
                    };

                    progressForm.Controls.AddRange(new Control[] { lblProgress, progressBar });
                    progressForm.Show();

                    // Запускаем установку в отдельном потоке
                    var installTask = Task.Run(() =>
                    {
                        return _programService.InstallProgramOnLocalComputer(program.Id, installPath);
                    });

                    // Ждем завершения установки
                    bool success = installTask.Result;

                    progressForm.Close();

                    if (success)
                    {
                        MessageBox.Show($"Программа {program.Name} успешно установлена в корень диска C:!\n\nПуть: {installPath}", 
                            "Установка завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Обновляем карточки
                        LoadPrograms();
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка при установке программы {program.Name}", 
                            "Ошибка установки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке {program.Name}: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnUpdateRequested(object sender, ProgramInfo program)
        {
            // Здесь можно добавить логику обновления
            MessageBox.Show("Функция обновления будет добавлена позже.", 
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnUninstallRequested(object sender, ProgramInfo program)
        {
            var confirm = MessageBox.Show(
                string.Format("Вы уверены, что хотите удалить программу '{0}' с вашего компьютера?", program.Name),
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                if (_programService.UninstallProgram(program.Id))
                {
                    MessageBox.Show("Программа успешно удалена с вашего компьютера!", 
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPrograms();
                }
            }
        }

        private void OnAboutRequested(object sender, ProgramInfo program)
        {
            string aboutInfo = string.Format(
                "Название: {0}\n" +
                "Версия: {1}\n" +
                "Исходная папка: {2}\n" +
                "Путь для установки: {3}\n" +
                "EXE файл: {4}\n" +
                "Добавил: {5}\n" +
                "Дата добавления: {6}\n" +
                "Статус: {7}",
                program.Name,
                program.Version,
                program.SourcePath,
                program.InstallPath,
                program.DesktopShortcutPath,
                program.InstalledBy,
                program.InstallDate.ToString("dd.MM.yyyy HH:mm"),
                GetStatusDisplayName(program.Status));

            MessageBox.Show(aboutInfo, "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnDeleteRequested(object sender, ProgramInfo program)
        {
            if (_currentUser.Role != UserRole.Developer)
            {
                MessageBox.Show("Только разработчики могут удалять программы из системы!", 
                    "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                string.Format("Вы уверены, что хотите удалить программу '{0}' из системы?\n\nЭто действие нельзя отменить!", program.Name),
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                if (_programService.DeleteProgramFromSystem(program.Id))
                {
                    MessageBox.Show("Программа успешно удалена из системы!", 
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Обновляем интерфейс - перезагружаем список программ
                    LoadPrograms();
                }
                else
                {
                    MessageBox.Show("Не удалось удалить программу из системы!", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetStatusDisplayName(ProgramStatus status)
        {
            switch (status)
            {
                case ProgramStatus.Installed:
                    return "Установлена";
                case ProgramStatus.Outdated:
                    return "Требует обновления";
                case ProgramStatus.NotInstalled:
                    return "Не установлена";
                default:
                    return "Неизвестно";
            }
        }

        private void BtnInstallProgram_Click(object sender, EventArgs e)
        {
            if (_currentUser.Role != UserRole.Developer)
            {
                MessageBox.Show("Только разработчики могут устанавливать программы!", 
                    "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var installForm = new InstallProgramForm(_programService, _currentUser);
            if (installForm.ShowDialog() == DialogResult.OK)
            {
                LoadPrograms();
            }
        }

        private void BtnRefreshPrograms_Click(object sender, EventArgs e)
        {
            _programService.CheckForUpdates();
            LoadPrograms();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Устанавливаем заголовок окна с информацией о пользователе
            this.Text = string.Format("NetControl Hub - {0} ({1})", _currentUser.DisplayName, _currentUser.Role);
            
            // Инициализируем drawer в закрытом состоянии
            navigationDrawer.Left = -220;
            isDrawerOpen = false;
            
            // Убеждаемся, что кнопка toggle всегда видна и поверх всех элементов
            btnToggleDrawer.BringToFront();
            
            // Загружаем программы
            LoadPrograms();
            
            // Показываем статус сети
            ShowNetworkStatus();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Обновление позиции элементов при изменении размера
            lblUserInfo.Location = new Point(
                this.Width - lblUserInfo.Width - 50, 
                lblUserInfo.Location.Y
            );
            
            // Убеждаемся, что кнопка toggle всегда видна
            btnToggleDrawer.BringToFront();
        }

        private void btnCleanupDuplicates_Click(object sender, EventArgs e)
        {
            // Очищаем дублированные программы
            _programService.CleanupDuplicatePrograms();
            
            // Перезагружаем список программ
            LoadPrograms();
        }

        private void BtnManageComputers_Click(object sender, EventArgs e)
        {
            if (_currentUser.Role != UserRole.Developer)
            {
                MessageBox.Show("Только разработчики могут управлять компьютерами!", 
                    "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var computersForm = new ComputersForm(_currentUser, _computerService);
            computersForm.ShowDialog();
        }

        private void ShowNetworkStatus()
        {
            if (_programService.IsNetworkMode())
            {
                this.Text += " [СЕТЕВОЙ РЕЖИМ]";
                // Можно добавить иконку или цветовую индикацию
            }
            else
            {
                this.Text += " [ЛОКАЛЬНЫЙ РЕЖИМ]";
            }
        }

        private void BtnMail_Click(object sender, EventArgs e)
        {
            // Открываем форму для работы с почтой
            try
            {
                using (var mailForm = new MailForm(_currentUser))
                {
                    mailForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка открытия почты: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
