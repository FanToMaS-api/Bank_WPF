using System;
using ExtensionLibrary;
using System.Windows;


namespace HomeWork_13_Bank_WPF
{
    /// <summary>
    /// Логика взаимодействия для Password.xaml
    /// </summary>
    public partial class Password : Window
    {
        public Password()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            PasswordEntered.SelectAll();
            PasswordEntered.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int password = PasswordEntered.Text.ToInt();
                if (password == 777 || password == 0102)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                MessageBox.Show("Что-то пошло не так...", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}
