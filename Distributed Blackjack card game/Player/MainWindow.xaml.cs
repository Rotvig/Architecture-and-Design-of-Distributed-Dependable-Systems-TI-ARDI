using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
        private int _value;

        public MainWindow()
        {
            InitializeComponent();

            subscriber = new Subscriber();
            subscriber.NewMessage += (sender, @event) => Dispatcher.Invoke(() => NewMessage(@event.Message));
            publisher = new Publisher();
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += timer_Tick;
        }

        private void NewMessage(Message message)
        {
            switch (message.Content.Event)
            {
                case Event.GameStart:
                    time =  TimeSpan.FromSeconds((message.Header.Timeout.Value - DateTime.Now).Seconds);
                    timer.Start();
                    btn_bet.IsEnabled = true;
                    lblTime.Text = time.ToString();
                    break;
                case Event.Hit:
                    RecieveCard(message.Content);
                    break;
                case Event.HandoutCards:
                    CardsHandout(message.Content);
                    break;
                case Event.GamerOver:
                    GameOver(message.Content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RecieveCard(MessageContent message)
        {
            var card = message.EventData.Cards.Single();
            cards.Add(card);
            listBox.Items.Add(card.CardName);

            CheckIfBust();
        }

        private void CheckIfBust()
        {
            _value = cards.Sum(x =>x.Facedown ? 0 : x.Flipped? x.SecondaryValue : x.Value);

            if (_value > 21)
            {
                foreach (var card1 in cards)
                {
                    if (card1.SecondaryValue > 0 && !card1.Flipped)
                    {
                        card1.Flipped = true;
                        _value = cards.Sum(x => x.Facedown ? 0 : x.Flipped ? x.SecondaryValue : x.Value);
                        if (_value <= 21)
                        break;
                    }
                }
                totalVal.Text = _value.ToString();
                if (_value > 21)
                {
                    publisher.Publish("Sub " + topic.Text.Trim(), Event.Bust, null, subscriber.SubscriptionId);
                    totalVal.Text = "Bust";
                    btn_stand.IsEnabled = false;
                    btn_hit.IsEnabled = false;
                }
                
            }
            else
            {
                totalVal.Text = _value.ToString();
            }
        }

        private void GameOver(MessageContent message)
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

        private void CardsHandout(MessageContent message)
        {
            lblTime.Text = "";
            cards = message.EventData.Cards;
            listBox.Items.Add(cards.Single(x => x.Facedown == false).CardName);
            listBox.Items.Add("Facedown");
            totalVal.Text = cards.First().Value.ToString();
            btn_facedown.IsEnabled = true;
            btn_stand.IsEnabled = true;

        }

        private void btn_sub_Click(object sender, RoutedEventArgs e)
        {
            subscriber.Subscribe(topic.Text.Trim());

            ((Button)sender).Visibility = Visibility.Collapsed;
            btn_unsub.Visibility = Visibility.Visible;
        }

        private void btn_unsub_Click(object sender, RoutedEventArgs e)
        {
            subscriber.Unsubscribe();
            ((Button)sender).Visibility = Visibility.Collapsed;
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
                    value = _value
                },
                subscriber.SubscriptionId);
            StandText.Text = "Good luck!";
            btn_stand.IsEnabled = false;
            btn_hit.IsEnabled = false;
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

            CheckIfBust();

            btn_facedown.IsEnabled = false;
            btn_hit.IsEnabled = true;
        }
    }
}
