using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ExtensionLibrary;

namespace HomeWork_13_Bank_WPF
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        private int _choise; // переменная, которая "запоминает выбор пользователя"
        #region Свойства
        public int Choise
        {
            get => _choise;
        }
        /// <summary>
        /// Возвращает введенное имя пользователя
        /// </summary>
        public string GetNameOfClient
        {
            get => EnteredName.Text;
        }
        /// <summary>
        /// Возвращает введенное id пользователя
        /// </summary>
        public uint GetIdOfClient
        {
            get => EnteredId.Text.ToUInt();
          
        }
        /// <summary>
        /// Возвращает введеное пользователем пароля
        /// </summary>
        public string GetPasswordOfClient
        {
            get => EnteredPassword.Password;
        }
        #endregion
        public Authorization()
        {
            InitializeComponent();
            _choise = -1;
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            _choise = 1;
            this.Close();
        }
        /// <summary>
        /// Задает choise = 0, чтобы в MainWindow.xaml.cs уже создать окно регистрации нового клиента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Registaion_Click(object sender, RoutedEventArgs e)
        {
            _choise = 0;
            this.Close();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            EnteredName.SelectAll();
            EnteredName.Focus();
        }

        private void EnteredId_TextChanged(object sender, TextChangedEventArgs e)
        {
            string t = EnteredId.Text;
            if (System.Text.RegularExpressions.Regex.IsMatch(t, "^[0-9]")) 
            {
                EnteredId.Text = t.Remove(t.Length - 1, 1);
                EnteredId.SelectionStart = EnteredId.Text.Length - 1;
            }
        }
    }
}
