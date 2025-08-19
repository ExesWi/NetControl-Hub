using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NetControl_Hub.Models;

namespace NetControl_Hub.Services
{
    public class ProgramService
    {
        private readonly List<ProgramInfo> _programs;
        private readonly string _programsDataPath;
        private readonly string _desktopPath;

        public ProgramService()
        {
            _programs = new List<ProgramInfo>();
            
            // Пробуем разные варианты сетевого пути
            string[] networkPaths = {
                @"\\DESKTOP-56FV0G8\Users\Exest\3D Objects\Server\Shared\Net Control Hub\Programs", // Правильный путь
                @"\\DESKTOP-56FV0G8\Shared\Net Control Hub\Programs",
                @"\\DESKTOP-56FV0G8\Shared\NetControlHub\Programs"
            };
            
            _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            // Пытаемся найти доступный сетевой путь
            bool networkFound = false;
            foreach (string networkPath in networkPaths)
            {
                if (IsNetworkPathAvailable(networkPath))   
                {
                    _programsDataPath = networkPath;
                    networkFound = true;
                    MessageBox.Show(string.Format("Сетевой режим активирован!\nПуть: {0}", networkPath), 
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                }
                else
                {
                    Console.WriteLine("Не удалось подключиться к: " + networkPath);
                }
            }
            
            if (!networkFound)
            {
                // Если сеть недоступна, используем локальную папку
                _programsDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetControl Hub", "Programs");
                MessageBox.Show("Сетевой режим недоступен! Работаем в локальном режиме.\n\nПроверьте:\n1. Общий доступ к папке\n2. Права доступа\n3. Сетевой путь", 
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            // Создаем папки, если их нет
            if (!Directory.Exists(_programsDataPath))
            {
                try
                {
                    Directory.CreateDirectory(_programsDataPath);
                    Console.WriteLine("Создана папка: " + _programsDataPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Не удалось создать папку для данных: {0}", ex.Message), 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            
            CreateNetworkSoftwareFolder();
            
            LoadPrograms();
        }

        private void CreateNetworkSoftwareFolder()
        {
            try
            {
                // Создаем папку Software в сетевом хранилище
                string networkSoftwarePath = Path.Combine(Path.GetDirectoryName(_programsDataPath), "Software");
                if (!Directory.Exists(networkSoftwarePath))
                {
                    Directory.CreateDirectory(networkSoftwarePath);
                    Console.WriteLine("Создана сетевая папка для программ: " + networkSoftwarePath);
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка создания сетевой папки для программ: " + ex.Message);
            }
        }

        private string GetNetworkSoftwarePath()
        {
            return Path.Combine(Path.GetDirectoryName(_programsDataPath), "Software");
        }

        private void LoadPrograms()
        {
            try
            {
                string dataFile = Path.Combine(_programsDataPath, "programs.txt");
                if (File.Exists(dataFile))
                {
                    string[] lines = File.ReadAllLines(dataFile);
                    var loadedPrograms = new List<ProgramInfo>();
                    
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            try
                            {
                                string[] parts = line.Split('|');
                                if (parts.Length >= 11)
                                {
                                    var program = new ProgramInfo
                                    {
                                        Id = parts[0],
                                        Name = parts[1],
                                        SourcePath = parts[2],
                                        InstallPath = parts[3],
                                        DesktopShortcutPath = parts[4],
                                        InstallDate = DateTime.Parse(parts[5]),
                                        LastUpdateDate = DateTime.Parse(parts[6]),
                                        Version = parts[7],
                                        IsLatestVersion = bool.Parse(parts[8]),
                                        InstalledBy = parts[9],
                                        Status = (ProgramStatus)Enum.Parse(typeof(ProgramStatus), parts[10])
                                    };
                                    
                                    // Проверяем на дубликаты по имени
                                    var existingProgram = loadedPrograms.FirstOrDefault(p => p.Name.Equals(program.Name, StringComparison.OrdinalIgnoreCase));
                                    if (existingProgram == null)
                                    {
                                        loadedPrograms.Add(program);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Пропущен дубликат программы: " + program.Name);
                                    }
                                }
                            }
                            catch
                            {
                                // Пропускаем некорректные записи
                                continue;
                            }
                        }
                    }
                    
                    _programs.Clear();
                    _programs.AddRange(loadedPrograms);
                    
                    // Если были найдены дубликаты, сохраняем очищенный список
                    if (loadedPrograms.Count < lines.Length)
                    {
                        SavePrograms();
                        Console.WriteLine("Дублированные программы удалены из списка");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка загрузки программ: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SavePrograms()
        {
            try
            {
                string dataFile = Path.Combine(_programsDataPath, "programs.txt");
                Console.WriteLine("Сохраняем программы в: " + dataFile);
                
                var lines = new List<string>();
                
                foreach (var program in _programs)
                {
                    string line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}", 
                        program.Id ?? "", 
                        program.Name ?? "", 
                        program.SourcePath ?? "", 
                        program.InstallPath ?? "", 
                        program.DesktopShortcutPath ?? "",
                        program.InstallDate.ToString("yyyy-MM-dd HH:mm:ss"), 
                        program.LastUpdateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        program.Version ?? "", 
                        program.IsLatestVersion.ToString(), 
                        program.InstalledBy ?? "", 
                        program.Status.ToString());
                    
                    lines.Add(line);
                }
                
                File.WriteAllLines(dataFile, lines);
                Console.WriteLine("Программы успешно сохранены. Количество: " + _programs.Count);
                
                // Показываем уведомление о сохранении
                if (IsNetworkMode())
                {
                    MessageBox.Show(string.Format("Программа сохранена в сетевую папку!\nПуть: {0}\nКоличество программ: {1}", 
                        dataFile, _programs.Count), "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка сохранения: " + ex.Message);
                MessageBox.Show(string.Format("Ошибка сохранения программ: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool InstallProgram(string programName, string sourcePath, string installPath, string exePath, string installedBy)
        {
            try
            {
                if (!Directory.Exists(sourcePath))
                {
                    MessageBox.Show("Исходная папка не найдена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!File.Exists(exePath))
                {
                    MessageBox.Show("Файл .exe не найден по указанному пути!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Проверяем, не существует ли уже программа с таким именем
                var existingProgram = _programs.FirstOrDefault(p => p.Name.Equals(programName, StringComparison.OrdinalIgnoreCase));
                if (existingProgram != null)
                {
                    MessageBox.Show(string.Format("Программа с названием '{0}' уже существует в системе!\n\nПожалуйста, используйте другое название или удалите существующую программу.", programName), 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Копируем программу в сетевое хранилище
                string networkSoftwarePath = GetNetworkSoftwarePath();
                string programFolderName = Path.GetFileName(sourcePath);
                string networkProgramPath = Path.Combine(networkSoftwarePath, programFolderName);
                
                // Если папка уже существует, добавляем номер
                int counter = 1;
                string finalNetworkPath = networkProgramPath;
                while (Directory.Exists(finalNetworkPath))
                {
                    finalNetworkPath = Path.Combine(networkSoftwarePath, string.Format("{0}_{1}", programFolderName, counter));
                    counter++;
                }
                
                // Копируем программу в сеть
                CopyDirectory(sourcePath, finalNetworkPath);
                Console.WriteLine("Программа скопирована в сеть: " + finalNetworkPath);
                
                // Обновляем путь к .exe файлу в сетевом хранилище
                string originalExeName = Path.GetFileName(exePath);
                string networkExePath = Path.Combine(finalNetworkPath, originalExeName);

                // Создаем программу с сетевыми путями
                var program = new ProgramInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = programName,
                    SourcePath = finalNetworkPath, // Теперь это сетевой путь
                    InstallPath = installPath, // Путь для будущей установки
                    DesktopShortcutPath = networkExePath, // Сетевой путь к .exe файлу
                    InstallDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                    Version = "1.0",
                    IsLatestVersion = true,
                    InstalledBy = installedBy,
                    Status = ProgramStatus.NotInstalled // Программа доступна для установки
                };
                
                _programs.Add(program);
                SavePrograms();
                
                MessageBox.Show(string.Format("Программа успешно добавлена в сетевое хранилище!\n\nНазвание: {0}\nСетевой путь: {1}\nКоличество программ: {2}", 
                    programName, finalNetworkPath, _programs.Count), "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка добавления программы: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetUniqueInstallPath(string basePath)
        {
            if (!Directory.Exists(basePath))
                return basePath;

            string directory = Path.GetDirectoryName(basePath);
            string name = Path.GetFileName(basePath);
            
            int counter = 1;
            string newPath = basePath;
            
            while (Directory.Exists(newPath))
            {
                newPath = Path.Combine(directory, string.Format("{0}_old{1}", name, counter));
                counter++;
            }
            
            return newPath;
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }
            
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dir);
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(dir, destSubDir);
            }
        }

        private string CreateDesktopShortcut(string shortcutName, string targetPath)
        {
            try
            {
                // Очищаем имя ярлыка от недопустимых символов
                string cleanShortcutName = string.Join("", shortcutName.Split(Path.GetInvalidFileNameChars()));
                string shortcutPath = Path.Combine(_desktopPath, cleanShortcutName + ".lnk");
                
                // Используем PowerShell для создания ярлыка
                string command = string.Format("powershell -Command \"$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('{0}'); $Shortcut.TargetPath = '{1}'; $Shortcut.WorkingDirectory = '{2}'; $Shortcut.Save()\"", 
                    shortcutPath, targetPath, Path.GetDirectoryName(targetPath));
                
                System.Diagnostics.Process.Start("cmd", "/c " + command);
                
                // Ждем немного, чтобы ярлык создался
                System.Threading.Thread.Sleep(500);
                
                return shortcutPath;
            }
            catch
            {
                // В случае ошибки возвращаем путь к .exe файлу
                return targetPath;
            }
        }

        public bool UpdateProgram(string programId, string newSourcePath, string updatedBy)
        {
            try
            {
                var program = _programs.FirstOrDefault(p => p.Id == programId);
                if (program == null)
                    return false;

                string oldPath = program.InstallPath;
                string newPath = GetUniqueInstallPath(oldPath);
                Directory.Move(oldPath, newPath);
                
                string finalInstallPath = GetUniqueInstallPath(program.InstallPath);
                CopyDirectory(newSourcePath, finalInstallPath);
                
                program.InstallPath = finalInstallPath;
                program.LastUpdateDate = DateTime.Now;
                program.IsLatestVersion = true;
                program.Status = ProgramStatus.Installed;
                
                SavePrograms();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка обновления программы: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool InstallProgramOnLocalComputer(string programId, string localInstallPath)
        {
            try
            {
                var program = _programs.FirstOrDefault(p => p.Id == programId);
                if (program == null)
                    return false;

                if (program.Status == ProgramStatus.Installed)
                {
                    MessageBox.Show("Программа уже установлена на этом компьютере!", 
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                // Проверяем, что исходная папка существует (теперь это сетевой путь)
                if (!Directory.Exists(program.SourcePath))
                {
                    MessageBox.Show(string.Format("Исходная папка программы не найдена!\nПуть: {0}\n\nУбедитесь, что:\n1. Сеть доступна\n2. Папка существует в сетевом хранилище", program.SourcePath), 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Копируем программу с сети на локальный компьютер
                string installDirectory = Path.GetDirectoryName(localInstallPath);
                
                // Создаем папку в корне диска C: если её нет
                if (!Directory.Exists(installDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(installDirectory);
                        Console.WriteLine($"Создана папка для установки программ: {installDirectory}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось создать папку {installDirectory}: {ex.Message}", 
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                
                string programFolderName = Path.GetFileName(program.SourcePath);
                string baseInstallPath = Path.Combine(installDirectory, programFolderName);
                
                string finalInstallPath = GetUniqueInstallPath(baseInstallPath);
                
                // Копируем из сетевого хранилища
                CopyDirectory(program.SourcePath, finalInstallPath);
                Console.WriteLine("Программа скопирована с сети на локальный компьютер: " + finalInstallPath);
                
                // Создаем ярлык на рабочем столе
                string shortcutName = "Неизвестно";
                string exePath = "";
                
                if (!string.IsNullOrEmpty(program.DesktopShortcutPath))
                {
                    // Получаем имя .exe файла из сетевого пути
                    string originalExeName = Path.GetFileName(program.DesktopShortcutPath);
                    shortcutName = Path.GetFileNameWithoutExtension(originalExeName);
                    
                    // Формируем путь к .exe файлу в локальной папке установки
                    exePath = Path.Combine(finalInstallPath, originalExeName);
                }
                else if (!string.IsNullOrEmpty(program.Name))
                {
                    shortcutName = program.Name;
                    // Ищем .exe файл в папке установки
                    string[] exeFiles = Directory.GetFiles(finalInstallPath, "*.exe", SearchOption.TopDirectoryOnly);
                    if (exeFiles.Length > 0)
                    {
                        exePath = exeFiles[0]; // Берем первый найденный .exe файл
                    }
                }
                
                // Если .exe файл найден, создаем ярлык на него
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    string shortcutPath = CreateDesktopShortcut(shortcutName, exePath);
                    
                    // Обновляем статус программы для этого пользователя
                    program.Status = ProgramStatus.Installed;
                    program.InstallPath = finalInstallPath;
                    program.DesktopShortcutPath = shortcutPath;
                    program.InstallDate = DateTime.Now;
                    
                    MessageBox.Show(string.Format("Программа успешно установлена!\n\nНазвание: {0}\nЛокальный путь: {1}\nЯрлык создан на рабочем столе", 
                        program.Name, finalInstallPath), "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось найти .exe файл для создания ярлыка!", 
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    // Обновляем статус программы для этого пользователя
                    program.Status = ProgramStatus.Installed;
                    program.InstallPath = finalInstallPath;
                    program.DesktopShortcutPath = "";
                    program.InstallDate = DateTime.Now;
                }
                
                SavePrograms();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка установки программы: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UninstallProgram(string programId)
        {
            try
            {
                var program = _programs.FirstOrDefault(p => p.Id == programId);
                if (program == null)
                    return false;

                // Удаляем установленную папку, если она существует
                if (!string.IsNullOrEmpty(program.InstallPath) && Directory.Exists(program.InstallPath))
                {
                    Directory.Delete(program.InstallPath, true);
                }
                
                // Удаляем ярлык, если он существует
                if (!string.IsNullOrEmpty(program.DesktopShortcutPath) && File.Exists(program.DesktopShortcutPath))
                {
                    File.Delete(program.DesktopShortcutPath);
                }
                
                // Возвращаем статус "не установлена"
                program.Status = ProgramStatus.NotInstalled;
                program.InstallPath = "";
                program.DesktopShortcutPath = "";
                
                SavePrograms();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка удаления программы: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<ProgramInfo> GetAllPrograms()
        {
            return _programs.ToList();
        }

        public ProgramInfo GetProgramById(string programId)
        {
            return _programs.FirstOrDefault(p => p.Id == programId);
        }

        public void CheckForUpdates()
        {
            foreach (var program in _programs)
            {
                if (Directory.Exists(program.SourcePath))
                {
                    var sourceInfo = new DirectoryInfo(program.SourcePath);
                    if (sourceInfo.LastWriteTime > program.LastUpdateDate)
                    {
                        program.Status = ProgramStatus.Outdated;
                        program.IsLatestVersion = false;
                    }
                }
            }
            SavePrograms();
        }

        public bool DeleteProgramFromSystem(string programId)
        {
            try
            {
                var program = _programs.FirstOrDefault(p => p.Id == programId);
                if (program == null)
                    return false;

                // Удаляем программу из списка
                _programs.Remove(program);
                
                // Сохраняем обновленный список
                SavePrograms();
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка удаления программы из системы: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool IsNetworkPathAvailable(string networkPath)
        {
            try
            {
                Console.WriteLine("Проверяем доступность: " + networkPath);
                
                // Проверяем существование папки
                if (!Directory.Exists(networkPath))
                {
                    Console.WriteLine("Папка не существует: " + networkPath);
                    return false;
                }
                
                // Пытаемся создать тестовый файл
                string testFile = Path.Combine(networkPath, "test.tmp");
                File.WriteAllText(testFile, "test");
                
                // Проверяем, что файл создался
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                    Console.WriteLine("Сетевой путь доступен: " + networkPath);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при проверке сети: " + ex.Message);
                return false;
            }
        }

        public bool IsNetworkMode()
        {
            return _programsDataPath.StartsWith(@"\\");
        }

        public void RefreshFromNetwork()
        {
            if (IsNetworkMode())
            {
                LoadPrograms(); // Перезагружаем данные из сети
            }
        }

        public void CleanupDuplicatePrograms()
        {
            try
            {
                var uniquePrograms = new List<ProgramInfo>();
                var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                foreach (var program in _programs)
                {
                    if (!seenNames.Contains(program.Name))
                    {
                        seenNames.Add(program.Name);
                        uniquePrograms.Add(program);
                    }
                    else
                    {
                        Console.WriteLine("Удален дубликат: " + program.Name);
                    }
                }
                
                if (uniquePrograms.Count != _programs.Count)
                {
                    _programs.Clear();
                    _programs.AddRange(uniquePrograms);
                    SavePrograms();
                    
                    MessageBox.Show(string.Format("Очистка завершена!\n\nУдалено дублированных программ: {0}\nОсталось уникальных программ: {1}", 
                        _programs.Count - uniquePrograms.Count, uniquePrograms.Count), "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка очистки дубликатов: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string ProgramsDataPath => _programsDataPath;
    }
}
