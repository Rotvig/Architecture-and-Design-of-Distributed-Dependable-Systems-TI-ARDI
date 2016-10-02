using System;
using System.Collections.Generic;
using System.Windows;
using Shared;

namespace Dealer
{
    public partial class MainWindow : Window
    {
        private readonly Publisher publisher;
        private readonly Subscriber subscriber;
        private List<Card> currentDeck;
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
                    break;
                case Event.Hit:
                    break;
                case Event.Stand:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            currentDeck = DeckFactory.CreateDeck();
            currentDeck.Shuffle();
            publisher.Publish(Utils.TablePublishTopic, Event.GameStart);
        }
    }
}
