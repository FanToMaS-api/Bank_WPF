using System;
using ExtensionLibrary;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace HomeWork_13_Bank_WPF
{
    public class Bank
    {
        #region Fields

        private ObservableCollection<Credit> _allBankCredits; // все кредиты банка
        private readonly ObservableCollection<BasicClient> _allBankClient; // все клиенты банка
        private readonly List<int> _listPasswordsHash; // все клиенты банка
        private double _allMoney; // все средства которыми располагает банк на данный момент
        private ObservableCollection<Deposit> _allBankDeposits;
        public event Action<string, int, BasicClient> HistoryEvent;
        private JArray _clientsArr;
        private JToken[] _arrOfClients;

        #endregion

        #region .ctor

        public Bank()
        {
            _allBankClient = new ObservableCollection<BasicClient>();
            _allBankDeposits = new ObservableCollection<Deposit>();
            _allBankCredits = new ObservableCollection<Credit>();
            _listPasswordsHash = new List<int>();
            _allMoney = 50_000_000;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Возврат НЕЗАВИСИМОЙ копии всех кредитов
        /// </summary>
        public ObservableCollection<Credit> AllBankCredits
        {
            get
            {
                ObservableCollection<Credit> credits = new ObservableCollection<Credit>();

                for (var i = 0; i < _allBankCredits.Count; i++)
                {
                    credits.Add(new Credit(_allBankCredits[i].GetCredit, _allBankCredits[i].Percent,
                        _allBankCredits[i].Days, _allBankCredits[i].Name, _allBankCredits[i].Fine));
                }

                return credits;
            }
        }

        /// <summary>
        ///     Возвращает всех клиентов банка
        /// </summary>
        public ObservableCollection<BasicClient> AllBankClient => _allBankClient;

        /// <summary>
        ///     Возвращает НЕЗАВИСИМУЮ копию всех депозитов
        /// </summary>
        public ObservableCollection<Deposit> AllBankDeposits
        {
            get
            {
                ObservableCollection<Deposit> deposits = new ObservableCollection<Deposit>();

                for (var i = 0; i < _allBankDeposits.Count; i++)
                {
                    deposits.Add(new Deposit(_allBankDeposits[i].Name, _allBankDeposits[i].GetMinSumOfDeposit,
                        _allBankDeposits[i].Percent, _allBankDeposits[i].Days,
                        _allBankDeposits[i].WithdrawingMoneyForBank, _allBankDeposits[i].ReplenishmentDepositForBank));
                }

                return deposits;
            }
        }

        /// <summary>
        ///     Возвращает хеш-лист всех пользователей
        /// </summary>
        public List<int> ListPasswordsHash => _listPasswordsHash;

        /// <summary>
        ///     Индексатор, возвращающий клиента банка по его паролю и имени
        ///     Если указанного клиента нет - возвращает null
        /// </summary>
        public BasicClient this[string name, string password]
        {
            get
            {
                var t = new Client { Name = name, Password = password };
                var temp = t.GetHash;

                return _listPasswordsHash.Contains(temp) ? _allBankClient.ElementAt(_listPasswordsHash.IndexOf(temp)) : null;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Добавление клиента в базу данных банка
        /// </summary>
        public void AddClient(BasicClient client)
        {
            _allBankClient.Add(client);
            _listPasswordsHash.Add(client.GetHash);
        }

        /// <summary>
        ///     Возвращает true, если заемщик получил кредит
        /// </summary>
        public bool GetCredit(BasicClient client, Credit credit)
        {
            if (!(_allMoney - credit.GetCredit > 0))
            {
                return false;
            }

            switch (client.Trust > 0.71)
            {
                case true when credit.GetCredit >= 1_000_000 && client.Credits.Count <= 1:
                    return SettingsCredit(ref client, ref credit);
                case true when credit.GetCredit > 200_000 && credit.GetCredit < 1_000_000 &&
                               client.Credits.Count <= 2:
                    return SettingsCredit(ref client, ref credit);
                default:
                    {
                        if (client.Trust > 0.81 && credit.GetCredit > 40_000 && credit.GetCredit < 200_000 &&
                            client.Credits.Count <= 2)
                        {
                            return SettingsCredit(ref client, ref credit);
                        }

                        return false;
                    }
            }
        }

        /// <summary>
        ///     Добавляет базовый депозит
        /// </summary>
        public void AddBasicDeposit(Deposit deposit)
        {
            _allBankDeposits.Add(deposit);
        }

        /// <summary>
        ///     Добавляет базовый кредит
        /// </summary>
        public void AddBasicCredit(Credit credit)
        {
            _allBankCredits.Add(credit);
        }

        /// <summary>
        /// Возвращает true, если заемщик получил депозит
        /// </summary>
        public bool SetDeposit(BasicClient client, Deposit deposit)
        {
            if (!(client.Balance - deposit.GetMinSumOfDeposit >= 0))
            {
                return false;
            }

            deposit = deposit.GetDeposit();

            if (deposit == null)
            {
                return false;
            }

            _allMoney += deposit.GetMinSumOfDeposit;
            client.GetDeposits.Add(deposit);
            client.Balance -= deposit.GetMinSumOfDeposit;

            HistoryEvent = BasicClient.HistoryMessage;
            HistoryEvent?.Invoke($"Сохранение депозита {deposit.Name} на сумму {deposit.GetMinSumOfDeposit:#.##} руб.", 1, client);

            return true;
        }

        /// <summary>
        ///     Зачисляет взнос по кредиту на счет банка
        /// </summary>
        public void TakePayment(double payment)
        {
            _allMoney += payment;
        }

        /// <summary>
        ///     Сохраняет всю информацию о банке в json-файл
        /// </summary>
        public void Save()
        {
            var mainTree = new JObject
            {
                ["Средства банка"] = _allMoney,
                ["Кредиты банка"] = SaveCredits(_allBankCredits),
                ["Депозиты банка"] = SaveDeposits(_allBankDeposits),
                ["Клиенты банка"] = SaveClients()
            };

            File.WriteAllText("BankJson.json", mainTree.ToString());
        }

        /// <summary>
        ///     Выгружает полную информацию о банке с json-файла
        ///     Возвращает экземпляр класса Bank
        /// </summary>
        public void GetInfoFromJson()
        {
            if (!File.Exists("BankJson.json"))
            {
                return;
            }

            var str = File.ReadAllText("BankJson.json");
            _allMoney = JObject.Parse(str)["Средства банка"].ToString().ToDouble();

            Task.Factory.StartNew(() => _allBankCredits = SetCreditFromJson(str, "Кредиты банка"));
            Task.Factory.StartNew(() => _allBankDeposits = SetDepositFromJson(str, "Депозиты банка"));

            SetClientsFromJson(str);
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Устанавливает свойства кредита
        /// </summary>
        private bool SettingsCredit(ref BasicClient client, ref Credit credit)
        {
            _allMoney -= credit.GetCredit;

            if (client.Trust >= 1.4)
            {
                if (credit.Percent > 11)
                {
                    credit.Percent -= 5;
                }
                else if (credit.Percent >= 5)
                {
                    credit.Percent -= 2;
                }
                else
                {
                    credit.Percent -= 0.5;
                }
            }

            client.Credits.Add(credit);
            client.Balance += credit.GetCredit;

            HistoryEvent = BasicClient.HistoryMessage;
            HistoryEvent?.Invoke($"Взятие кредита {credit.Name} на сумму {credit.GetCredit:#.##} руб.", 1, client);

            return true;
        }


        /// <summary>
        ///     Возвращает JSON массив с данными о базовых кредитах или пустой массив, если кредитов нет
        /// </summary>
        private JArray SaveCredits(ObservableCollection<Credit> list)
        {
            var creditsArr = new JArray();

            if (list.Count == 0)
            {
                return creditsArr;
            }

            foreach (var e in list)
            {
                var credit = new JObject
                {
                    ["Название"] = e.Name,
                    ["Сумма кредита"] = e.GetCredit,
                    ["Срок"] = e.Days,
                    ["Процент"] = e.Percent,
                    ["Штраф"] = e.Fine
                };

                creditsArr.Add(credit);
            }

            return creditsArr;
        }

        /// <summary>
        ///     Возвращает JSON массив с данными о базовых депозитах или пустой массив, если кредитов нет
        /// </summary>
        private JArray SaveDeposits(ObservableCollection<Deposit> list)
        {
            var depositsArr = new JArray();

            if (list.Count == 0)
            {
                return depositsArr;
            }

            foreach (var e in list)
            {
                var deposit = new JObject
                {
                    ["Название"] = e.Name,
                    ["Минимальная сумма депозита"] = e.GetMinSumOfDeposit,
                    ["Срок"] = e.Days,
                    ["Процент"] = e.Percent,
                    ["Возможность снятия"] = e.WithdrawingMoney,
                    ["Возможность пополнения"] = e.ReplenishmentDeposit
                };

                depositsArr.Add(deposit);
            }

            return depositsArr;
        }

        /// <summary>
        ///     Возвращает JSON массив с данными о клиентах, если кредитов нет
        /// </summary>
        private JArray SaveClients()
        {
            _clientsArr = new JArray();

            if (AllBankClient.Count == 0)
            {
                return _clientsArr;
            }

            var threads = new Thread[500];

            for (var i = 0; i < 500; i++)
            {
                threads[i] = new Thread(ParallelSaveClient);
                threads[i].Start(new[] { i * _allBankClient.Count / 500, (i + 1) * _allBankClient.Count / 500 });
                threads[i].Join();
            }

            return _clientsArr;
        }

        /// <summary>
        ///     Для параллельного сохранения клиентов
        /// </summary>
        private void ParallelSaveClient(object o)
        {
            var segment = o as int[];

            for (var i = segment[0]; i < segment[1]; i++)
            {
                var client = new JObject
                {
                    ["Имя"] = _allBankClient[i].Name,
                    ["ID"] = _allBankClient[i].Id,
                    ["Пароль"] = _allBankClient[i].Password,
                    ["Баланс"] = _allBankClient[i].Balance,
                    ["Уровень доверия"] = _allBankClient[i].Trust,
                    ["Зарплата клиента"] = _allBankClient[i].Salary,
                    ["Выплаченные кредиты"] = _allBankClient[i].CompletedCredit,
                    ["Кредиты клиента"] = SaveCredits(_allBankClient[i].Credits),
                    ["Депозиты клиента"] = SaveDeposits(_allBankClient[i].GetDeposits)
                };

                _clientsArr.Add(client);
            }
        }

        /// <summary>
        ///     Возвращает лист кредитов из json-файла
        /// </summary>
        private ObservableCollection<Credit> SetCreditFromJson(string json, string jsonBranch)
        {
            var list = new ObservableCollection<Credit>();
            var arrOfCredits = JObject.Parse(json)[jsonBranch].ToArray();

            foreach (var credit in arrOfCredits)
            {
                var name = credit["Название"].ToString();
                var sum = credit["Сумма кредита"].ToString().ToDouble();
                var days = credit["Срок"].ToString().ToInt();
                var percent = credit["Процент"].ToString().ToDouble();
                var fine = credit["Штраф"].ToString().ToDouble();

                list.Add(new Credit(sum, percent, days, name, fine));
            }

            return list;
        }

        /// <summary>
        ///     Возвращает лист депозитов из json-файла
        /// </summary>
        private ObservableCollection<Deposit> SetDepositFromJson(string json, string jsonBranch)
        {
            var list = new ObservableCollection<Deposit>();
            var arrOfDeposits = JObject.Parse(json)[jsonBranch].ToArray();

            foreach (var deposit in arrOfDeposits)
            {
                var dep = new Deposit
                {
                    Name = deposit["Название"].ToString(),
                    GetMinSumOfDeposit = deposit["Минимальная сумма депозита"].ToString().ToDouble(),
                    Days = deposit["Срок"].ToString().ToInt(),
                    Percent = deposit["Процент"].ToString().ToDouble(),
                    ReplenishmentDeposit = deposit["Возможность пополнения"].ToString(),
                    WithdrawingMoney = deposit["Возможность снятия"].ToString()
                };

                list.Add(dep);
            }

            return list;
        }

        /// <summary>
        ///     Заполняет данные о ВСЕХ клиентах банка из json-файла
        /// </summary>
        private void SetClientsFromJson(string json)
        {
            _arrOfClients = JObject.Parse(json)["Клиенты банка"].ToArray();

            foreach (var clientInfo in _arrOfClients)
            {
                if (clientInfo["Зарплата клиента"].ToString() == BasicClient.salaryArray[0].ToString())
                {
                    var client = new Client();
                    SetPropertiesOfClient(client, clientInfo);

                    _allBankClient.Add(client);
                    _listPasswordsHash.Add(client.GetHash);
                }

                else if (clientInfo["Зарплата клиента"].ToString() == BasicClient.salaryArray[1].ToString())
                {
                    var vipClient = new VipClient();
                    SetPropertiesOfClient(vipClient, clientInfo);

                    _allBankClient.Add(vipClient);
                    _listPasswordsHash.Add(vipClient.GetHash);
                }
                else
                {
                    var entityClient = new EntityClient();
                    SetPropertiesOfClient(entityClient, clientInfo);

                    _allBankClient.Add(entityClient);
                    _listPasswordsHash.Add(entityClient.GetHash);
                }

                BasicClient.NextId();
            }
        }

        /// <summary>
        ///     Заполняет все данные об ОДНОМ клиенте из json-файла
        /// </summary>
        private void SetPropertiesOfClient(BasicClient client, JToken clientInfoJson)
        {
            client.Name = clientInfoJson["Имя"].ToString();
            client.Id = clientInfoJson["ID"].ToString().ToUInt();
            client.Password = clientInfoJson["Пароль"].ToString();
            client.Balance = clientInfoJson["Баланс"].ToString().ToDouble();
            client.Trust = clientInfoJson["Уровень доверия"].ToString().ToDouble();
            client.Salary = clientInfoJson["Зарплата клиента"].ToString().ToDouble();
            client.CompletedCredit = clientInfoJson["Выплаченные кредиты"].ToString().ToUInt();

            client.GetDeposits = SetDepositFromJson(clientInfoJson.ToString(), "Депозиты клиента");
            client.Credits = SetCreditFromJson(clientInfoJson.ToString(), "Кредиты клиента");
        }


        #endregion
    }
}