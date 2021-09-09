using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using ExtensionLibrary;

namespace HomeWork_13_Bank_WPF.Views
{
    /// <summary>
    /// Логика взаимодействия для PersonalAccount.xaml
    /// </summary>
    public partial class PersonalAccount
    {
        #region Fields

        private readonly BasicClient _client;

        private readonly Bank _bank;

        public event Action<string, int, BasicClient> HistoryEvent;

        #endregion

        #region .ctor

        public PersonalAccount(ref BasicClient client, ref Bank bank)
        {
            InitializeComponent();
            var timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };

            timer.Tick += TimerTick;
            timer.Start();
            _client = client;
            HistoryEvent = BasicClient.HistoryMessage;
            _bank = bank;

            TableOfCredits.ItemsSource = _client.Credits;
            TableOfDeposits.ItemsSource = _client.GetDeposits;
            cmbButtonDep.SelectedIndex = 0;
            InfoOfClient.ItemsSource = new List<BasicClient> { _client }; // скорее всего костыль для работы события PropertyChanged

            SetPossibleCredits();
            SetPossibleDeposits();
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Событие, которое каждую секунду обнновляет время на экране
        /// </summary>
        private void TimerTick(object sender, EventArgs e)
        {
            TimeLabel.Content = DateTime.Now.ToLongTimeString();
        }

        /// <summary>
        ///     Функция, показывающая все оставшиеся депозиты для пользователя
        /// </summary>
        private void SetPossibleDeposits()
        {
            var deposits = _bank.AllBankDeposits;
            foreach (var e in _client.GetDeposits)
            {
                if (deposits.Contains(e))
                {
                    deposits.Remove(e);
                }
            }
            TableOfPossibleDeposits.ItemsSource = deposits;
        }

        /// <summary>
        ///     Функция, показывающая все оставшиеся кредиты для пользователя
        /// </summary>
        private void SetPossibleCredits()
        {
            var credits = _bank.AllBankCredits;
            foreach (var e in _client.Credits)
            {
                if (credits.Contains(e))
                {
                    credits.Remove(e);
                }
            }
            TableOfPossibleCredits.ItemsSource = credits;
        }

