using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using NetControl_Hub.Models;

namespace NetControl_Hub.Controls
{
    public class ProgramCard : Panel
    {
        private ProgramInfo _program;
        private User _currentUser;
        private Button _btnInstall;
        private Button _btnUpdate;
        private Button _btnUninstall;
        private Label _lblName;
        private Label _lblVersion;
        private Label _lblStatus;
        private Label _lblDate;
        private Label _lblInstalledBy;
        private ContextMenuStrip _contextMenu;

        public event EventHandler<ProgramInfo> InstallRequested;
        public event EventHandler<ProgramInfo> UpdateRequested;
        public event EventHandler<ProgramInfo> UninstallRequested;
        public event EventHandler<ProgramInfo> AboutRequested;
        public event EventHandler<ProgramInfo> DeleteRequested;

        public ProgramCard(ProgramInfo program, User currentUser)
        {
            _program = program;
            _currentUser = currentUser;
            InitializeComponents();
            UpdateCardAppearance();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(320, 350);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.None;
            this.Margin = new Padding(15, 20, 15, 15);

            // Название программы
            _lblName = new Label();
            _lblName.Text = _program.Name;
            _lblName.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            _lblName.ForeColor = Color.FromArgb(55, 71, 79); // текст темно-серый
            _lblName.TextAlign = ContentAlignment.MiddleLeft;
            _lblName.Location = new Point(20, 20);
            _lblName.Size = new Size(280, 30);
            _lblName.BackColor = Color.Transparent;

            // Версия
            _lblVersion = new Label();
            _lblVersion.Text = string.Format("Версия: {0}", _program.Version);
            _lblVersion.Font = new Font("Segoe UI", 10);
            _lblVersion.ForeColor = Color.FromArgb(106, 119, 142); // серый текст
            _lblVersion.TextAlign = ContentAlignment.MiddleLeft;
            _lblVersion.Location = new Point(20, 55);
            _lblVersion.Size = new Size(280, 20);
            _lblVersion.BackColor = Color.Transparent;

            // Статус
            _lblStatus = new Label();
            _lblStatus.Text = GetStatusText(_program.Status);
            _lblStatus.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            _lblStatus.ForeColor = GetStatusColor(_program.Status);
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            _lblStatus.Location = new Point(20, 80);
            _lblStatus.Size = new Size(280, 22);
            _lblStatus.BackColor = Color.Transparent;

            // Дата установки
            _lblDate = new Label();
            _lblDate.Text = _program.Status == ProgramStatus.NotInstalled ? 
                "Не установлена" : 
                string.Format("Установлена: {0}", _program.InstallDate.ToString("dd.MM.yyyy"));
            _lblDate.Font = new Font("Segoe UI", 10);
            _lblDate.ForeColor = Color.FromArgb(106, 119, 142);
            _lblDate.TextAlign = ContentAlignment.MiddleLeft;
            _lblDate.Location = new Point(20, 105);
            _lblDate.Size = new Size(280, 20);
            _lblDate.BackColor = Color.Transparent;

            // Установил
            _lblInstalledBy = new Label();
            _lblInstalledBy.Text = _program.Status == ProgramStatus.NotInstalled ? 
                string.Format("Добавил: {0}", _program.InstalledBy) : 
                string.Format("Установил: {0}", _program.InstalledBy);
            _lblInstalledBy.Font = new Font("Segoe UI", 10);
            _lblInstalledBy.ForeColor = Color.FromArgb(106, 119, 142);
            _lblInstalledBy.TextAlign = ContentAlignment.MiddleLeft;
            _lblInstalledBy.Location = new Point(20, 130);
            _lblInstalledBy.Size = new Size(280, 20);
            _lblInstalledBy.BackColor = Color.Transparent;

            // Дата добавления в систему
            var lblAddedDate = new Label();
            lblAddedDate.Text = string.Format("Добавлена: {0}", _program.InstallDate.ToString("dd.MM.yyyy"));
            lblAddedDate.Font = new Font("Segoe UI", 10);
            lblAddedDate.ForeColor = Color.FromArgb(106, 119, 142);
            lblAddedDate.TextAlign = ContentAlignment.MiddleLeft;
            lblAddedDate.Location = new Point(20, 155);
            lblAddedDate.Size = new Size(280, 20);
            lblAddedDate.BackColor = Color.Transparent;

            // Путь к .exe файлу
            var lblExePath = new Label();
            string exeFileName = "Не указан";
            if (!string.IsNullOrEmpty(_program.DesktopShortcutPath))
            {
                if (File.Exists(_program.DesktopShortcutPath))
                {
                    exeFileName = Path.GetFileName(_program.DesktopShortcutPath);
                }
                else
                {
                    exeFileName = Path.GetFileName(_program.DesktopShortcutPath);
                }
            }
            lblExePath.Text = string.Format("EXE: {0}", exeFileName);
            lblExePath.Font = new Font("Segoe UI", 10);
            lblExePath.ForeColor = Color.FromArgb(120, 130, 140);
            lblExePath.TextAlign = ContentAlignment.MiddleLeft;
            lblExePath.Location = new Point(20, 205);
            lblExePath.Size = new Size(280, 22);
            lblExePath.BackColor = Color.Transparent;

            // Кнопка установить
            _btnInstall = new Button();
            _btnInstall.Text = "Установить";
            _btnInstall.Size = new Size(100, 34);
            _btnInstall.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _btnInstall.BackColor = Color.FromArgb(70, 130, 180); // primary
            _btnInstall.ForeColor = Color.White;
            _btnInstall.FlatStyle = FlatStyle.Flat;
            _btnInstall.FlatAppearance.BorderSize = 0;
            _btnInstall.Cursor = Cursors.Hand;
            _btnInstall.Location = new Point(20, 220);
            _btnInstall.Visible = _program.Status == ProgramStatus.NotInstalled;

            // Кнопка обновить
            _btnUpdate = new Button();
            _btnUpdate.Text = "Обновить";
            _btnUpdate.Size = new Size(100, 34);
            _btnUpdate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _btnUpdate.BackColor = Color.FromArgb(0, 150, 136); // Акцентный бирюзовый
            _btnUpdate.ForeColor = Color.White;
            _btnUpdate.FlatStyle = FlatStyle.Flat;
            _btnUpdate.FlatAppearance.BorderSize = 0;
            _btnUpdate.Cursor = Cursors.Hand;
            _btnUpdate.Location = new Point(20, 220);
            _btnUpdate.Visible = _program.Status == ProgramStatus.Outdated;

            // Кнопка удалить
            _btnUninstall = new Button();
            _btnUninstall.Text = "Удалить";
            _btnUninstall.Size = new Size(100, 34);
            _btnUninstall.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _btnUninstall.BackColor = Color.FromArgb(192, 57, 43);
            _btnUninstall.ForeColor = Color.White;
            _btnUninstall.FlatStyle = FlatStyle.Flat;
            _btnUninstall.FlatAppearance.BorderSize = 0;
            _btnUninstall.Cursor = Cursors.Hand;
            _btnUninstall.Location = new Point(130, 220);
            _btnUninstall.Visible = _program.Status == ProgramStatus.Installed || _program.Status == ProgramStatus.Outdated;

            // События
            _btnInstall.Click += (s, e) => InstallRequested?.Invoke(this, _program);
            _btnUpdate.Click += (s, e) => UpdateRequested?.Invoke(this, _program);
            _btnUninstall.Click += (s, e) => UninstallRequested?.Invoke(this, _program);

            // Добавляем элементы управления
            this.Controls.AddRange(new Control[] 
            { 
                _lblName, _lblVersion, _lblStatus, _lblDate, _lblInstalledBy, lblAddedDate, lblExePath,
                _btnInstall, _btnUpdate, _btnUninstall
            });

            // Создаем контекстное меню
            CreateContextMenu();

            // Добавляем обработчик правой кнопки мыши
            this.MouseClick += ProgramCard_MouseClick;
        }

