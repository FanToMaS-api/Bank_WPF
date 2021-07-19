using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using ExtensionLibrary;
using System.Linq;
namespace HomeWork_13_Bank_WPF
{
    /// <summary>
    /// Логика взаимодействия для PersonalAccount.xaml
    /// </summary>
    public partial class PersonalAccount : Window
    {
        private readonly BasicClient _client;

        private readonly Bank _bank;
        public event Action<string, int, BasicClient> HistoryEvent;
        public PersonalAccount(ref BasicClient client, ref Bank bank)
        {
            InitializeComponent();
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
            this._client = client;
            HistoryEvent = BasicClient.HistoryMessage;
            this._bank = bank;
            TableOfCredits.ItemsSource = this._client.Credits;
            TableOfDeposits.ItemsSource = this._client.GetDeposits;
            cmbButtonDep.SelectedIndex = 0;
            InfoOfClient.ItemsSource = new List<BasicClient>() {this._client}; // скорее всего костыль для работы события PropertyChanged
            SetPossibleCredits();
            SetPossibleDeposits();            
        }
        /// <summary>
        /// Событие, которое каждую секунду обнновляет время на экране
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            TimeLabel.Content = DateTime.Now.ToLongTimeString();
        }
        /// <summary>
        /// Функция, показывающая все оставшиеся депозиты для пользователя
        /// </summary>
        private void SetPossibleDeposits()
        {
            ObservableCollection<Deposit> deposits = _bank.AllBankDeposits;
            foreach (var e in _client.GetDeposits)
            {
                if (deposits.Contains(e))
                    deposits.Remove(e);
            }
            TableOfPossibleDeposits.ItemsSource = deposits;
        }
        /// <summary>
        /// Функция, показывающая все оставшиеся кредиты для пользователя
        /// </summary>
        private void SetPossibleCredits()
        {
            ObservableCollection<Credit> credits = _bank.AllBankCredits;
            foreach(var e in _client.Credits)
            {
                if (credits.Contains(e))
                    credits.Remove(e);
            }
            TableOfPossibleCredits.ItemsSource = credits;
        }
        /// <summary>
        /// Функция, вносящая платеж по кредиту
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfCredits.SelectedItem != null)
            {
                PaymentWindow payment = new PaymentWindow();
                payment.ShowDialog();
                if ((bool)payment.DialogResult)
                    if (!_client.PaymentOfCredit(_bank, TableOfCredits.SelectedItem as Credit, payment.GetPayment))                    
                        MessageBox.Show("Недостаточно средств на счете.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);                    
            }
            else            
                MessageBox.Show("Выберите кредит для внесения платежа.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);            
        }
        /// <summary>
        /// Функция, добавляющая кредит клиенту
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetNewCredit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfPossibleCredits.SelectedItem != null)
            {
                if (!_bank.GetCredit(_client, TableOfPossibleCredits.SelectedItem as Credit))
                    MessageBox.Show("Вам отказано в кредите!", "Инфо", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                else                   
                    SetPossibleCredits();
            }
            else
                MessageBox.Show("Желаемый кредит не выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        /// <summary>
        /// Функция, добавляющая депозит клиенту
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetNewDeposit_Click(object sender, RoutedEventArgs e)
        {
            Deposit deposit = TableOfPossibleDeposits.SelectedItem as Deposit;
            if (deposit != null)
            {
                if (!_bank.SetDeposit(_client, deposit))
                    MessageBox.Show("Недостаточно средств на счете!", "Инфо", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                else                  
                    SetPossibleDeposits();
            }
            else
                MessageBox.Show("Желаемый депозит не выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        /// <summary>
        /// Функция, вносящая средcтва на депозит
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddMoneyToDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfDeposits.SelectedItem != null)
            {
                PaymentWindow payment = new PaymentWindow();
                payment.ShowDialog();
                if ((bool)payment.DialogResult)
                {
                    if (_client.GetDeposits.GetElem(TableOfDeposits.SelectedItem as Deposit).AddMoneyToDeposit(payment.GetPayment, _client))
                    {                        
                        TableOfDeposits.Items.Refresh();
                        SetPossibleCredits();
                    }
                    else
                        MessageBox.Show("Недостаточно средств на счете.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else            
                MessageBox.Show("Выберите депозит для внесения средств.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);            
        }
        /// <summary>
        ///  /// Функция, снимающая средства с депозита
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GiveMoneyFromDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfDeposits.SelectedItem != null)
            {
                PaymentWindow payment = new PaymentWindow();
                payment.ShowDialog();
                if ((bool)payment.DialogResult)
                {
                    var dep = _client.GetDeposits.GetElem(TableOfDeposits.SelectedItem as Deposit);
                    var result = dep.GetMoneyFromDeposit(payment.GetPayment, _client);
                    if (result)                     
                        SetPossibleDeposits();
                    else if(!result && dep.WithdrawingMoney == "да")
                        MessageBox.Show("Недостаточно средств на депозите.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show("Снятие средств с данного вклада запрещено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("Выберите депозит для снятия средств.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        /// <summary>
        /// Функция, закрывающая депозит и возвращающая всю сумму депозита на счет клиента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EndofDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfDeposits.SelectedItem != null)
            {
                var dep = TableOfDeposits.SelectedItem as Deposit;
                _client.GetDeposits.GetElem(dep).GetMoneyFromDeposit(dep.GetMinSumOfDeposit, _client);
                
                TableOfDeposits.Items.Refresh();
                SetPossibleDeposits();
            }
            else
                MessageBox.Show("Выберите депозит для закрытия.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        /// <summary>
        /// Выход из личного кабинета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Обработка нажатия на кнопку перевода другому лицу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Transaction_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorizationDialogWindow = new Authorization();
            authorizationDialogWindow.Title = "Перевод другому лицу";
            authorizationDialogWindow.Registaion.Visibility = Visibility.Hidden; // скрывает кнопку добавления клиента
            authorizationDialogWindow.EnteredPassword.Visibility = Visibility.Collapsed;
            authorizationDialogWindow.passwordTextBlock.Visibility = Visibility.Collapsed;
            authorizationDialogWindow.TransactionStackPanel.Visibility = Visibility.Visible;
            authorizationDialogWindow.ShowDialog();            
            if (authorizationDialogWindow.Choise == 1)
            {
                try
                {
                    string name = authorizationDialogWindow.GetNameOfClient;
                    uint id = authorizationDialogWindow.GetIdOfClient;
                    BasicClient recipient = null;
                    for (int i = 0; i < _bank.AllBankClient.Count; i++)
                    {
                        if (_bank.AllBankClient[i].Name == name && _bank.AllBankClient[i].Id == id)
                            recipient = _client;
                    }
                    if (recipient != null)
                    {
                        if (_client != recipient)
                        {
                            PaymentWindow payment = new PaymentWindow();
                            payment.ShowDialog();
                            if ((bool)payment.DialogResult)
                                if (_client.GiveTransaction(payment.GetPayment))
                                    recipient.TakeTransaction(payment.GetPayment);
                                else
                                    MessageBox.Show("Недостаточно средств на счете.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                            MessageBox.Show("Нельзя переводить самому себе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                        throw new Exception();
                }
                catch (Exception)
                {
                    MessageBox.Show("Не найдено ни одного клиента с таким именем и id.\n" +
                   "Проверьте данные или зарегистрируйтесь.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        /// <summary>
        /// Обрабатывает нажатие клиента на кнопку внесения средств на счет
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepositBalance_Click(object sender, RoutedEventArgs e)
        {
            PaymentWindow payment = new PaymentWindow();
            payment.ShowDialog();
            if((bool)payment.DialogResult)
            {
                _client.Balance += payment.GetPayment;
                HistoryEvent?.Invoke($"Внесение средств на сумму {payment.GetPayment} р.", 0, _client);
            }
        }
        /// <summary>
        /// Обработка нажатия пользователя на кнопку показа истории
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void History_Click(object sender, RoutedEventArgs e)
        {
            HistoryOperation historyWindow = new HistoryOperation(_client);
            _client.historyOperationWindow = historyWindow;
            historyWindow.ShowDialog();
        }
    }
}
