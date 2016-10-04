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
        private List<Player> players;
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
                case Event.Bust:
                    PlayerBust(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayerBust(Message message)
        {
            players.Single(x => x.SubscriptionId == message.SubscriptionId.Value).Status = Status.Bust;
            TryFinishGame();
        }

        private void PlayerStands(Message message)
        {
            players.Single(x => x.SubscriptionId == message.SubscriptionId.Value).Status = Status.Stands;
            //Player should sent info about what cards are facedown here
            TryFinishGame();
        }

        private void PlayerHits(Message message)
        {
            publisher.Publish(
                Utils.TablePublishTopic,
                Event.Hit,
                new EventData
                {
                    Cards = new List<Card> {currentDeck.Dequeue()}
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
            players = new List<Player>();
            currentDealerCards = new Queue<Card>();
            currentDeck = DeckFactory.CreateDeck().Shuffle();
            button.IsEnabled = false;

            publisher.Publish(Utils.TablePublishTopic, Event.GameStart);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(20000);
                HandoutCards();
            });
        }

        private void HandoutCards()
        {
            //Dealer picks two cards for himself
            currentDealerCards.Enqueue(currentDeck.Dequeue());
            var card = currentDeck.Dequeue();
            card.Facedown = true;
            currentDealerCards.Enqueue(card);

            foreach (var player in players)
            {
                //Pick cards for the player and give one facedown
                var cards = new List<Card> {currentDeck.Dequeue(), currentDeck.Dequeue()};
                cards.Last().Facedown = true;
                player.Cards = cards;
                publisher.Publish(
                    Utils.TablePublishTopic,
                    Event.HandoutCards,
                    new EventData
                    {
                        Cards = cards
                    },
                    player.SubscriptionId,
                    true);
            }
        }

        private void TryFinishGame()
        {
            if (players.All(player => player.Status == Status.Playing)) return;
            //Dealer should play his hand now
            //LET ALL PLAYERS KNOW IF THEY WIN OR NOT
            foreach (var player in players)
            {
                if (player.Status == Status.Stands &&
                    (player.Cards.Sum(x => x.Value) >
                     currentDealerCards.Where(x => x.Facedown == false).Sum(x => x.Value)))
                {
                    publisher.Publish(Utils.TablePublishTopic,
                        Event.GamerOver,
                        new EventData
                        {
                            Win = true,
                            Bet = player.Bet*2
                        },
                        player.SubscriptionId,
                        true);
                }
                else
                {
                    publisher.Publish(Utils.TablePublishTopic,
                        Event.GamerOver,
                        new EventData
                        {
                            Win = false
                        },
                        player.SubscriptionId,
                        true);
                }
            }
            //Game Over
            button.IsEnabled = true;
        }
    }
}