        /// <summary>
        ///     Функция, вносящая платеж по кредиту
        /// </summary>
        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfCredits.SelectedItem != null)
            {
                var payment = new PaymentWindow();
                payment.ShowDialog();
                if (payment.DialogResult != null && (bool)payment.DialogResult)
                {
                    if (!_client.PaymentOfCredit(_bank, TableOfCredits.SelectedItem as Credit, payment.GetPayment))
                    {
                        MessageBox.Show("Недостаточно средств на счете.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите кредит для внесения платежа.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     Функция, добавляющая кредит клиенту
        /// </summary>
        private void GetNewCredit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfPossibleCredits.SelectedItem != null)
            {
                if (!_bank.GetCredit(_client, TableOfPossibleCredits.SelectedItem as Credit))
                {
                    MessageBox.Show("Вам отказано в кредите!", "Инфо", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    SetPossibleCredits();
                }
            }
            else
            {
                MessageBox.Show("Желаемый кредит не выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///     Функция, добавляющая депозит клиенту
        /// </summary>
        private void GetNewDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfPossibleDeposits.SelectedItem is Deposit deposit)
            {
                if (!_bank.SetDeposit(_client, deposit))
                {
                    MessageBox.Show("Недостаточно средств на счете!", "Инфо", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    SetPossibleDeposits();
                }
            }
            else
            {
                MessageBox.Show("Желаемый депозит не выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///     Функция, вносящая средcтва на депозит
        /// </summary>
        private void AddMoneyToDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfDeposits.SelectedItem != null)
            {
                var payment = new PaymentWindow();
                payment.ShowDialog();

                if (payment.DialogResult == null || !(bool)payment.DialogResult)
                {
                    return;
                }

                if (_client.GetDeposits.GetElem(TableOfDeposits.SelectedItem as Deposit).AddMoneyToDeposit(payment.GetPayment, _client))
                {
                    TableOfDeposits.Items.Refresh();
                    SetPossibleCredits();
                }
                else
                {
                    MessageBox.Show("Недостаточно средств на счете.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите депозит для внесения средств.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     Функция, снимающая средства с депозита
        /// </summary>
        private void GiveMoneyFromDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfDeposits.SelectedItem != null)
            {
                var payment = new PaymentWindow();
                payment.ShowDialog();

                if (payment.DialogResult == null || !(bool)payment.DialogResult)
                {
                    return;
                }

                var dep = _client.GetDeposits.GetElem(TableOfDeposits.SelectedItem as Deposit);
                var result = dep.GetMoneyFromDeposit(payment.GetPayment, _client);

                if (result)
                {
                    SetPossibleDeposits();
                }
                else if (dep.WithdrawingMoney == "да")
                {
                    MessageBox.Show("Недостаточно средств на депозите.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Снятие средств с данного вклада запрещено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите депозит для снятия средств.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     Функция, закрывающая депозит и возвращающая всю сумму депозита на счет клиента
        /// </summary>
        private void EndOfDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfDeposits.SelectedItem != null)
            {
                if (TableOfDeposits.SelectedItem is Deposit dep)
                {
                    _client.GetDeposits.GetElem(dep).GetMoneyFromDeposit(dep.GetMinSumOfDeposit, _client);
                }

                TableOfDeposits.Items.Refresh();
                SetPossibleDeposits();
            }
            else
            {
                MessageBox.Show("Выберите депозит для закрытия.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     Выход из личного кабинета
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Обработка нажатия на кнопку перевода другому лицу
        /// </summary>
        private void Transaction_Click(object sender, RoutedEventArgs e)
        {
            var authorizationDialogWindow = new Authorization
            {
                Title = "Перевод другому лицу"
            };

            authorizationDialogWindow.Registaion.Visibility = Visibility.Hidden; // скрывает кнопку добавления клиента
            authorizationDialogWindow.EnteredPassword.Visibility = Visibility.Collapsed;
            authorizationDialogWindow.passwordTextBlock.Visibility = Visibility.Collapsed;
            authorizationDialogWindow.TransactionStackPanel.Visibility = Visibility.Visible;
            authorizationDialogWindow.ShowDialog();

            if (authorizationDialogWindow.Choise == 1)
            {
                try
                {
                    var name = authorizationDialogWindow.GetNameOfClient;
                    var id = authorizationDialogWindow.GetIdOfClient;
                    BasicClient recipient = null;
                    foreach (var client in _bank.AllBankClient)
                    {
                        if (client.Name == name && client.Id == id)
                        {
                            recipient = _client;
                        }
                    }

                    if (recipient != null)
                    {
                        if (_client != recipient)
                        {
                            var payment = new PaymentWindow();
                            payment.ShowDialog();

                            if (payment.DialogResult == null || !(bool)payment.DialogResult)
                            {
                                return;
                            }

                            if (_client.GiveTransaction(payment.GetPayment))
                            {
                                recipient.TakeTransaction(payment.GetPayment);
                            }
                            else
                            {
                                MessageBox.Show("Недостаточно средств на счете.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Нельзя переводить самому себе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Не найдено ни одного клиента с таким именем и id.\n" +
                   "Проверьте данные или зарегистрируйтесь.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        ///     Обрабатывает нажатие клиента на кнопку внесения средств на счет
        /// </summary>
        private void DepositBalance_Click(object sender, RoutedEventArgs e)
        {
            var payment = new PaymentWindow();
            payment.ShowDialog();

            if (payment.DialogResult == null || !(bool)payment.DialogResult)
            {
                return;
            }

            _client.Balance += payment.GetPayment;
            HistoryEvent?.Invoke($"Внесение средств на сумму {payment.GetPayment} р.", 0, _client);
        }

        /// <summary>
        ///     Обработка нажатия пользователя на кнопку показа истории
        /// </summary>>
        private void History_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryOperation(_client);
            _client.historyOperationWindow = historyWindow;
            historyWindow.ShowDialog();
        }

        #endregion
    }
}
