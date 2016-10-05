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
                    time = TimeSpan.FromMilliseconds(Utils.Timeout);
                    timer.Start();
                    btn_bet.IsEnabled = true;
                    lblTime.Text = time.ToString();
                    break;
                case Event.Hit:
                    RecieveCard(message);
                    break;
                case Event.HandoutCards:
                    CardsHandout(message);
                    break;
                case Event.GamerOver:
                    GameOver(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RecieveCard(Message message)
        {
            var card = message.EventData.Cards.Single();
            cards.Add(card);
            listBox.Items.Add(card.CardName);
            totalVal.Text = cards.Sum(x => x.Value).ToString();
            var value = cards.Sum(x => x.Value);

            if (value > 21)
            {
                publisher.Publish("Sub " + topic.Text.Trim(), Event.Bust, null, subscriber.SubscriptionId);
                totalVal.Text = "Bust";
            }
            else
            {
                totalVal.Text = value.ToString();
            }
        }

        private void GameOver(Message message)
        {
            var msg = "Dealer wins !";
            //Inform player about the result
            if (message.EventData.Win)
            {
                msg = "YOU WIN !";
            }

            MessageBox.Show(msg);

            btn_facedown.IsEnabled = false;
            btn_hit.IsEnabled = false;
            btn_stand.IsEnabled = false;
            btn_bet.IsEnabled = false;
        }

        private void CardsHandout(Message message)
        {
            lblTime.Text = "";
            cards = message.EventData.Cards;
            listBox.Items.Add(cards.Single(x => x.Facedown == false).CardName);
            listBox.Items.Add("Facedown");
            totalVal.Text = cards.First().Value.ToString();
            btn_facedown.IsEnabled = true;
            btn_stand.IsEnabled = true;

            /*
            if (cards.All(x => x.SecondaryValue == 0)) return;

            // MessageBox.Show("You have picked an ace", "Should i count for 1 or 11?",)

            MessageBoxManager.Yes = "11";
            MessageBoxManager.No = "1";
            MessageBoxManager.Register();

            //DialogResult dialogResult = MessageBox.Show("An important decision!", "You have picked an ace. Which value should count as?", MessageBoxButton.YesNo);
            //if (dialogResult == DialogResult.Yes)
            //{
            //    //do something
            //}
            //else if (dialogResult == DialogResult.No)
            //{
            //    //do something else
            //}
            */
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
            btn_bet.IsEnabled = false;
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
            btn_bet.IsEnabled = false;
        }

        private void btn_stand_Click(object sender, RoutedEventArgs e)
        {
            publisher.Publish(
                "Sub " + topic.Text.Trim(),
                Event.Stand, new EventData
                {
                    Cards = cards
                },
                subscriber.SubscriptionId);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (time == TimeSpan.Zero)
            {
                timer.Stop();
                btn_bet.IsEnabled = false;
            }
            time = time.Add(TimeSpan.FromSeconds(-1));
            lblTime.Text = time.ToString();
        }

        private void btn_facedown_Click(object sender, RoutedEventArgs e)
        {
            var card = cards.Single(x => x.Facedown);
            card.Facedown = false;
            listBox.Items.Remove("Facedown");
            listBox.Items.Add(card.CardName);
            var value = cards.Sum(x => x.Value);

            if (value > 21)
            {
                publisher.Publish("Sub " + topic.Text.Trim(), Event.Bust, null, subscriber.SubscriptionId);
                return;
            }

            totalVal.Text = value.ToString();
            btn_facedown.IsEnabled = false;
            btn_hit.IsEnabled = true;
        }
    }
}
