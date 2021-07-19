using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HomeWork_13_Bank_WPF
{
    public enum OppotunityWithdrawingMoney
    {
        Yes,
        No
    }
    /// <summary>
    ///     Возможность пополнения вклада
    /// </summary>
    public enum OppotunityReplenishmentDeposit
    {
        Yes,
        No
    }

    public class Deposit : IEquatable<Deposit>, ICloneable, INotifyPropertyChanged
    {
        #region Fields

        private double _deposit;     // сумма депозита
        private double _percent;     // процент 
        private int _days;           // срок в днях
        private string _name;        // название вклада
        private OppotunityWithdrawingMoney _withdrawing;          // Возможность снятия средств
        private OppotunityReplenishmentDeposit _replenishment;    // Возможность пополнения вклада

        public event PropertyChangedEventHandler PropertyChanged; // Отражает изменение полей депозита без обновления всей таблицы
        protected event Action<string, int, BasicClient> HistoryEvent;

        #endregion

        #region .ctor

        public Deposit()
        {
            _deposit = 0;
            _percent = 0;
            _days = 0;
            _replenishment = OppotunityReplenishmentDeposit.No;
            _withdrawing = OppotunityWithdrawingMoney.No;

        }
        public Deposit(string name, double deposit, double percent, int days, OppotunityWithdrawingMoney oppotunity,
            OppotunityReplenishmentDeposit replenishmentDeposit)
        {
            _name = name;
            _deposit = deposit;
            _percent = percent;
            _days = days;
            _withdrawing = oppotunity;
            _replenishment = replenishmentDeposit;
        }

        #endregion

        #region Properties

        /// <inheritdoc cref="OppotunityWithdrawingMoney"/>>
        public OppotunityWithdrawingMoney WithdrawingMoneyForBank => _withdrawing;

        /// <inheritdoc cref="OppotunityReplenishmentDeposit"/>>
        public OppotunityReplenishmentDeposit ReplenishmentDepositForBank => _replenishment;

        /// <summary>
        ///     Возвращает и устанавливает название вкалада
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     Возвращает и устанавливает сумму депозита
        /// </summary>
        public double GetMinSumOfDeposit
        {
            get => _deposit;
            set
            {
                _deposit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.GetMinSumOfDeposit)));
            }
        }

        /// <summary>
        ///     Возвращает и устанавливает процент по вкладу
        /// </summary>
        public double Percent
        {
            get => _percent;
            set => _percent = value;
        }

        /// <summary>
        ///     Возвращает и устанавливает кол-во дней "существования" вклада
        /// </summary>
        public int Days
        {
            get => _days;
            set
            {
                _days = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Days)));
            }
        }

        /// <summary>
        ///     Показывает и устанавливает возможность снятия средств со вклада
        /// </summary>
        public string WithdrawingMoney
        {
            get => _withdrawing == OppotunityWithdrawingMoney.No ? "нет" : "да";
            set => _withdrawing = value == "нет" ? OppotunityWithdrawingMoney.No : OppotunityWithdrawingMoney.Yes;
        }

        /// <summary>
        ///     Показывает и устанавливает возможность пополнения вклада
        /// </summary>
        public string ReplenishmentDeposit
        {
            get => _replenishment == OppotunityReplenishmentDeposit.No ? "нет" : "да";
            set => _replenishment = value == "нет" ? OppotunityReplenishmentDeposit.No : OppotunityReplenishmentDeposit.Yes;
        }

        /// <summary>
        ///     Возвращает сумму еждедневного пополнения счета депозита
        /// </summary>
        public double GetDailySum => _deposit * _percent / 100 / _days;

        #endregion

        #region Public methods

        /// <summary>
        ///     Возвращает депозит, если все прошло успешно, null - если создание депозита не удалось
        /// </summary>
        public Deposit GetDeposit()
        {
            switch (_days)
            {
                case 365 * 5 when _deposit >= 200_000 && _deposit < 800_000:
                    _percent = 3.5;
                    PercentCalculation();
                    return this;
                case 365 * 3 when _deposit >= 3_000_000 && _withdrawing != OppotunityWithdrawingMoney.Yes && _replenishment != OppotunityReplenishmentDeposit.Yes:
                    _percent = 4.0;
                    return this;
                case 365 when _deposit >= 800_000 && _withdrawing != OppotunityWithdrawingMoney.Yes && _deposit < 2_500_000:
                    _percent = 4.5;
                    PercentCalculation();
                    return this;
                case 365 / 2 when _deposit >= 40_000 && _deposit < 800_000:
                    _percent = 6.0;
                    PercentCalculation();
                    return this;
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Добавляет проценты к депозиту каждый день (каждые 30 секунд)
        /// </summary>
        public void Calculation()
        {
            if (_days <= 0)
            {
                return;
            }

            GetMinSumOfDeposit += _deposit * _percent / 100 / 365;
            Days--;
        }

        /// <summary>
        ///     Возвращает true, если снятие произошло успешно
        /// </summary>
        public bool GetMoneyFromDeposit(double value, BasicClient client)
        {
            if (_withdrawing != OppotunityWithdrawingMoney.Yes || !(_deposit - value >= 0))
            {
                return false;
            }

            client.Balance += value;
            GetMinSumOfDeposit -= value;

            HistoryEvent = BasicClient.HistoryMessage;
            HistoryEvent?.Invoke($"Снятие средств с депозита {_name} на сумму {value:#.##} руб.", 0, client);

            if (_deposit == 0)
            {
                client.GetDeposits.Remove(this);
            }

            return true;
        }

        /// <summary>
        ///     Добавляет средства на счет вклада, если предусмотрена такая возможность и возвращает true 
        ///     если все прошло успешно, иначе возвращает false
        /// </summary>
        public bool AddMoneyToDeposit(double value, BasicClient client)
        {
            if (_replenishment != OppotunityReplenishmentDeposit.Yes || !(client.Balance - value >= 0))
            {
                return false;
            }

            client.Balance -= value;
            GetMinSumOfDeposit += value;

            HistoryEvent = BasicClient.HistoryMessage;
            HistoryEvent?.Invoke($"Добавление средств на депозит {_name} на сумму {value:#.##} руб.", 1, client);

            return true;
        }

        /// <summary>
        ///     Возвращает депозиты со стандартными условиями
        /// </summary>
        public static ObservableCollection<Deposit> GetBasicDeposits()
        {
            var deposits = new ObservableCollection<Deposit>
            {
                new Deposit("Сохраняй", 40000, 6.0, 182, OppotunityWithdrawingMoney.Yes,
                    OppotunityReplenishmentDeposit.Yes),
                new Deposit("Успешный", 200000, 3.5, 1825, OppotunityWithdrawingMoney.Yes,
                    OppotunityReplenishmentDeposit.Yes),
                new Deposit("Выгодный", 800000, 4.5, 365, OppotunityWithdrawingMoney.No,
                    OppotunityReplenishmentDeposit.Yes),
                new Deposit("Перспективный", 3000000, 4.0, 1095, OppotunityWithdrawingMoney.No,
                    OppotunityReplenishmentDeposit.No)
            };

            return deposits;
        }

        /// <summary>
        ///     Сравнение депозитов на равенство
        /// </summary>
        public bool Equals(Deposit other)
        {
            return other != null && (_name == other._name);
        }

        /// <summary>
        ///     Создает копию депозита
        /// </summary>
        public object Clone()
        {
            return new Deposit(_name, _deposit, _percent, _days, _withdrawing, _replenishment);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Функция расчета процентов по вкладу в зависимости от условий
        ///     Также устанавливает условия по вкладу
        /// </summary>
        private void PercentCalculation()
        {
            if (_withdrawing == OppotunityWithdrawingMoney.Yes)
            {
                _percent -= 0.75;
            }

            if (_replenishment == OppotunityReplenishmentDeposit.Yes)
            {
                _percent -= 1;
            }
        }

        #endregion
    }
}
