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

            publisher = new Publisher();
            subscriber = new Subscriber();
            subscriber.Subscribe("Sub Table 1");
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
            publisher.Publish(topic.Text, Event.GameStart);

        }
    }
}
