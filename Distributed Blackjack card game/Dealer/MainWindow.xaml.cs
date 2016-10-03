using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Shared;

namespace Dealer
{
    public partial class MainWindow : Window
    {
        private readonly Publisher publisher;
        private readonly Subscriber subscriber;
        private Queue<Card> currentDeck;
        private readonly List<Player> players = new List<Player>();

        public MainWindow()
        {
            InitializeComponent();

            Block1.Text = "Topic: " + Utils.TablePublishTopic;
            Block2.Text = "Sub Topic: " + Utils.TableSubscribeTopic;

            publisher = new Publisher();
            subscriber = new Subscriber();
            subscriber.Subscribe(Utils.TableSubscribeTopic);
            subscriber.NewMessage += (sender, @event) => Dispatcher.Invoke(() => NewMessage(@event.Message));
        }

        private void NewMessage(Message message)
        {
            switch (message.Event)
            {
                case Event.GameStart:
                    break;
                case Event.Bet:
                    AddPlayer(message);
                    break;
                case Event.Hit:
                    break;
                case Event.Stand:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddPlayer(Message message)
        {
            players.Add(new Player
            {
                SubscriptionId = message.SubscriptionId.Value,
                Bet = message.EventData.Bet
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            currentDeck = DeckFactory.CreateDeck().Shuffle();
            button.IsEnabled = false;
            publisher.Publish(Utils.TablePublishTopic, Event.GameStart);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                HandoutCards();
            });
        }

        private void HandoutCards()
        {
            foreach (var player in players)
            {
                publisher.Publish(
                    Utils.TablePublishTopic,
                    Event.HandoutCards,
                    new EventData()
                    {
                        Cards = new List<Card> { currentDeck.Dequeue(), currentDeck.Dequeue() }
                    },
                    player.SubscriptionId,
                    true);
            }
        }
    }
}
