using System;
using ExtensionLibrary;
using System.Windows;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Linq;

namespace HomeWork_13_Bank_WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bank _bank;
        PersonalAccount _account = null;
        Authorization _authorizationDialogWindow;
        NewClientDialogWindow _newClientDialogWindow;
        readonly DispatcherTimer _timer0;
        readonly DispatcherTimer _timer1;
        readonly DataBase _dataBase; // объект класса производящего сохранение и выгрузку данных
        public MainWindow()
        {
            InitializeComponent();
            TableOfClients.Visibility = Visibility.Hidden;
            _bank = new Bank();
            _dataBase = new DataBase();
            _dataBase.DataUpload(_bank);
            //MessageBox.Show("Выгрузка данных может занять некоторое время.\nПожалуйста, подождите.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            //bank.GetInfoFromJson();
            #region Заполнение данными для объемных тестов
            // не работает...
            //Task t1 = new Task(() =>
            //{
            //    for (int i = 0; i < 100_000; i++)
            //        new Client($"{i}", i, ref bank);
            //});
            //Task t2 = new Task(() =>
            //{
            //    for (int i = 100_000; i < 200_000; i++)
            //        new Client($"{i}", i, ref bank);
            //});
            //Task t3 = new Task(() =>
            //{
            //    for (int i = 200_000; i < 300_000; i++)
            //        new Client($"{i}", i, ref bank);
            //});
            //Task t4 = new Task(() =>
            //{
            //    for (int i = 300_000; i < 400_000; i++)
            //        new Client($"{i}", i, ref bank);
            //});
            //Task t5 = new Task(() =>
            //{
            //    for (int i = 400_000; i < 500_000; i++)
            //        new Client($"{i}", i, ref bank);
            //});
            //t1.Start();
            //t2.Start();
            //t3.Start();
            //t4.Start();
            //t5.Start();
            //Task.WaitAll();
            #endregion

            TableOfClients.ItemsSource = _bank.AllBankClient;
            _timer0 = new DispatcherTimer();     // таймер для вывода времени
            _timer1 = new DispatcherTimer();     // таймер для начисления зарплат и перерасчета кредитов и депозитов
            _timer0.Tick += new EventHandler(TimerTick0);
            _timer0.Interval = new TimeSpan(0, 0, 1);
            _timer1.Tick += new EventHandler(TimerTick1);
            _timer1.Interval = new TimeSpan(0, 0, 10);
            _timer0.Start();
            _timer1.Start();
        }
        /// <summary>
        /// Событие, которое каждую секунду обнновляет время на экране
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick0(object sender, EventArgs e)
        {
            TimeLabel.Content = DateTime.Now.ToLongTimeString();
        }
        /// <summary>
        /// Событие, которое каждые 30 сек. вызывает функции начисления зарплат и перерасчета кредитов и депозитов
        /// А также сохраняет данные о банке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick1(object sender, EventArgs e)
        {
            for (int i = 0; i < _bank.AllBankClient.Count; i++)
            {
                _bank.AllBankClient[i].GetSalary();
            }
            //Parallel.For(0, bank.AllBankClient.Count, ParalleGetSalary); // при параллельном начислении зарплат и открытом окне история операций
            //возникает исключение: "Вызывающим потоком должен быть поток-STA"
            if (_bank.AllBankClient.Count != 0)
            {
                for (int i = 0; i < _bank.AllBankClient.Count; i++)
                {
                    if (_bank.AllBankClient[i].GetDeposits.Count != 0)
                        foreach (var dep in _bank.AllBankClient[i].GetDeposits)
                        {
                            dep.Calculation();
                        }
                    if (_bank.AllBankClient[i].Credits.Count != 0)
                        foreach (var credit in _bank.AllBankClient[i].Credits)
                        {
                            if (!_bank.AllBankClient[i].PaymentOfCredit(_bank, credit, credit.GetDailyPayment))
                            {
                                credit.Sanction(_bank.AllBankClient[i]);
                            }
                            credit.Calculation();
                        }
                }
            }
        }
        /// <summary>
        /// Начисляет зарплату в многопоточном режиме
        /// </summary>
        /// <param name="i"></param>
        //private void ParalleGetSalary(int i)
        //{
        //    bank.AllBankClient[i].GetSalary();
        //}
        /// <summary>
        /// Возвращает клиента, если клиент ввел свои данные верно или зарегистрировался успешно
        /// иначе возвращает null
        /// </summary>
        /// <returns></returns>
        private BasicClient Identification()
        {
            _authorizationDialogWindow = new Authorization();
            _authorizationDialogWindow.ShowDialog();
            if (_authorizationDialogWindow.Choise == 1)
            {
                try
                {
                    string password = _authorizationDialogWindow.GetPasswordOfClient;
                    return _bank[_authorizationDialogWindow.GetNameOfClient, password];
                }
                catch (Exception)
                {
                    MessageBox.Show("Некорректно введены данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (_authorizationDialogWindow.Choise == 0) // значит, что клиент новый и решил забить свои данные в первый раз.
            {
                _newClientDialogWindow = new NewClientDialogWindow();
                _newClientDialogWindow.ShowDialog();
                BasicClient client = _newClientDialogWindow.GetChoise;
                if ((bool)_newClientDialogWindow.DialogResult && client != null)
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
                    MessageBox.Show($"Ваше имя: {client.Name}\nВаш пароль: {client.Password}\nВаш id: {client.Id}.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                    return _bank[client.Name, client.Password];
                }
            }
            return null;
        }
        /// <summary>
        /// Функция, отвечающая за работу клиента в своем личном кабинете
        /// </summary>
        /// <param name="client"></param>
        private void PersonalAccountFunc(ref BasicClient client)
        {
            _account = new PersonalAccount(ref client, ref _bank);
            _account.ShowDialog();
        }
        /// <summary>
        /// Обработчик события нажатия кнопки для входа в режим разработчика
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Master_Click(object sender, RoutedEventArgs e)
        {
            Password password = new Password();
            password.ShowDialog();
            if ((bool)password.DialogResult)
            {
                TableOfClients.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// Выполняется при закрытии основного окна Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer0.Stop();
            _timer1.Stop();
            _dataBase.DataSave(_bank);
            //MessageBox.Show("Выгрузка данных может занять некоторое время.\nПожалуйста, подождите.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            //bank.Save();
            Application.Current.Shutdown();
        }
        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            BasicClient client = Identification();
            if (client != null)
            {
                PersonalAccountFunc(ref client);
            }
            else if (_authorizationDialogWindow.Choise == 1)
                MessageBox.Show("Не найдено ни одного клиента с таким именем и паролем.\n" +
        "Проверьте данные или зарегистрируйтесь.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            else if ((bool)_authorizationDialogWindow.DialogResult && (bool)_newClientDialogWindow.DialogResult)
            {
                MessageBox.Show("Пользователь с такими данными уже зарегистрирован.\n" +
        "Введите другой пароль или имя.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
