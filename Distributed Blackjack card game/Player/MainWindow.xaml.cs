using System;
using System.Windows;
using System.Windows.Controls;
using Shared;

namespace Player
{
    public partial class MainWindow : Window
    {
        private readonly Subscriber subscriber;

        public MainWindow()
        {
            InitializeComponent();

            subscriber = new Subscriber();
            subscriber.NewMessage += (sender, @event) => Dispatcher.Invoke(() => NewMessage(@event.Message));
        }

        private void NewMessage(Message message)
        {
            switch (message.Event)
            {
                case Event.GameStart:
                    break;
                case Event.Bet:
                    break;
                case Event.Hit:
                    break;
                case Event.Stand:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void btn_sub_Click(object sender, RoutedEventArgs e)
        {
            subscriber.Subscribe(topic.Text.Trim());

            ((Button) sender).Visibility = Visibility.Collapsed;
            btn_unsub.Visibility = Visibility.Visible;
        }

        private void btn_unsub_Click(object sender, RoutedEventArgs e)
        {
            subscriber.Unsubscribe();
            ((Button) sender).Visibility = Visibility.Collapsed;
            btn_sub.Visibility = Visibility.Visible;
        }
    }
}
