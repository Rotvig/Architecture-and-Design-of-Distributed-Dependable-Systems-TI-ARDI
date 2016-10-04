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
        private List<Card> currentDealerCards;

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
            var player = players.Single(x => x.SubscriptionId == message.SubscriptionId.Value);
            player.Status = Status.Stands;
            player.Cards = message.EventData.Cards;
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
            currentDealerCards = new List<Card>();
            currentDeck = DeckFactory.CreateDeck().Shuffle();
            button.IsEnabled = false;

            publisher.Publish(Utils.TablePublishTopic, Event.GameStart);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(Utils.Timeout+1000);
                Dispatcher.Invoke(HandoutCards);
            });
        }

        private void HandoutCards()
        {
            //Dealer picks two cards for himself
            currentDealerCards.Add(currentDeck.Dequeue());
            var card = currentDeck.Dequeue();
            card.Facedown = true;
            currentDealerCards.Add(card);
            listBox.Items.Add(currentDealerCards.First().CardName);
            listBox.Items.Add("Facedown");
            totalVal.Text = currentDealerCards.First().Value.ToString();

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
            if (players.Any(player => player.Status == Status.Playing)) return;

            var dealerValue = PlayDealerCards();
            if (dealerValue.HasValue)
            {
                totalVal.Text = dealerValue.Value.ToString();
            }

            foreach (var player in players)
            {
                if (player.Status == Status.Stands && 
                    !dealerValue.HasValue ||
                    (dealerValue.HasValue && player.Cards.Where(x => x.Facedown == false).Sum(x => x.Value) >= dealerValue))
                {
                    var prizeMoney = player.Bet;

                    //Standoff
                    if (player.Cards.Where(x => x.Facedown == false).Sum(x => x.Value) != dealerValue)
                    {
                        prizeMoney = prizeMoney * 1.5;
                    }

                    publisher.Publish(Utils.TablePublishTopic,
                        Event.GamerOver,
                        new EventData
                        {
                            Win = true,
                            Bet = prizeMoney
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

        /// <summary>
        /// Dealer algorithm returns null when dealer is bust
        /// </summary>
        /// <returns></returns>
        private int? PlayDealerCards()
        {
            var totalCardVal = currentDealerCards.Where(x => x.Facedown == false).Sum(x => x.Value);
            var possibleValues = new List<int> {totalCardVal};

            if (totalCardVal >= 17)
            {
                //Check if there is any ace's
                if (currentDealerCards.Any(x => x.SecondaryValue > 0))
                {
                    //Calculate new possibleValues with the change of the ace's value
                    possibleValues.AddRange(
                        currentDealerCards
                            .Where(x => x.SecondaryValue > 0)
                            .Select(
                                currentDealerCard =>
                                    totalCardVal - currentDealerCard.Value + currentDealerCard.SecondaryValue));
                }
            }
            else
            {
                Card card;
                if (currentDealerCards.Any(x => x.Facedown))
                {
                    card = currentDealerCards.Single(x => x.Facedown);
                    card.Facedown = false;
                    listBox.Items.Remove("Facedown");
                }
                else
                {
                    card = currentDeck.Dequeue();
                    currentDealerCards.Add(card);
                }

                listBox.Items.Add(card.CardName);

                PlayDealerCards();
            }

            try
            {
                return possibleValues.Where(x => x <= 21).Max();

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
