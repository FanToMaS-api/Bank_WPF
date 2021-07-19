using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HomeWork_13_Bank_WPF
{
    public class Credit : IEquatable<Credit>, ICloneable, INotifyPropertyChanged
    {
        #region Fields

        private double _credit; // сумма кредита
        private double _percent; // процент 
        private readonly int _days; // срок в днях
        private double _sanction; // санкция при просрочке внесения платы по кредиту
        private double _fine; // общая сумма штрафа по просрочкам
        private readonly string _name; // название кредита
        public event PropertyChangedEventHandler PropertyChanged;
        private event Action<string, int, BasicClient> HistoryEvent;

        #endregion

        #region .ctor

        public Credit(double credit, double percent, int days, string name, double fine)
        {
            _credit = credit;
            _percent = percent;
            _days = days;
            _fine = fine;
            _sanction = 0;
            _name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Возвращает число дней, на которые взяли кредит
        /// </summary>
        public int Days => _days;

        /// <summary>
        ///     Возвращает общую сумму штрафа по кредиту
        /// </summary>
        public double Fine
        {
            get => _fine;
            set
            {
                _fine = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Fine)));
            }
        }

        /// <summary>
        ///     Возвращает или устанавливает число процентов, под которое взяли кредит
        /// </summary>
        public double Percent
        {
            get => _percent;
            set => _percent = value;
        }

        /// <summary>
        ///     Возвращает остаток по кредиту
        /// </summary>
        public double GetCredit
        {
            get => _credit;
            set
            {
                _credit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.GetCredit)));
            }
        }

        /// <summary>
        ///     Возвращает название кредита
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     Возвращает сумму ежедневного платежа
        /// </summary>
        public double GetDailyPayment => _credit / _days;

        #endregion

        #region Public methods

        /// <summary>
        ///     Возвращает лист стандартных кредитов для банка
        /// </summary>
        public static ObservableCollection<Credit> GetBasicCredits()
        {
            var credits = new ObservableCollection<Credit>
            {
                new Credit(50_000, 25, 30, "Срочный", 0),
                new Credit(100_000, 12, 60, "Необходимый", 0),
                new Credit(300_000, 7, 180, "Желаемый", 0),
                new Credit(500_000, 5, 365, "Целевой", 0),
                new Credit(1_000_000, 5, 365 * 2, "На мечту", 0),
                new Credit(5_000_000, 3.3, 365 * 3, "Удачная ипотека", 0)
            };

            return credits;
        }

        /// <summary>
        ///     Добавляет проценты к кредиту
        /// </summary>
        public void Calculation()
        {
            GetCredit += _credit * Percent / 100 / 365;
        }

        /// <summary>
        ///     Функция начисления штрафных пени
        /// </summary>
        public void Sanction(BasicClient client)
        {
            // санкция при неуплате нужной суммы возврата денег по кредиту за день
            _sanction = GetCredit / Days / 10 + _fine / 10;
            GetCredit += _sanction;
            Fine += _sanction;

            HistoryEvent = BasicClient.HistoryMessage;
            HistoryEvent?.Invoke($"Начислена санкиця по кредиту {_name} на сумму {_sanction:#.##} руб.", 2, client);

            if (client.Trust - 0.05 < 0)
            {
                client.Trust = 0;
            }
            else
            {
                client.Trust -= 0.05;
            }
        }

        /// <summary>
        ///     Уменьшает остаток по кредиту на payment
        /// </summary>
        public double Payment(Bank bank, double value)
        {
            if (_credit - value < 0)
            {
                bank.TakePayment(_credit);
                var temp = _credit;
                GetCredit = 0;
                return -(temp - value);
            }

            bank.TakePayment(value);
            GetCredit -= value;
            return 0;
        }

        /// <summary>
        ///     Реализация интерфейса сравнения двух объектов класса
        ///     для заполнения Таблицы возможных кредитов (SetPossibleCredits)
        /// </summary>
        public bool Equals(Credit other)
        {
            return other != null && (_name == other._name);
        }

        /// <summary>
        ///     Возвращает копию кредита
        /// </summary>
        public object Clone()
        {
            return new Credit(_credit, _percent, _days, _name, _fine);
        }

        #endregion
    }
}
