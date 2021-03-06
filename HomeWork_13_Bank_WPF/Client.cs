using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace HomeWork_13_Bank_WPF
{
    /// <summary>
    ///     Обычный клиент банка
    /// </summary>
    internal class Client : BasicClient
    {
        #region .ctor

        /// <inheritdoc cref="Client"/>
        public Client(string name, double balance, ref Bank bank)
        {
            this.name = name;
            this.balance = balance;
            completedCredit = 0;

            lock (obj)
            {
                salary = salaryArray[0];
                id = NextId();
            }

            password = "0";
            trust = 1.00;
            credits = new ObservableCollection<Credit>();
            deposits = new ObservableCollection<Deposit>();

            listOfHistoryMessages = new List<string>();
            listTypesOfHistoryMess = new List<int>();
            HistoryEvent += HistoryMessage;


            try
            {
                bank.AddClient(this);
            }
            catch (Exception e)
            {
                // TODO: использовать Serilog
                Debug.Print(e.Message);
            }

        }

        /// <inheritdoc cref="Client"/>
        public Client()
        {
            name = "";
            balance = 0;
            trust = 1.00;
            password = "0";
            completedCredit = 0;
            salary = salaryArray[0];

            credits = new ObservableCollection<Credit>();
            deposits = new ObservableCollection<Deposit>();


            listOfHistoryMessages = new List<string>();
            listTypesOfHistoryMess = new List<int>();
            HistoryEvent += HistoryMessage;
        }

        #endregion
    }
}
