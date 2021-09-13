using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HomeWork_13_Bank_WPF.Views
{
    /// <summary>
    /// Логика взаимодействия для HistoryOperation.xaml
    /// </summary>
    public partial class HistoryOperation : Window
    {
        #region Fields

        private readonly ObservableCollection<TextBlock> _textBlocks;

        #endregion

        #region .ctor

        public HistoryOperation(BasicClient client1)
        {
            var client = client1;

            InitializeComponent();
            _textBlocks = new ObservableCollection<TextBlock>();

            for (var i = 0; i < client.listOfHistoryMessages.Count; i++)
            {
                HistoryMessage(client.listOfHistoryMessages[i], client.listTypesOfHistoryMess[i]);
            }

            ListOfHistory.ItemsSource = _textBlocks;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Выводит сообщение о действиях с аккаунтом клиента в банке
        /// </summary>
        public void HistoryMessage(string str, int i)
        {
            var textBlock = new TextBlock
            {
                Text = str,
                FontSize = 18,
                Margin = new Thickness(1)
            }; //!!!---- Используйте диспетчер, чтбы прокинуть вызов в основной поток ----!!!// - не помогает

            var brushConvert = new BrushConverter();
            switch (i)
            {
                case 0:
                    textBlock.Background = (Brush)brushConvert.ConvertFrom("#2EFE9A");  // зеленый цвет для "положительных" событий со счетом клиента
                    break;
                case 2:
                    textBlock.Background = (Brush)brushConvert.ConvertFrom("#FE642E");  // красный цвет для "отрицательных" событий с аккаунтом клиента
                    break;
                default:
                    textBlock.Background = (Brush)brushConvert.ConvertFrom("#E0F2F7");  // серый цвет для нейтральных событий с аккаунтом клиента
                    break;
            }

            _textBlocks.Add(textBlock);
            ListOfHistory.Items.Refresh();
        }

        #endregion
    }
}
