using System;
using System.Windows;
using System.Windows.Controls;
using ExtensionLibrary;

namespace HomeWork_13_Bank_WPF.Views
{
    /// <summary>
    ///     Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization
    {
        #region Fields

        private int _choise; // переменная, которая "запоминает выбор пользователя"

        #endregion

        #region Свойства

        public int Choise => _choise;

        /// <summary>
        ///     Возвращает введенное имя пользователя
        /// </summary>
        public string GetNameOfClient => EnteredName.Text;

        /// <summary>
        ///     Возвращает введенное id пользователя
        /// </summary>
        public uint GetIdOfClient => EnteredId.Text.ToUInt();

        /// <summary>
        ///     Возвращает введеное пользователем пароля
        /// </summary>
        public string GetPasswordOfClient => EnteredPassword.Password;

        #endregion

        #region .ctor

        /// <inheritdoc cref="Authorization"/>
        public Authorization()
        {
            InitializeComponent();
            _choise = -1;
        }

        #endregion

        #region Private methods

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            _choise = 1;
            Close();
        }

        /// <summary>
        ///     Задает choise = 0, чтобы в MainWindow.xaml.cs уже создать окно регистрации нового клиента
        /// </summary>
        private void Registaion_Click(object sender, RoutedEventArgs e)
        {
            _choise = 0;
            Close();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            EnteredName.SelectAll();
            EnteredName.Focus();
        }

        private void EnteredId_TextChanged(object sender, TextChangedEventArgs e)
        {
            var t = EnteredId.Text;
            if (System.Text.RegularExpressions.Regex.IsMatch(t, "^[0-9]"))
            {
                EnteredId.Text = t.Remove(t.Length - 1, 1);
                EnteredId.SelectionStart = EnteredId.Text.Length - 1;
            }
        }

        #endregion
    }
}
