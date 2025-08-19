using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using NetControl_Hub.Models;

namespace NetControl_Hub.Forms
{
    public partial class RemoteFileExplorerForm : Form
    {
        private readonly Computer _targetComputer;
        private readonly User _currentUser;
        private TreeView treeViewFiles;
        private ListView listViewFiles;
        private TextBox txtPath;
        private Button btnRefresh;
        private Button btnBack;
        private Button btnUp;
        private SplitContainer splitContainer;
        private string currentPath = "";

        public RemoteFileExplorerForm(Computer computer, User currentUser)
        {
            _targetComputer = computer;
            _currentUser = currentUser;
            InitializeComponent();
            SetupForm();
            LoadDrives();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = string.Format("Удаленный проводник - {0} ({1})", _targetComputer.Name, _targetComputer.UserName);
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Split Container
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                SplitterDistance = 300
            };

            // TreeView for drives and folders
            treeViewFiles = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // ListView for files
            listViewFiles = new ListView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            listViewFiles.Columns.Add("Имя", 200);
            listViewFiles.Columns.Add("Тип", 100);
            listViewFiles.Columns.Add("Размер", 100);
            listViewFiles.Columns.Add("Дата изменения", 150);

            // Path TextBox
            txtPath = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 25,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };

            // Buttons Panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            btnBack = new Button
            {
                Text = "← Назад",
                Size = new Size(80, 30),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnUp = new Button
            {
                Text = "↑ Вверх",
                Size = new Size(80, 30),
                Location = new Point(100, 10),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnRefresh = new Button
            {
                Text = "Обновить",
                Size = new Size(80, 30),
                Location = new Point(190, 10),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Добавляем новые кнопки для работы с файлами
            var btnOpenFile = new Button
            {
                Text = "Открыть",
                Size = new Size(80, 30),
                Location = new Point(280, 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            var btnCopyFile = new Button
            {
                Text = "Копировать",
                Size = new Size(80, 30),
                Location = new Point(370, 10),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            var btnSendFile = new Button
            {
                Text = "Отправить",
                Size = new Size(80, 30),
                Location = new Point(460, 10),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            var btnPreviewFile = new Button
            {
                Text = "Просмотр",
                Size = new Size(80, 30),
                Location = new Point(550, 10),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            buttonPanel.Controls.AddRange(new Control[] { btnBack, btnUp, btnRefresh, btnOpenFile, btnCopyFile, btnSendFile, btnPreviewFile });

            // Add controls to split container
            splitContainer.Panel1.Controls.Add(treeViewFiles);
            splitContainer.Panel2.Controls.Add(listViewFiles);

            // Add controls to form
            this.Controls.Add(splitContainer);
            this.Controls.Add(txtPath);
            this.Controls.Add(buttonPanel);

            // Event handlers
            treeViewFiles.AfterSelect += TreeViewFiles_AfterSelect;
            listViewFiles.DoubleClick += ListViewFiles_DoubleClick;
            listViewFiles.SelectedIndexChanged += ListViewFiles_SelectedIndexChanged;
            btnBack.Click += BtnBack_Click;
            btnUp.Click += BtnUp_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnOpenFile.Click += BtnOpenFile_Click;
            btnCopyFile.Click += BtnCopyFile_Click;
            btnSendFile.Click += BtnSendFile_Click;
            btnPreviewFile.Click += BtnPreviewFile_Click;

            this.ResumeLayout(false);
        }

        private void SetupForm()
        {
            // Проверяем права доступа
            if (_currentUser.Role != UserRole.Developer && _currentUser.Role != UserRole.Director)
            {
                MessageBox.Show("У вас нет прав для просмотра удаленных файлов!", "Ошибка доступа", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            // Проверяем, включен ли удаленный доступ
            if (!_targetComputer.RemoteAccessEnabled)
            {
                MessageBox.Show("Удаленный доступ к этому компьютеру не включен!", "Ошибка доступа", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }
        }

        private void LoadDrives()
        {
            try
            {
                treeViewFiles.Nodes.Clear();
                
                // Добавляем доступные диски
                string[] drives = { "C:\\", "D:\\", "E:\\" };
                foreach (string drive in drives)
                {
                    if (Directory.Exists(drive))
                    {
                        var driveNode = new TreeNode(drive)
                        {
                            Tag = drive,
                            ImageIndex = 0,
                            SelectedImageIndex = 0
                        };
                        treeViewFiles.Nodes.Add(driveNode);
                        
                        // Загружаем корневые папки
                        LoadSubDirectories(driveNode);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка загрузки дисков: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSubDirectories(TreeNode parentNode)
        {
            try
            {
                string path = parentNode.Tag.ToString();
                string[] directories = Directory.GetDirectories(path);
                
                foreach (string dir in directories)
                {
                    var dirNode = new TreeNode(Path.GetFileName(dir))
                    {
                        Tag = dir,
                        ImageIndex = 1,
                        SelectedImageIndex = 1
                    };
                    parentNode.Nodes.Add(dirNode);
                }
            }
            catch
            {
                // Игнорируем ошибки доступа
            }
        }

        private void TreeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag != null)
            {
                currentPath = e.Node.Tag.ToString();
                txtPath.Text = currentPath;
                LoadFiles(currentPath);
            }
        }

        private void LoadFiles(string path)
        {
            try
            {
                listViewFiles.Items.Clear();
                
                // Загружаем папки
                string[] directories = Directory.GetDirectories(path);
                foreach (string dir in directories)
                {
                    var item = new ListViewItem(Path.GetFileName(dir))
                    {
                        SubItems = { "Папка", "", Directory.GetLastWriteTime(dir).ToString("dd.MM.yyyy HH:mm") }
                    };
                    listViewFiles.Items.Add(item);
                }
                
                // Загружаем файлы
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var item = new ListViewItem(Path.GetFileName(file))
                    {
                        SubItems = { 
                            Path.GetExtension(file).ToUpper(), 
                            FormatFileSize(fileInfo.Length),
                            fileInfo.LastWriteTime.ToString("dd.MM.yyyy HH:mm")
                        }
                    };
                    listViewFiles.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка загрузки файлов: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return bytes + " Б";
            if (bytes < 1024 * 1024) return (bytes / 1024) + " КБ";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)) + " МБ";
            return (bytes / (1024 * 1024 * 1024)) + " ГБ";
        }

        private void ListViewFiles_DoubleClick(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                var selectedItem = listViewFiles.SelectedItems[0];
                string itemName = selectedItem.Text;
                string itemType = selectedItem.SubItems[1].Text;
                
                if (itemType == "Папка")
                {
                    string newPath = Path.Combine(currentPath, itemName);
                    currentPath = newPath;
                    txtPath.Text = currentPath;
                    LoadFiles(currentPath);
                }
                else
                {
                    // Показываем информацию о файле
                    string filePath = Path.Combine(currentPath, itemName);
                    ShowFileInfo(filePath);
                }
            }
        }

        private void ShowFileInfo(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                string info = string.Format(
                    "Файл: {0}\n" +
                    "Путь: {1}\n" +
                    "Размер: {2}\n" +
                    "Дата создания: {3}\n" +
                    "Дата изменения: {4}\n" +
                    "Атрибуты: {5}",
                    fileInfo.Name,
                    fileInfo.FullName,
                    FormatFileSize(fileInfo.Length),
                    fileInfo.CreationTime.ToString("dd.MM.yyyy HH:mm:ss"),
                    fileInfo.LastWriteTime.ToString("dd.MM.yyyy HH:mm:ss"),
                    fileInfo.Attributes.ToString()
                );
                
                MessageBox.Show(info, "Информация о файле", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка получения информации о файле: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            // Возврат к предыдущему пути
            if (!string.IsNullOrEmpty(currentPath))
            {
                string parentPath = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrEmpty(parentPath))
                {
                    currentPath = parentPath;
                    txtPath.Text = currentPath;
                    LoadFiles(currentPath);
                }
            }
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            // Переход на уровень вверх
            BtnBack_Click(sender, e);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            // Обновление текущего пути
            if (!string.IsNullOrEmpty(currentPath))
            {
                LoadFiles(currentPath);
            }
        }

        private void ListViewFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Включаем/выключаем кнопки в зависимости от выбора
            bool hasSelection = listViewFiles.SelectedItems.Count > 0;
            bool isFile = false;
            
            if (hasSelection)
            {
                var selectedItem = listViewFiles.SelectedItems[0];
                isFile = selectedItem.SubItems[1].Text != "Папка";
            }

            // Находим кнопки по тексту
            foreach (Control control in this.Controls)
            {
                if (control is Panel panel)
                {
                    foreach (Control btn in panel.Controls)
                    {
                        if (btn is Button button)
                        {
                            switch (button.Text)
                            {
                                case "Открыть":
                                case "Копировать":
                                case "Отправить":
                                case "Просмотр":
                                    button.Enabled = hasSelection && isFile;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void BtnOpenFile_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                var selectedItem = listViewFiles.SelectedItems[0];
                string fileName = selectedItem.Text;
                string filePath = Path.Combine(currentPath, fileName);
                
                try
                {
                    // Открываем файл с помощью системного приложения по умолчанию
                    System.Diagnostics.Process.Start(filePath);
                    MessageBox.Show($"Файл '{fileName}' открыт!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка открытия файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnCopyFile_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                var selectedItem = listViewFiles.SelectedItems[0];
                string fileName = selectedItem.Text;
                string sourcePath = Path.Combine(currentPath, fileName);
                
                try
                {
                    // Открываем диалог выбора папки для копирования
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.Description = "Выберите папку для копирования файла";
                        folderDialog.ShowNewFolderButton = true;
                        
                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            string destPath = Path.Combine(folderDialog.SelectedPath, fileName);
                            
                            // Проверяем, существует ли файл
                            if (File.Exists(destPath))
                            {
                                var result = MessageBox.Show(
                                    $"Файл '{fileName}' уже существует в выбранной папке. Заменить?",
                                    "Подтверждение",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);
                                
                                if (result != DialogResult.Yes)
                                    return;
                            }
                            
                            // Копируем файл
                            File.Copy(sourcePath, destPath, true);
                            MessageBox.Show($"Файл '{fileName}' скопирован в '{folderDialog.SelectedPath}'", 
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка копирования файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                var selectedItem = listViewFiles.SelectedItems[0];
                string fileName = selectedItem.Text;
                string filePath = Path.Combine(currentPath, fileName);
                
                try
                {
                    // Открываем диалог выбора папки для сохранения
                    using (var saveDialog = new SaveFileDialog())
                    {
                        saveDialog.FileName = fileName;
                        saveDialog.Filter = "Все файлы (*.*)|*.*";
                        saveDialog.Title = "Сохранить файл как";
                        
                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Копируем файл в выбранное место
                            File.Copy(filePath, saveDialog.FileName, true);
                            MessageBox.Show($"Файл '{fileName}' отправлен и сохранен как '{saveDialog.FileName}'", 
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отправки файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnPreviewFile_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0)
            {
                var selectedItem = listViewFiles.SelectedItems[0];
                string fileName = selectedItem.Text;
                string filePath = Path.Combine(currentPath, fileName);
                string extension = Path.GetExtension(fileName).ToLower();
                
                try
                {
                    // Просмотр в зависимости от типа файла
                    switch (extension)
                    {
                        case ".txt":
                        case ".log":
                        case ".ini":
                        case ".cfg":
                        case ".xml":
                        case ".json":
                        case ".csv":
                            ShowTextPreview(filePath, fileName);
                            break;
                            
                        case ".jpg":
                        case ".jpeg":
                        case ".png":
                        case ".gif":
                        case ".bmp":
                            ShowImagePreview(filePath, fileName);
                            break;
                            
                        case ".pdf":
                            ShowPdfPreview(filePath, fileName);
                            break;
                            
                        default:
                            MessageBox.Show($"Предварительный просмотр для файлов типа '{extension}' не поддерживается.\nИспользуйте кнопку 'Открыть' для просмотра.", 
                                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка предварительного просмотра: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowTextPreview(string filePath, string fileName)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                
                var previewForm = new Form
                {
                    Text = $"Предварительный просмотр - {fileName}",
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(30, 30, 30)
                };
                
                var textBox = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    ReadOnly = true,
                    BackColor = Color.FromArgb(45, 45, 48),
                    ForeColor = Color.White,
                    Font = new Font("Consolas", 10),
                    Text = content
                };
                
                previewForm.Controls.Add(textBox);
                previewForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения текстового файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowImagePreview(string filePath, string fileName)
        {
            try
            {
                var image = Image.FromFile(filePath);
                
                var previewForm = new Form
                {
                    Text = $"Предварительный просмотр - {fileName}",
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(30, 30, 30)
                };
                
                var pictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = image
                };
                
                previewForm.Controls.Add(pictureBox);
                previewForm.Show();
                
                // Освобождаем ресурсы при закрытии формы
                previewForm.FormClosed += (s, e) => image.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowPdfPreview(string filePath, string fileName)
        {
            try
            {
                // Для PDF используем системный просмотрщик
                System.Diagnostics.Process.Start(filePath);
                MessageBox.Show($"PDF файл '{fileName}' открыт в системном просмотрщике!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия PDF: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
