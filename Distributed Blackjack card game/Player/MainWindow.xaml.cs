using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Shared;

namespace Player
{
    public partial class MainWindow : Window
    {
        private readonly Subscriber subscriber;
        private readonly Publisher publisher;
        private readonly DispatcherTimer timer;
        private TimeSpan time;
        private List<Card> cards; 

        public MainWindow()
        {
            InitializeComponent();

            subscriber = new Subscriber();
            subscriber.NewMessage += (sender, @event) => Dispatcher.Invoke(() => NewMessage(@event.Message));
            publisher = new Publisher();
            timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            timer.Tick += timer_Tick;
        }

        private void NewMessage(Message message)
        {
            switch (message.Event)
            {
                case Event.GameStart:
                    time = TimeSpan.FromSeconds(20);
                    timer.Start();
                    btn_bet.IsEnabled = true;
                    break;
                case Event.Bet:
                    break;
                case Event.Hit:
                    break;
                case Event.Stand:
                    break;
                case Event.HandoutCards:
                    cards =  message.EventData.Cards;
                    card1.Text = cards.First().CardName;
                    card2.Text = cards.Last().CardName;
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

        private void btn_bet_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            lblTime.Text = "";
            publisher.Publish("Sub " + topic.Text.Trim(), Event.Bet, 
                new EventData
            {
                Bet = int.Parse(BetTextBox.Text)
            }, 
            subscriber.SubscriptionId
            );
        }

        private void btn_hit_Click(object sender, RoutedEventArgs e)
        {
            publisher.Publish("Sub " + topic.Text.Trim(), Event.Hit, null, subscriber.SubscriptionId);
        }

        private void btn_stand_Click(object sender, RoutedEventArgs e)
        {
            publisher.Publish("Sub " + topic.Text.Trim(), Event.Stand, null, subscriber.SubscriptionId);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            lblTime.Text = time.ToString();
            if (time == TimeSpan.Zero)
            {
                timer.Stop();
                btn_bet.IsEnabled = false;
            }
            time = time.Add(TimeSpan.FromSeconds(-1));
        }
    }
}
