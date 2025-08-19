using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using NetControl_Hub.Models;

namespace NetControl_Hub.Services
{
    public class AuthService
    {
        private readonly List<User> _users;
        private readonly string _usersDataPath;

        public AuthService()
        {
            // Настройка базы данных в общей сетевой папке
            string[] networkPaths = {
                @"\\DESKTOP-56FV0G8\Users\Exest\3D Objects\Server\Shared\Net Control Hub\Database",
                @"\\DESKTOP-56FV0G8\Shared\Net Control Hub\Database",
                @"\\DESKTOP-56FV0G8\Shared\NetControlHub\Database",
                @"\\SERVER\Shared\NetControl Hub\Database"
            };
            
            // Пытаемся найти доступный сетевой путь для базы данных
            bool networkFound = false;
            foreach (string networkPath in networkPaths)
            {
                if (IsNetworkPathAvailable(networkPath))   
                {
                    _usersDataPath = networkPath;
                    networkFound = true;
                    Console.WriteLine("AuthService: Сетевая база данных активирована! Путь: " + networkPath);
                    break;
                }
            }
            
            if (!networkFound)
            {
                // Если сеть недоступна, используем локальную папку
                _usersDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetControl Hub", "Database");
                Console.WriteLine("AuthService: Сетевая база недоступна! Работаем локально. Путь: " + _usersDataPath);
            }
            
            // Создаем папки, если их нет
            if (!Directory.Exists(_usersDataPath))
            {
                try
                {
                    Directory.CreateDirectory(_usersDataPath);
                    Console.WriteLine("AuthService: Создана папка: " + _usersDataPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Не удалось создать папку для пользователей: {0}", ex.Message), 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            _users = LoadUsers();
        }

        private List<User> InitializeUsers()
        {
            var users = new List<User>();

            // Аккаунт role{разработчик}
            users.Add(new User
            {
                Username = "Exest",
                Password = "ex557exwww",
                Role = UserRole.Developer,
                DisplayName = "Exest",
                LastLogin = DateTime.MinValue,
                
            });
            //Личка для теста и обновлений
            users.Add(new User
            {
                Username = "Test",
                Password = "ts",
                DisplayName= "Test",
                Role = UserRole.Developer,
                LastLogin = DateTime.MinValue,
            });

            // Аккаунт role{директора
            users.Add(new User
            {
                Username = "test",
                Password = "test",
                Role = UserRole.Director,
                DisplayName = "test",
                LastLogin = DateTime.MinValue
            });

            // Аккаунты сотрудников (100 штук)
            for (int i = 1; i <= 100; i++)
            {
                users.Add(new User
                {
                    Username = string.Format("afarma2ex{0}", i),
                    Password = string.Format("exelect{0}", i),
                    Role = UserRole.Employee,
                    DisplayName = string.Format("Сотрудник {0}", i),
                    LastLogin = DateTime.MinValue
                });
            }

            return users;
        }

        private List<User> LoadUsers()
        {
            try
            {
                var users = new List<User>();
                string dataFile = Path.Combine(_usersDataPath, "users.txt");
                
                if (!File.Exists(dataFile))
                {
                    users = InitializeUsers();
                    SaveUsers(users);
                    return users;
                }

                var lines = File.ReadAllLines(dataFile);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    if (parts.Length < 6) continue;
                    users.Add(new User
                    {
                        Username = parts[0],
                        Password = parts[1],
                        Role = (UserRole)Enum.Parse(typeof(UserRole), parts[2]),
                        DisplayName = parts[3],
                        LastLogin = DateTime.TryParse(parts[4], out var dt) ? dt : DateTime.MinValue,
                        LoginCount = int.TryParse(parts[5], out var c) ? c : 0
                    });
                }
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AuthService: Ошибка загрузки пользователей: " + ex.Message);
                return InitializeUsers();
            }
        }

        private void SaveUsers(List<User> users)
        {
            try
            {
                string dataFile = Path.Combine(_usersDataPath, "users.txt");
                var lines = new List<string>();
                foreach (var u in users)
                {
                    lines.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}",
                        u.Username ?? "",
                        u.Password ?? "",
                        u.Role,
                        u.DisplayName ?? "",
                        u.LastLogin.ToString("yyyy-MM-dd HH:mm:ss"),
                        u.LoginCount));
                }
                File.WriteAllLines(dataFile, lines);
                Console.WriteLine("AuthService: Пользователи сохранены в: " + dataFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AuthService: Ошибка сохранения пользователей: " + ex.Message);
                MessageBox.Show(string.Format("Ошибка сохранения пользователей: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public User AuthenticateUser(string username, string password)
        {
            var user = _users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                u.Password == password);

            if (user != null)
            {
                user.LastLogin = DateTime.Now;
                user.LoginCount += 1;
                SaveUsers(_users);
            }

            return user;
        }

        public List<User> GetAllUsers()
        {
            return _users.ToList();
        }

        public bool AddUser(User newUser)
        {
            if (_users.Any(u => u.Username.Equals(newUser.Username, StringComparison.OrdinalIgnoreCase)))
                return false;
            _users.Add(newUser);
            SaveUsers(_users);
            return true;
        }

        public bool DeleteUser(string username)
        {
            var u = _users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (u == null) return false;
            _users.Remove(u);
            SaveUsers(_users);
            return true;
        }

        private bool IsNetworkPathAvailable(string networkPath)
        {
            try
            {
                Console.WriteLine("AuthService: Проверяем доступность: " + networkPath);
                
                // Проверяем существование папки
                if (!Directory.Exists(networkPath))
                {
                    Console.WriteLine("AuthService: Папка не существует: " + networkPath);
                    return false;
                }
                
                // Пытаемся создать тестовый файл
                string testFile = Path.Combine(networkPath, "auth_test.tmp");
                File.WriteAllText(testFile, "test");
                
                // Проверяем, что файл создался
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                    Console.WriteLine("AuthService: Сетевой путь доступен: " + networkPath);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AuthService: Ошибка при проверке сети: " + ex.Message);
                return false;
            }
        }

        public bool IsNetworkMode()
        {
            return _usersDataPath.StartsWith(@"\\");
        }
    }
}
