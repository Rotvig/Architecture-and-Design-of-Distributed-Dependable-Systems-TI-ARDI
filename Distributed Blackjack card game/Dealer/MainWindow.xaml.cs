using System;
using System.Collections.Generic;
using System.Linq;
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
        private Queue<Card> currentDealerCards;

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
                case Event.Bet:
                    AddPlayer(message);
                    break;
                case Event.Hit:
                    PlayerHits(message);
                    break;
                case Event.Stand:
                    PlayerStands(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayerStands(Message message)
        {
            players.Single(x => x.SubscriptionId == message.SubscriptionId.Value).Status = Status.Stands;
            if (players.Any(player => player.Status != Status.Playing))
            {
                //LET ALL PLAYERS KNOW IF THEY WIN OR NOT
            }
        }

        private void PlayerHits(Message message)
        {
            publisher.Publish(
                Utils.TablePublishTopic,
                Event.Hit,
                new EventData
                    {
                        Cards = new List<Card> { currentDeck.Dequeue() }
                    },
                message.SubscriptionId,
                true
             );
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

            //Dealer picks two cards for himself
            currentDealerCards = new Queue<Card>();
            currentDealerCards.Enqueue(currentDeck.Dequeue());
            currentDealerCards.Enqueue(currentDeck.Dequeue());
        }
    }
}
