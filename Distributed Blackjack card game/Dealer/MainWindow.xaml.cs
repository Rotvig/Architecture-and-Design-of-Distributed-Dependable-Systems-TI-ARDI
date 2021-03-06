﻿using System;
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
        private string publishTopic;

        public MainWindow()
        {
            InitializeComponent();

            publishTopic = topic.Text.Trim();
            publisher = new Publisher();
            subscriber = new Subscriber();
            subscriber.Subscribe("Sub " + publishTopic);
            subscriber.NewMessage += (sender, @event) => Dispatcher.Invoke(() => NewMessage(@event.Message));
        }

        private void NewMessage(Message message)
        {
            switch (message.Content.Event)
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
            players.Single(x => x.SubscriptionId == message.Content.SubscriptionId.Value).Status = Status.Bust;
            TryFinishGame();
        }

        private void PlayerStands(Message message)
        {
            var player = players.Single(x => x.SubscriptionId == message.Content.SubscriptionId.Value);
            player.Status = Status.Stands;
            player.value = message.Content.EventData.value;
            TryFinishGame();
        }

        private void PlayerHits(Message message)
        {
            publisher.Publish(
                publishTopic,
                Event.Hit,
                null,
                new EventData
                {
                    Cards = new List<Card> {currentDeck.Dequeue()}
                },
                message.Content.SubscriptionId,
                true
                );
        }

        private void AddPlayer(Message message)
        {
            players.Add(new Player
            {
                SubscriptionId = message.Content.SubscriptionId.Value,
                Bet = message.Content.EventData.Bet
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            players = new List<Player>();
            currentDealerCards = new List<Card>();
            currentDeck = DeckFactory.CreateDeck().Shuffle();
            button.IsEnabled = false;

            publisher.Publish(publishTopic, Event.GameStart, TimeSpan.FromSeconds(10));
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(10000);
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
            var timeSpan = TimeSpan.FromSeconds(30);

            foreach (var player in players)
            {
                //Pick cards for the player and give one facedown
                var cards = new List<Card> {currentDeck.Dequeue(), currentDeck.Dequeue()};
                cards.Last().Facedown = true;
                player.Cards = cards;
                publisher.Publish(
                    publishTopic,
                    Event.HandoutCards,
                    timeSpan,
                    new EventData
                    {
                        Cards = cards
                    },
                    player.SubscriptionId,
                    true);
            }

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(timeSpan);
                if (players.All(x => x.Status != Status.Playing)) return;
                //Remove all players who hasent answered back
                players.RemoveAll(x => x.Status == Status.Playing);
                Dispatcher.Invoke(TryFinishGame);
            });
        }

        private void TryFinishGame()
        {
            if (players.Any(player => player.Status == Status.Playing)) return;

            var dealerValue = PlayDealerCards();
            totalVal.Text = dealerValue?.ToString() ?? "Bust";

            foreach (var player in players)
            {
                if (player.Status == Status.Stands && 
                    !dealerValue.HasValue ||
                    (dealerValue.HasValue && player.value >= dealerValue))
                {
                    var prizeMoney = player.Bet;

                    //Standoff
                    if (player.Cards.Where(x => x.Facedown == false).Sum(x => x.Value) != dealerValue)
                    {
                        prizeMoney = prizeMoney * 1.5;
                    }

                    publisher.Publish(publishTopic,
                        Event.GameOver,
                        null,
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
                    publisher.Publish(publishTopic,
                        Event.GameOver,
                        null,
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
            var possibleValues = PlayCards();

            try
            {
                return possibleValues.Where(x => x <= 21).Max();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<int> PlayCards()
        {
            var totalCardVal = currentDealerCards.Where(x => x.Facedown == false).Sum(x => x.Value);
            var possibleValues = new List<int> {totalCardVal};

            //Check if there are any ace's
            if (currentDealerCards.Any(x => x.SecondaryValue > 0) && totalCardVal >= 6 && totalCardVal <= 10)
            {
                //Calculate new possibleValues with the change of the ace's value
                possibleValues.AddRange(
                    currentDealerCards
                        .Where(x => x.SecondaryValue > 0)
                        .Select(
                            currentDealerCard =>
                                totalCardVal - currentDealerCard.Value + currentDealerCard.SecondaryValue));
                return possibleValues;
            }
            if (totalCardVal >= 17)
            {
                return possibleValues;
            }

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

            return PlayCards();
        }

        private void btn_topic_Click(object sender, RoutedEventArgs e)
        {
            subscriber.Unsubscribe();
            publishTopic = topic.Text.Trim();
            subscriber.Subscribe("Sub " + publishTopic);
        }
    }
}
