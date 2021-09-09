using System;
using System.Windows;
using ExtensionLibrary;

namespace HomeWork_13_Bank_WPF.Views
{
    /// <summary>
    ///     Логика взаимодействия для Password.xaml
    /// </summary>
    public partial class Password
    {
        public Password()
        {
            InitializeComponent();
        }

        #region Private methods

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            PasswordEntered.SelectAll();
            PasswordEntered.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var password = PasswordEntered.Text.ToInt();
                if (password == 777 || password == 0102)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Что-то пошло не так...", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        #endregion
    }
}
