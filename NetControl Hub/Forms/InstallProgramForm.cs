using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using NetControl_Hub.Services;
using NetControl_Hub.Models;

namespace NetControl_Hub.Forms
{
    public partial class InstallProgramForm : Form
    {
        private readonly ProgramService _programService;
        private readonly User _currentUser;
        
        private TextBox txtProgramName;
        private TextBox txtSourcePath;
        private TextBox txtInstallPath;
        private TextBox txtShortcutName;
        private Button btnBrowseSource;
        private Button btnBrowseInstall;
        private Button btnBrowseExe;
        private Button btnInstall;
        private Button btnCancel;
        private Label lblProgramName;
        private Label lblSourcePath;
        private Label lblInstallPath;
        private Label lblShortcutName;
        private Label lblTitle;

        public InstallProgramForm(ProgramService programService, User currentUser)
        {
            _programService = programService;
            _currentUser = currentUser;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Title Label
            lblTitle = new Label
            {
                Text = "Добавление новой программы",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Program Name Label
            lblProgramName = new Label
            {
                Text = "Название программы:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 70),
                Size = new Size(200, 25)
            };

            // Program Name TextBox
            txtProgramName = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Source Path Label
            lblSourcePath = new Label
            {
                Text = "Путь к исходной папке:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 140),
                Size = new Size(200, 25)
            };

            // Source Path TextBox
            txtSourcePath = new TextBox
            {
                Location = new Point(20, 170),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Browse Source Button
            btnBrowseSource = new Button
            {
                Text = "Обзор",
                Location = new Point(440, 170),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // Install Path Label
            lblInstallPath = new Label
            {
                Text = "Путь для установки:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 210),
                Size = new Size(200, 25)
            };

            // Install Path TextBox
            txtInstallPath = new TextBox
            {
                Location = new Point(20, 240),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Browse Install Button
            btnBrowseInstall = new Button
            {
                Text = "Обзор",
                Location = new Point(440, 240),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // Shortcut Name Label
            lblShortcutName = new Label
            {
                Text = "Путь к .exe файлу программы:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 280),
                Size = new Size(250, 25)
            };

            // Shortcut Name TextBox
            txtShortcutName = new TextBox
            {
                Location = new Point(20, 310),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Browse Exe Button
            btnBrowseExe = new Button
            {
                Text = "Обзор",
                Location = new Point(440, 310),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // Install Button
            btnInstall = new Button
            {
                Text = "Добавить программу",
                Location = new Point(200, 360),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // Cancel Button
            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(370, 360),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            // Event handlers
            btnBrowseSource.Click += BtnBrowseSource_Click;
            btnBrowseInstall.Click += BtnBrowseInstall_Click;
            btnBrowseExe.Click += BtnBrowseExe_Click;
            btnInstall.Click += BtnInstall_Click;
            btnCancel.Click += BtnCancel_Click;

            // Add controls
            this.Controls.AddRange(new Control[] 
            { 
                lblTitle, lblProgramName, txtProgramName,
                lblSourcePath, txtSourcePath, btnBrowseSource,
                lblInstallPath, txtInstallPath, btnBrowseInstall,
                lblShortcutName, txtShortcutName, btnBrowseExe, btnInstall, btnCancel
            });

            // Form properties
            this.Text = "Добавление программы - NetControl Hub";
            this.Size = new Size(560, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            this.ResumeLayout(false);
        }

        private void BtnBrowseSource_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите исходную папку с программой";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtSourcePath.Text = folderDialog.SelectedPath;
                    
                    // Автоматически заполняем название программы
                    if (string.IsNullOrEmpty(txtProgramName.Text))
                    {
                        txtProgramName.Text = Path.GetFileName(folderDialog.SelectedPath);
                    }
                }
            }
        }

        private void BtnBrowseInstall_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите папку для установки";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInstallPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void BtnBrowseExe_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите .exe файл программы";
                openFileDialog.Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtShortcutName.Text = openFileDialog.FileName;
                }
            }
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProgramName.Text) ||
                string.IsNullOrEmpty(txtSourcePath.Text) || 
                string.IsNullOrEmpty(txtInstallPath.Text) || 
                string.IsNullOrEmpty(txtShortcutName.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(txtSourcePath.Text))
            {
                MessageBox.Show("Исходная папка не найдена!", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(txtShortcutName.Text))
            {
                MessageBox.Show("Файл .exe не найден по указанному пути!", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                bool success = _programService.InstallProgram(
                    txtProgramName.Text,
                    txtSourcePath.Text,
                    txtInstallPath.Text,
                    txtShortcutName.Text,
                    _currentUser.Username);

                if (success)
                {
                    MessageBox.Show("Программа успешно добавлена!", 
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка при добавлении: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
