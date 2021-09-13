using System;
using System.Collections.Generic;
using System.Windows;

namespace HomeWork_13_Bank_WPF.Views
{
    /// <summary>
    /// Логика взаимодействия для NewClientDialogWindow.xaml
    /// </summary>
    public partial class NewClientDialogWindow : Window
    {
        public NewClientDialogWindow()
        {
            InitializeComponent();
            cmbTypeofClient.ItemsSource = new List<string>() { "Обычный клиент",
                                                               "Юридическое лицо",
                                                               "ВИП-клиент",};
        }
        #region Properties

        /// <summary>
        ///     Возвращает какой тип выбрал пользователь
        ///         0 - обычный клиент
        ///         1 - Юридическое лицо
        ///         2 - ВИП-клиент
        /// </summary>
        public BasicClient GetChoise
        {
            get
            {
                if (cmbTypeofClient.SelectedItem == cmbTypeofClient.Items[0])
                {
                    return new Client();
                }

                if (cmbTypeofClient.SelectedItem == cmbTypeofClient.Items[1])
                {
                    return new EntityClient();
                }

                if (cmbTypeofClient.SelectedItem != cmbTypeofClient.Items[2])
                {
                    return null;
                }

                var passwordDialogWindow = new Password();
                passwordDialogWindow.ShowDialog();

                if (passwordDialogWindow.DialogResult != null && (bool)passwordDialogWindow.DialogResult)
                {
                    return new VipClient();
                }

                return null;
            }
        }

        /// <summary>
        ///     Возвращает введеное имя пользователем
        /// </summary>
        public string GetName => EnteredNewClientName.Text;

        /// <summary>
        ///     Возврат введенного пароля клиентом
        /// </summary>
        public string GetPassword => EnteredNewClientPassword.Password;

        #endregion

        #region Private methods

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            EnteredNewClientName.SelectAll();
            EnteredNewClientName.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (EnteredNewClientName.Text != "")
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Неверно введены данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Close();
        }

        #endregion
    }
}