        private string GetStatusText(ProgramStatus status)
        {
            switch (status)
            {
                case ProgramStatus.Installed:
                    return "✓ Установлена";
                case ProgramStatus.Outdated:
                    return "✓ Установлена";
                case ProgramStatus.NotInstalled:
                    return "○ Не установлена";
                default:
                    return "? Неизвестно";
            }
        }

        private Color GetStatusColor(ProgramStatus status)
        {
            switch (status)
            {
                case ProgramStatus.Installed:
                    return Color.FromArgb(76, 175, 80); // Зеленый
                case ProgramStatus.Outdated:
                    return Color.FromArgb(76, 175, 80); // Зеленый (как установленная)
                case ProgramStatus.NotInstalled:
                    return Color.FromArgb(158, 158, 158); // Серый
                default:
                    return Color.White;
            }
        }

        private void UpdateCardAppearance()
        {
            // Обновляем видимость кнопок в зависимости от статуса
            _btnInstall.Visible = _program.Status == ProgramStatus.NotInstalled;
            _btnUpdate.Visible = _program.Status == ProgramStatus.Outdated;
            _btnUninstall.Visible = _program.Status == ProgramStatus.Installed || _program.Status == ProgramStatus.Outdated;

            // Обновляем статус - теперь показываем только установлена/не установлена
            _lblStatus.Text = GetStatusText(_program.Status);
            _lblStatus.ForeColor = GetStatusColor(_program.Status);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Рисуем закругленные углы
            using (var path = GetRoundedRectanglePath(this.ClientRectangle, 16))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Тень (легкая)
                using (var shadowPath = GetRoundedRectanglePath(new Rectangle(4, 6, this.Width - 8, this.Height - 8), 16))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(24, 0, 0, 0)))
                {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }

