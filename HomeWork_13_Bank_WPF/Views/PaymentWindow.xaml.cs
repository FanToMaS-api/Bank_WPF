using System;
using System.Windows;
using ExtensionLibrary;

namespace HomeWork_13_Bank_WPF.Views
{
    /// <summary>
    /// Логика взаимодействия для PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        #region Fields

        private double _payment;

        /// <summary>
        ///     Возвращает значение введеное пользователем
        /// </summary>
        public double GetPayment => _payment;

        #endregion

        #region .ctor

        public PaymentWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Private methods

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            PaymentEntered.SelectAll();
            PaymentEntered.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _payment = PaymentEntered.Text.ToDouble();
                if (_payment > 0)
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
                MessageBox.Show("Некорректный ввод.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        #endregion
    }
}
