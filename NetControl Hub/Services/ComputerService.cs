using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net;
using NetControl_Hub.Models;

namespace NetControl_Hub.Services
{
    public class ComputerService
    {
        private readonly List<Computer> _computers;
        private readonly string _computersDataPath;

        public ComputerService(string baseDataPath)
        {
            _computers = new List<Computer>();
            _computersDataPath = Path.Combine(Path.GetDirectoryName(baseDataPath), "Computers");
            
            Console.WriteLine($"ComputerService: Путь к данным компьютеров: {_computersDataPath}");
            
            try
            {
                if (!Directory.Exists(_computersDataPath))
                {
                    Directory.CreateDirectory(_computersDataPath);
                    Console.WriteLine($"Создана папка для компьютеров: {_computersDataPath}");
                }
                
                LoadComputers();
                Console.WriteLine($"Загружено компьютеров: {_computers.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации ComputerService: {ex.Message}");
                MessageBox.Show($"Ошибка доступа к сетевым данным: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadComputers()
        {
            try
            {
                string dataFile = Path.Combine(_computersDataPath, "computers.txt");
                if (File.Exists(dataFile))
                {
                    string[] lines = File.ReadAllLines(dataFile);
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            try
                            {
                                string[] parts = line.Split('|');
                                if (parts.Length >= 10)
                                {
                                    var computer = new Computer
                                    {
                                        Id = parts[0],
                                        Name = parts[1],
                                        IpAddress = parts[2],
                                        UserName = parts[3],
                                        UserRole = parts[4],
                                        LastSeen = DateTime.Parse(parts[5]),
                                        IsOnline = bool.Parse(parts[6]),
                                        RemoteAccessEnabled = bool.Parse(parts[7]),
                                        SharedFolders = parts[8],
                                        AccessGrantedDate = DateTime.Parse(parts[9]),
                                        GrantedBy = parts[10]
                                    };
                                    _computers.Add(computer);
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка загрузки компьютеров: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveComputers()
        {
            try
            {
                string dataFile = Path.Combine(_computersDataPath, "computers.txt");
                var lines = new List<string>();
                
                foreach (var computer in _computers)
                {
                    string line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}", 
                        computer.Id ?? "", 
                        computer.Name ?? "", 
                        computer.IpAddress ?? "", 
                        computer.UserName ?? "", 
                        computer.UserRole ?? "",
                        computer.LastSeen.ToString("yyyy-MM-dd HH:mm:ss"), 
                        computer.IsOnline.ToString(), 
                        computer.RemoteAccessEnabled.ToString(),
                        computer.SharedFolders ?? "",
                        computer.AccessGrantedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        computer.GrantedBy ?? "");
                    
                    lines.Add(line);
                }
                
                File.WriteAllLines(dataFile, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка сохранения компьютеров: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RegisterComputer(string computerName, string userName, string userRole)
        {
            try
            {
                // Получаем IP адрес
                string ipAddress = GetLocalIPAddress();
                
                // Проверяем, не зарегистрирован ли уже компьютер
                var existingComputer = _computers.FirstOrDefault(c => c.Name.Equals(computerName, StringComparison.OrdinalIgnoreCase));
                if (existingComputer != null)
                {
                    // Обновляем информацию
                    existingComputer.UserName = userName;
                    existingComputer.UserRole = userRole;
                    existingComputer.LastSeen = DateTime.Now;
                    existingComputer.IsOnline = true;
                }
                else
                {
                    // Создаем новый компьютер
                    var computer = new Computer
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = computerName,
                        IpAddress = ipAddress,
                        UserName = userName,
                        UserRole = userRole,
                        LastSeen = DateTime.Now,
                        IsOnline = true,
                        RemoteAccessEnabled = false,
                        SharedFolders = "",
                        AccessGrantedDate = DateTime.MinValue,
                        GrantedBy = ""
                    };
                    
                    _computers.Add(computer);
                }
                
                SaveComputers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка регистрации компьютера: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        public List<Computer> GetAllComputers()
        {
            return _computers.ToList();
        }

        public List<Computer> GetOnlineComputers()
        {
            return _computers.Where(c => c.IsOnline && c.RemoteAccessEnabled).ToList();
        }

        public bool EnableRemoteAccess(string computerId, string grantedBy)
        {
            try
            {
                var computer = _computers.FirstOrDefault(c => c.Id == computerId);
                if (computer == null)
                    return false;

                computer.RemoteAccessEnabled = true;
                computer.AccessGrantedDate = DateTime.Now;
                computer.GrantedBy = grantedBy;
                computer.SharedFolders = "C:\\|D:\\|E:\\"; // Полный доступ ко всем дискам
                
                SaveComputers();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка включения удаленного доступа: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool DisableRemoteAccess(string computerId)
        {
            try
            {
                var computer = _computers.FirstOrDefault(c => c.Id == computerId);
                if (computer == null)
                    return false;

                computer.RemoteAccessEnabled = false;
                computer.SharedFolders = "";
                
                SaveComputers();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка отключения удаленного доступа: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool IsComputerOnline(string computerId)
        {
            try
            {
                var computer = _computers.FirstOrDefault(c => c.Id == computerId);
                if (computer == null)
                    return false;

                // Пингуем компьютер
                using (var ping = new Ping())
                {
                    var reply = ping.Send(computer.IpAddress, 1000);
                    bool isOnline = reply?.Status == IPStatus.Success;
                    
                    // Обновляем статус
                    computer.IsOnline = isOnline;
                    if (isOnline)
                    {
                        computer.LastSeen = DateTime.Now;
                    }
                    
                    SaveComputers();
                    return isOnline;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
