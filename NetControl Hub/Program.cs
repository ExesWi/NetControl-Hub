using System;
using System.Windows.Forms;
using NetControl_Hub.Forms;

namespace NetControl_Hub
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                // Запускаем форму входа
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Критическая ошибка приложения: {0}", ex.Message), 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