                // Основной фон (белый)
                using (var bgBrush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(bgBrush, path);
                }

                // Тонкая светлая граница
                using (var pen = new Pen(Color.FromArgb(224, 224, 224), 1f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            
            int diameter = radius * 2;
            var size = new Size(diameter, diameter);
            var arc = new Rectangle(rect.Location, size);
            
            // Верхний левый угол
            path.AddArc(arc, 180, 90);
            
            // Верхний правый угол
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            
            // Нижний правый угол
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            
            // Нижний левый угол
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            
            path.CloseFigure();
            return path;
        }

        private void CreateContextMenu()
        {
            _contextMenu = new ContextMenuStrip();
            
            // Пункт "О программе"
            var aboutItem = new ToolStripMenuItem("О программе");
            aboutItem.Click += (s, e) => AboutRequested?.Invoke(this, _program);
            _contextMenu.Items.Add(aboutItem);
            
            // Пункт "Удалить" (только для разработчиков)
            if (_currentUser != null && _currentUser.Role == UserRole.Developer)
            {
                var deleteItem = new ToolStripMenuItem("Удалить");
                deleteItem.Click += (s, e) => DeleteRequested?.Invoke(this, _program);
                _contextMenu.Items.Add(deleteItem);
            }
        }

        private void ProgramCard_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _contextMenu.Show(this, e.Location);
            }
        }

        public void UpdateProgram(ProgramInfo updatedProgram)
        {
            // Обновляем данные программы
            _program.Status = updatedProgram.Status;
            _program.Version = updatedProgram.Version;
            _program.InstallDate = updatedProgram.InstallDate;
            _program.LastUpdateDate = updatedProgram.LastUpdateDate;
            
            // Обновляем отображение
            _lblVersion.Text = string.Format("Версия: {0}", _program.Version);
            _lblDate.Text = _program.Status == ProgramStatus.NotInstalled ? 
                "Не установлена" : 
                string.Format("Установлена: {0}", _program.InstallDate.ToString("dd.MM.yyyy"));
            _lblInstalledBy.Text = _program.Status == ProgramStatus.NotInstalled ? 
                "Доступна для установки" : 
                string.Format("Установил: {0}", _program.InstalledBy);
            UpdateCardAppearance();
        }
    }
}
