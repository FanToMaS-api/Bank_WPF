using System;
using System.Windows;
using System.Windows.Threading;

namespace HomeWork_13_Bank_WPF.Views
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private Bank _bank;

        private PersonalAccount _account;

        private Authorization _authorizationDialogWindow;

        private NewClientDialogWindow _newClientDialogWindow;

        private readonly DispatcherTimer _timer0;

        private readonly DispatcherTimer _timer1;

        private readonly DataBase _dataBase; // объект класса производящего сохранение и выгрузку данных

        #endregion

        #region .ctor

        public MainWindow()
        {
            InitializeComponent();
            TableOfClients.Visibility = Visibility.Hidden;
            _bank = new Bank();
            _bank.GetInfoFromJson();

            //_dataBase = new DataBase();
            // _dataBase.DataUpload(_bank);

            TableOfClients.ItemsSource = _bank.AllBankClient;
            _timer0 = new DispatcherTimer();     // таймер для вывода времени
            _timer1 = new DispatcherTimer();     // таймер для начисления зарплат и перерасчета кредитов и депозитов
            _timer0.Tick += TimerTick0;

            _timer0.Interval = new TimeSpan(0, 0, 1);
            _timer1.Tick += TimerTick1;
            _timer1.Interval = new TimeSpan(0, 0, 10);
            _timer0.Start();
            _timer1.Start();
        }

        #endregion

        /// <summary>
        ///     Событие, которое каждую секунду обнновляет время на экране
        /// </summary>
        private void TimerTick0(object sender, EventArgs e)
        {
            TimeLabel.Content = DateTime.Now.ToLongTimeString();
        }

        /// <summary>
        ///     Событие, которое каждые 30 сек. вызывает функции начисления зарплат и перерасчета кредитов и депозитов
        ///     А также сохраняет данные о банке
        /// </summary>
        private void TimerTick1(object sender, EventArgs e)
        {
            foreach (var client in _bank.AllBankClient)
            {
                client.GetSalary();
            }

            //Parallel.For(0, bank.AllBankClient.Count, ParalleGetSalary); // при параллельном начислении зарплат и открытом окне история операций
            //возникает исключение: "Вызывающим потоком должен быть поток-STA"
            if (_bank.AllBankClient.Count != 0)
            {
                foreach (var client in _bank.AllBankClient)
                {
                    if (client.GetDeposits.Count != 0)
                    {
                        foreach (var dep in client.GetDeposits)
                        {
                            dep.Calculation();
                        }
                    }

                    if (client.Credits.Count == 0)
                    {
                        continue;
                    }

                    foreach (var credit in client.Credits)
                    {
                        if (!client.PaymentOfCredit(_bank, credit, credit.GetDailyPayment))
                        {
                            credit.Sanction(client);
                        }
                        credit.Calculation();
                    }
                }
            }
        }

        /// <summary>
        ///     Возвращает клиента, если клиент ввел свои данные верно или зарегистрировался успешно
        ///     иначе возвращает null
        /// </summary>
        private BasicClient Identification()
        {
            _authorizationDialogWindow = new Authorization();
            _authorizationDialogWindow.ShowDialog();

            switch (_authorizationDialogWindow.Choise)
            {
                case 1:
                    try
                    {
                        var password = _authorizationDialogWindow.GetPasswordOfClient;
                        return _bank[_authorizationDialogWindow.GetNameOfClient, password];
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Некорректно введены данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    break;
                // значит, что клиент новый и решил забить свои данные в первый раз.
                case 0:
                {
                    _newClientDialogWindow = new NewClientDialogWindow();
                    _newClientDialogWindow.ShowDialog();

                    var client = _newClientDialogWindow.GetChoise;

                    if (_newClientDialogWindow.DialogResult != null && (bool)_newClientDialogWindow.DialogResult && client != null)
                    {
                        client.Balance = 0;
                        client.Name = _newClientDialogWindow.GetName;
                        client.Password = _newClientDialogWindow.GetPassword;

                        if (_bank.ListPasswordsHash.Count != 0 && _bank.ListPasswordsHash.Contains(client.GetHash))
                        {
                            return null;
                        }

                        client.Id = BasicClient.NextId();
                        _bank.AddClient(client);
                        TableOfClients.Items.Refresh();
                        MessageBox.Show($"Ваше имя: {client.Name}\nВаш пароль: {client.Password}\nВаш id: {client.Id}.",
                            "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);

                        return _bank[client.Name, client.Password];
                    }

                    break;
                }
            }
            return null;
        }

        /// <summary>
        ///     Функция, отвечающая за работу клиента в своем личном кабинете
        /// </summary>
        private void PersonalAccountFunc(ref BasicClient client)
        {
            _account = new PersonalAccount(ref client, ref _bank);
            _account.ShowDialog();
        }

        /// <summary>
        ///     Обработчик события нажатия кнопки для входа в режим разработчика
        /// </summary>
        private void Master_Click(object sender, RoutedEventArgs e)
        {
            var password = new Password();
            password.ShowDialog();

            if (password.DialogResult != null && (bool)password.DialogResult)
            {
                TableOfClients.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        ///     Выполняется при закрытии основного окна Window
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer0.Stop();
            _timer1.Stop();
            // _dataBase.DataSave(_bank);
            MessageBox.Show("Выгрузка данных может занять некоторое время.\nПожалуйста, подождите.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            _bank.Save();
            Application.Current.Shutdown();
        }
        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            var client = Identification();
            if (client != null)
            {
                PersonalAccountFunc(ref client);
            }
            else if (_authorizationDialogWindow.Choise == 1)
            {
                MessageBox.Show("Не найдено ни одного клиента с таким именем и паролем.\n" +
        "Проверьте данные или зарегистрируйтесь.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (_newClientDialogWindow.DialogResult != null
                     && _authorizationDialogWindow.DialogResult != null
                     && (bool)_authorizationDialogWindow.DialogResult &&
                     (bool)_newClientDialogWindow.DialogResult)
            {
                MessageBox.Show("Пользователь с такими данными уже зарегистрирован.\n" +
        "Введите другой пароль или имя.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
