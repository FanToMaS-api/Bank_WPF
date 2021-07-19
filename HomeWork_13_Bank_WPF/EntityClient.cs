using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace HomeWork_13_Bank_WPF
{
    class EntityClient : BasicClient
    {
        #region .ctor

        public EntityClient(string name, double balance, ref Bank bank)
        {
            this.name = name;
            this.balance = balance;
            password = "0";
            trust = 1.00;
            completedCredit = 0;

            lock (obj)
            {
                salary = salaryArray[2];
                id = NextId();
            }

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
                Debug.Print(e.Message);
            }
        }

        public EntityClient()
        {
            name = null;
            balance = 0;
            salary = salaryArray[2];
            trust = 1.00;
            password = "0";

            credits = new ObservableCollection<Credit>();
            deposits = new ObservableCollection<Deposit>();

            listOfHistoryMessages = new List<string>();
            listTypesOfHistoryMess = new List<int>();
            HistoryEvent += HistoryMessage;
            completedCredit = 0;
        }

        #endregion
    }
}
