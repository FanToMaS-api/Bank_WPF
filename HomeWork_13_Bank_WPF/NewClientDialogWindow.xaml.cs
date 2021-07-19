using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HomeWork_13_Bank_WPF
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
        /// <summary>
        /// Возвращает какой тип выбрал пользователь
        /// 0 - обычный клиент
        /// 1 - Юридическое лицо
        /// 2 - ВИП-клиент
        /// </summary>
        public BasicClient GetChoise
        {
            get
            {
                if (cmbTypeofClient.SelectedItem == cmbTypeofClient.Items[0])
                {
                    return new Client();
                }
                else if (cmbTypeofClient.SelectedItem == cmbTypeofClient.Items[1])
                {
                    return new EntityClient();
                }
                else if (cmbTypeofClient.SelectedItem == cmbTypeofClient.Items[2])
                {
                    Password passwordDialogWindow = new Password();
                    passwordDialogWindow.ShowDialog();
                    if ((bool)passwordDialogWindow.DialogResult)
                    {
                        return new VipClient();
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// Возвращает введеное имя пользователем
        /// </summary>
        public string GetName
        {
            get => EnteredNewClientName.Text;
        }
        public string GetPassword
        {
            get => EnteredNewClientPassword.Password;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            EnteredNewClientName.SelectAll();
            EnteredNewClientName.Focus();
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (EnteredNewClientName.Text != "")
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Неверно введены данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
        }
    }
}
