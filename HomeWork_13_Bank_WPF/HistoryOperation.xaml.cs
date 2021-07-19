using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HomeWork_13_Bank_WPF
{
    /// <summary>
    /// Логика взаимодействия для HistoryOperation.xaml
    /// </summary>
    public partial class HistoryOperation : Window
    {
        private readonly ObservableCollection<TextBlock> _textBlocks;
        private readonly BasicClient _client;
        public HistoryOperation(BasicClient client)
        {
            this._client = client;
            InitializeComponent();
            _textBlocks = new ObservableCollection<TextBlock>();
            for (int i = 0; i < this._client.listOfHistoryMessages.Count; i++)
            {
                HistoryMessage(this._client.listOfHistoryMessages[i], this._client.listTypesOfHistoryMess[i]);
            }
            ListOfHistory.ItemsSource = _textBlocks;
        }
        /// <summary>
        /// Выводит сообщение о действиях с аккаунтом клиента в банке
        /// </summary>
        /// <param name="str"></param>
        /// <param name="i"></param>
        public void HistoryMessage(string str, int i)
        {
            TextBlock textBlock = new TextBlock(); //!!!---- Используйте диспетчер, чтбы прокинуть вызов в основной поток ----!!!// - не помогает
            textBlock.Text = str;
            textBlock.FontSize = 18;
            textBlock.Margin = new Thickness(1);
            BrushConverter brushConvert = new BrushConverter();
            if (i == 0)
                textBlock.Background = (Brush)brushConvert.ConvertFrom("#2EFE9A");  // зеленый цвет для "положительных" событий со счетом клиента
            else if (i == 2)
                textBlock.Background = (Brush)brushConvert.ConvertFrom("#FE642E");  // красный цвет для "отрицательных" событий с аккаунтом клиента
            else
                textBlock.Background = (Brush)brushConvert.ConvertFrom("#E0F2F7");  // серый цвет для нейтральных событий с аккаунтом клиента
            _textBlocks.Add(textBlock);
            ListOfHistory.Items.Refresh();
        }
        public ObservableCollection<TextBlock> TextBlocks
        {
            get => _textBlocks;
        }
    }
}
