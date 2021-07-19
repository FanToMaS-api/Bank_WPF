using System;
using ExtensionLibrary;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows;
using System.Text;

namespace HomeWork_13_Bank_WPF
{
    class DataBase
    {
        #region Fileds

        private SqlCommand _command;
        private SqlDataReader _dataReader;
        private readonly SqlConnection _connection;

        #endregion

        public DataBase()
        {
            var stringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = "Bank",
                IntegratedSecurity = true,
                Pooling = true
            };

            _connection = new SqlConnection { ConnectionString = stringBuilder.ConnectionString };
        }

        #region Public Methods

        /// <summary>
        ///     Выгружает данные из базы
        /// </summary>
        public void DataUpload(Bank bank)
        {
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);

                MessageBox.Show("Произошла ошибак сохранения данных.\n" +
                   "Проверьте Ваше подключение к интернету.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                _connection.Close();

                return;
            }

            try
            {
                BasicCreditsAndDepositsUpload(bank);
                ClientsUpload(bank);
                CreditsUpload(bank);
                DepositsUpload(bank);
            }
            catch (Exception ex)
            {
                // TODO: Serilog
                Debug.Print(ex.Message);

                MessageBox.Show("Произошла ошибка сохранения данных.\n" +
                   "Проверьте Ваше подключение к интернету.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <summary>
        ///     Сохраняет данные в базу
        /// </summary>
        public void DataSave(Bank bank)
        {
            try
            {
                _connection.Open();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);

                MessageBox.Show("Произошла ошибак сохранения данных.\n" +
                                "Проверьте Ваше подключение к интернету.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                _connection.Close();
                return;
            }
            try
            {
                #region Очистка базы данных для перезаписи

                var sql = @"truncate table Credits
                        truncate table Deposits
                        truncate table Clients";
                _command = new SqlCommand(sql, _connection);
                _command.ExecuteNonQuery();

                #endregion

                SaveBasicCreditsAndDeposits();

                var id = 1;

                foreach (var client in bank.AllBankClient)
                {
                    SaveClient(client, id);
                    id++;
                }

            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);

                MessageBox.Show("Произошла ошибка сохранения данных.\n" +
                                "Проверьте Ваше подключение к интернету.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _connection.Close();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Сохраняет базовые кредиты и депозиты
        /// </summary>
        private void SaveBasicCreditsAndDeposits()
        {
            var sql = new StringBuilder();

            foreach (var e in Deposit.GetBasicDeposits())
            {
                var opW = 0;
                var opR = 0;

                if (e.WithdrawingMoney == "да")
                {
                    opW = 1;
                }

                if (e.ReplenishmentDeposit == "да")
                {
                    opR = 1;
                }

                sql.Append("insert into Deposits ([name], [deposit], [percent], [days], [opportunityWithdrawing], [opportunityReplenishment], [idClient])" +
                       $" values (N'{e.Name}', {e.GetMinSumOfDeposit}, {e.Percent:#.##}, {e.Days}, {opW}, {opR}, {0})");
            }

            foreach (var credit in Credit.GetBasicCredits())
            {
                sql.Append($@"insert into Credits ([name], [credit], [percent], [days], [fine], [idClient]) 
                        values (N'{credit.Name}', {credit.GetCredit}, {credit.Percent}, {credit.Days}, {credit.Fine}, {0})");
            }

            _command = new SqlCommand(sql.ToString(), _connection);
            _command.ExecuteNonQuery();
        }

        /// <summary>
        ///     Сохраняет данные в базу об одном клиенте
        /// </summary>
        private void SaveClient(BasicClient client, int id)
        {
            var sql = new StringBuilder($@"insert into Clients ([password], [salary], [name], [balance], [trust], [completedCredits]) 
                        values (N'{client.Password}', {client.Salary}, N'{client.Name}', {client.Balance}, {client.Trust}, {client.CompletedCredit})");

            foreach (var k in client.Credits)
            {
                sql.Append($@"insert into Credits ([name], [credit], [percent], [days], [fine], [idClient]) 
                        values (N'{k.Name}', {k.GetCredit}, {k.Percent}, {k.Days}, {k.Fine}, {id})");
            }

            foreach (var l in client.GetDeposits)
            {
                var opW = 0;
                var opR = 0;

                if (l.WithdrawingMoney == "да")
                {
                    opW = 1;
                }

                if (l.ReplenishmentDeposit == "да")
                {
                    opR = 1;
                }

                sql.Append($@"insert into Deposits ([name], [deposit], [percent], [days], [opportunityWithdrawing], [opportunityReplenishment], [idClient]) 
                        values (N'{l.Name}', {l.GetMinSumOfDeposit}, {l.Percent}, {l.Days}, {opW}, {opR}, {id})");
            }

            _command = new SqlCommand(sql.ToString(), _connection);
            _command.ExecuteNonQuery();
        }

        /// <summary>
        ///     Заносит данные о депозитах каждому пользователю
        /// </summary>
        private void DepositsUpload(Bank bank)
        {
            var sql = @"Select * from Deposits where idClient > 0";

            _command = new SqlCommand(sql, _connection);
            _dataReader = _command.ExecuteReader();

            while (_dataReader.Read())
            {
                var name = _dataReader["name"].ToString();
                var sum = _dataReader["deposit"].ToString().ToDouble();
                var days = _dataReader["days"].ToString().ToInt();
                var percent = _dataReader["percent"].ToString().ToDouble();

                var d = new Deposit(name, sum, percent, days, OppotunityWithdrawingMoney.No,
                    OppotunityReplenishmentDeposit.No)
                {
                    WithdrawingMoney = (int)_dataReader["opportunityWithdrawing"] == 0 ? "нет" : "да",
                    ReplenishmentDeposit = (int)_dataReader["opportunityReplenishment"] == 0 ? "нет" : "да"
                };

                bank.AllBankClient[(int)_dataReader["idClient"] - 1].GetDeposits.Add(d);
            }

            _dataReader.Close();
        }

        /// <summary>
        ///     Заносит данные о кредитах каждому пользователю
        /// </summary>
        private void CreditsUpload(Bank bank)
        {
            var sql = @"Select * from Credits where idClient > 0";
            _command = new SqlCommand(sql, _connection);
            _dataReader = _command.ExecuteReader();

            while (_dataReader.Read())
            {
                var name = _dataReader["name"].ToString();
                var sum = _dataReader["credit"].ToString().ToDouble();
                var days = _dataReader["days"].ToString().ToInt();
                var percent = _dataReader["percent"].ToString().ToDouble();
                var fine = _dataReader["fine"].ToString().ToDouble();
                var credit = new Credit(sum, percent, days, name, fine);

                bank.AllBankClient[(int)_dataReader["idClient"] - 1].Credits.Add(credit);
            }
            _dataReader.Close();
        }

        /// <summary>
        ///     Загружает данные клиентов из баз данных
        /// </summary>
        private void ClientsUpload(Bank bank)
        {
            var sql = @"Select * from Clients";
            _command = new SqlCommand(sql, _connection);
            _dataReader = _command.ExecuteReader();
            while (_dataReader.Read())
            {
                var salary = (double)_dataReader["salary"];
                switch (salary)
                {
                    case 1000:
                        {
                            var client = new Client();
                            SetPropertiesOfClient(client, _dataReader);

                            bank.AddClient(client);
                            break;
                        }
                    case 2000:
                        {
                            var vipClient = new VipClient();
                            SetPropertiesOfClient(vipClient, _dataReader);

                            bank.AddClient(vipClient);
                            break;
                        }
                    default:
                        {
                            var entityClient = new EntityClient();
                            SetPropertiesOfClient(entityClient, _dataReader);

                            bank.AddClient(entityClient);
                            break;
                        }
                }
                BasicClient.NextId();
            }

            _dataReader.Close();
        }

        /// <summary>
        ///     Заполняет все данные об ОДНОМ клиенте
        /// </summary>
        private void SetPropertiesOfClient(BasicClient client, SqlDataReader reader)
        {
            client.Name = reader["name"].ToString();
            client.Id = reader["id"].ToString().ToUInt() - 1;
            client.Password = reader["password"].ToString();
            client.Balance = reader["balance"].ToString().ToDouble();
            client.Trust = reader["trust"].ToString().ToDouble();
            client.Salary = reader["salary"].ToString().ToDouble();
            client.CompletedCredit = reader["completedCredits"].ToString().ToUInt();
        }

        /// <summary>
        ///     Загрузка базовых кредитов и депозитов, предоставляемых банком
        /// </summary>
        private void BasicCreditsAndDepositsUpload(Bank bank)
        {
            var sql = @"Select * from Credits where idClient = 0";
            _command = new SqlCommand(sql, _connection);

            _dataReader = _command.ExecuteReader();

            while (_dataReader.Read())
            {
                bank.AddBasicCredit(new Credit((double)_dataReader["credit"], (double)_dataReader["percent"],
                    (int)_dataReader["days"],
                    _dataReader["name"].ToString(), (double)_dataReader["fine"]));
            }

            _dataReader.Close();

            sql = @"Select * from Deposits where idClient = 0";

            _command = new SqlCommand(sql, _connection);
            _dataReader = _command.ExecuteReader();

            while (_dataReader.Read())
            {
                var d = new Deposit(_dataReader["name"].ToString(), (double)_dataReader["deposit"],
                    (double)_dataReader["percent"], (int)_dataReader["days"],
                    OppotunityWithdrawingMoney.No, OppotunityReplenishmentDeposit.No)
                {
                    WithdrawingMoney = (int)_dataReader["opportunityWithdrawing"] == 0 ? "нет" : "да",
                    ReplenishmentDeposit = (int)_dataReader["opportunityReplenishment"] == 0 ? "нет" : "да"
                };

                bank.AddBasicDeposit(d);
            }

            _dataReader.Close();
        }

        #endregion
    }
}
