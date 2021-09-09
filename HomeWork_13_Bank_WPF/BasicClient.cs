using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using HomeWork_13_Bank_WPF.Views;

namespace HomeWork_13_Bank_WPF
{
    /// <summary>
    ///     Базовый класс клиента банка
    /// </summary>
    public abstract class BasicClient : INotifyPropertyChanged
    {
        #region Fields

        protected static uint staticId;

        protected object obj = new object(); // объект для блокировки при параллельном сохранении, занесении данных

        protected uint id;  // id клиента

        protected string name; // имя клиента

        protected string password; // пароль клиента

        protected double balance; // баланс

        protected double trust; // уровень доверия к клиенту

        protected ObservableCollection<Credit> credits; // кредиты клиента

        protected ObservableCollection<Deposit> deposits; // вклады клиента

        protected uint completedCredit; // завершенные кредиты

        protected double salary; // зарплата клиента

        protected event Action<string, int, BasicClient> HistoryEvent; // событие, реагирующее на изменение баланса клиента

        public static int[] salaryArray; // массив, содержащий в себе зарплаты всех типов клиентов на индексе 0 - зарплата обычного клиента
                                         // на индексе 1 - зарплата VIP-клиента, на индексе 2 - зарплата юридческого лица

        public List<string> listOfHistoryMessages; // лист сообщений в истории клиента

        public List<int> listTypesOfHistoryMess; // лист типов (раскраски) сообщений в истории клиента

        public HistoryOperation historyOperationWindow; // окно для инициализации собственной истории сообщений для каждого клиента

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region .ctor

        /// <summary>
        ///     Статический конструктор для инициализации id
        /// </summary>
        static BasicClient()
        {
            staticId = 0;
            salaryArray = new[] { 1000, 2000, 3000 };  // массив, содержащий в себе зарплаты всех типов клиентов
                                                       // на индексе 0 - зарплата обычного клиента,
                                                       // на индексе 1 - зарплата VIP-клиента,
                                                       // на индексе 2 - зарплата юридческого лица
        }

        #endregion

        /// <summary>
        ///     Взоврат уникального id
        /// </summary>
        /// <returns></returns>
        public static uint NextId()
        {
            return staticId++;
        }

        #region Properties

        /// <summary>
        ///     Возвращает и задает пароль клиента
        /// </summary>
        public string Password
        {
            get => password;
            set => password = value;
        }

        /// <summary>
        ///     Возвращает хеш-код идентифицирующий пользователя
        /// </summary>
        public int GetHash => (name + password).GetHashCode() + "alkjdfn".GetHashCode(); // истинный хеш + хеш "соли"

        /// <summary>
        ///     Возвращает или задает остаток на балансе клиента
        /// </summary>
        public double Balance
        {
            get => balance;
            set
            {
                balance = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Balance)));
            }
        }

        /// <summary>
        ///     Возвращает или задает имя клиента
        /// </summary>
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        ///     Возвращает или устанавливает id клиента
        /// </summary>
        public uint Id
        {
            get => id;
            set => id = value;
        }

        /// <summary>
        ///     Возвращает или устанавливает уровень доверия к клиенту
        /// </summary>
        public double Trust
        {
            get => trust;
            set
            {
                trust = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Trust)));
            }
        }

        /// <summary>
        ///     Возвращает или задает лист депозитов клиента
        /// </summary>
        public ObservableCollection<Deposit> GetDeposits
        {
            get => deposits;
            set => deposits = value;
        }

        /// <summary>
        ///     Возвращает или задает лист кредитов клиента
        /// </summary>
        public ObservableCollection<Credit> Credits
        {
            get => credits;
            set => credits = value;
        }

        /// <summary>
        ///     Возвращает или задает кол-во выплаченных кредитов
        /// </summary>
        public uint CompletedCredit
        {
            get => completedCredit;
            set
            {
                completedCredit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompletedCredit)));
            }
        }

        /// <summary>
        ///     Возвращает или задает зарплату клиента
        /// </summary>
        public double Salary
        {
            get => salary;
            set => salary = value;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///    Заносит в историю сообщение
        /// </summary>
        public static void HistoryMessage(string s, int i, BasicClient client)
        {
            if (client.historyOperationWindow != null)
            {
                client.historyOperationWindow.HistoryMessage(s, i);
            }

            client.listOfHistoryMessages.Add(s);
            client.listTypesOfHistoryMess.Add(i);
        }

        /// <summary>
        ///     Отнимает от счета пользователя определeнную сумму
        /// </summary>
        public bool GiveTransaction(double value)
        {
            if (!(Balance - value >= 0))
            {
                return false;
            }

            Balance -= value;
            HistoryEvent?.Invoke($"Перевод на сумму {value:#.##} руб.", 1, this);

            return true;
        }

        /// <summary>
        ///     Добавляет к счету пользователя переведенную сумму
        /// </summary>
        public void TakeTransaction(double value)
        {
            Balance += value;
            HistoryEvent?.Invoke($"Получение перевода на сумму {value:#.##} руб.", 0, this);
        }

        /// <summary>
        ///     Взнос по кредиту, если кредит выплачен, удаляет его из активных
        ///     Возвращает true, если платеж прошел, иначе - false
        ///     Добавляет +1 к завершенным кредитам, если кредит выплачен
        /// </summary>
        public bool PaymentOfCredit(Bank bank, Credit credit, double payment)
        {
            if (!(balance - payment >= 0))
            {
                return false;
            }

            Balance -= payment;
            Balance += credit.Payment(bank, payment);
            HistoryEvent?.Invoke($"Платеж по кредиту на сумму {payment:#.##} руб.", 1, this);

            if (credit.GetCredit != 0)
            {
                return true;
            }

            completedCredit++;
            trust += 0.2;
            credits.Remove(credit);

            return true;
        }

        /// <summary>
        ///     Начисляет зарплату в зависимости от класса клиента
        /// </summary>
        public void GetSalary()
        {
            Balance += salary;
            HistoryEvent?.Invoke($"Зачисление зарплаты {salary:#.##} руб.", 0, this);
        }

        /// <summary>
        ///     Возвращает имя и баланс на счете клиента
        /// </summary>
        public override string ToString()
        {
            return $"Id: {id} " + $"Name: {name} " + $"Balance: {balance} " + $"Trust: {trust} ";
        }

        #endregion
    }
}
