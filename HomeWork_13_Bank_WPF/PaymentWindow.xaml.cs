using System;
using ExtensionLibrary;
using System.Windows;

namespace HomeWork_13_Bank_WPF
{
    /// <summary>
    /// Логика взаимодействия для PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private double _payment;
        /// <summary>
        /// Возвращает значение введеное пользователем
        /// </summary>
        public double GetPayment
        {
            get => _payment;
        }
        
        public PaymentWindow()
        {
            InitializeComponent();
        }

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
                    this.DialogResult = true;
                    this.Close();
                }
                else
                    throw new Exception();
            }
            catch(Exception)
            {
                MessageBox.Show("Некорректный ввод.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}
