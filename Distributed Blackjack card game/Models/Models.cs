using System;
using System.Collections.Generic;
using System.Net;

namespace Shared
{
    public class SubscriberTuple : Tuple<EndPoint, Guid>
    {
        public SubscriberTuple(EndPoint endpoint, Guid subscriptionId): base(endpoint, subscriptionId)
        {}

        public EndPoint Endpoint => Item1;
        public Guid SubscriptionId => Item2;
    }

    public class Message
    {
        public Command Command { get; set; }
        public Guid? SubscriptionId { get; set; }
        public string Topic { get; set; }
        public Event Event { get; set; }
        public EventData EventData { get; set; }
        public bool PublishToSubscriptionId { get; set; }
    }

    public class EventData
    {
        public List<Card> Cards { get; set; }
        public List<Card> DealerCards { get; set; }
        public double Bet { get; set; }
        public bool Win { get; set; }
    }

    public enum Command
    {
        Publish,
        Subscribe,
        Unsubscribe
    }

    public enum Event
    {
        GameStart,
        GamerOver,
        HandoutCards,
        Bet,
        Hit,
        Stand,
        Bust
    }

    public class Player
    {
        public Guid SubscriptionId { get; set; }
        public double Bet { get; set; }
        public Status Status { get; set; }
        public List<Card> Cards { get; set; } 
    }

    public enum Status
    {
        Playing,
        Stands,
        Bust
    }

    public class Card
    {
        public Card(string cardName, int value, int secondaryValue = 0)
        {
            CardName = cardName;
            Value = value;
            SecondaryValue = secondaryValue;
        }

        public string CardName { get; }
        public int Value { get; }
        public int SecondaryValue { get; }
        public bool Facedown { get; set; }
    }

    public static class DeckFactory
    {
        public static List<Card> CreateDeck()
        {
            return new List<Card>()
            {
                //Heart
                new Card("Ace-Heart", 1, 11),
                new Card("2-Heart", 2),
                new Card("3-Heart", 3),
                new Card("4-Heart", 4),
                new Card("5-Heart", 5),
                new Card("6-Heart", 6),
                new Card("7-Heart", 7),
                new Card("8-Heart", 8),
                new Card("9-Heart", 9),
                new Card("10-Heart", 10),
                new Card("Jack-Heart", 10),
                new Card("Queen-Heart", 10),
                new Card("King-Heart", 10),
                //Clubs
                new Card("Ace-Clubs", 1, 11),
                new Card("2-Clubs", 2),
                new Card("3-Clubs", 3),
                new Card("4-Clubs", 4),
                new Card("5-Clubs", 5),
                new Card("6-Clubs", 6),
                new Card("7-Clubs", 7),
                new Card("8-Clubs", 8),
                new Card("9-Clubs", 9),
                new Card("10-Clubs", 10),
                new Card("Jack-Clubs", 10),
                new Card("Queen-Clubs", 10),
                new Card("King-Clubs", 10),
                //Diamonds
                new Card("Ace-Diamonds", 1, 11),
                new Card("2-Diamonds", 2),
                new Card("3-Diamonds", 3),
                new Card("4-Diamonds", 4),
                new Card("5-Diamonds", 5),
                new Card("6-Diamonds", 6),
                new Card("7-Diamonds", 7),
                new Card("8-Diamonds", 8),
                new Card("9-Diamonds", 9),
                new Card("10-Diamonds", 10),
                new Card("Jack-Diamonds", 10),
                new Card("Queen-Diamonds", 10),
                new Card("King-Diamonds", 10),
                //Spades
                new Card("Ace-Spades", 1, 11),
                new Card("2-Spades", 2),
                new Card("3-Spades", 3),
                new Card("4-Spades", 4),
                new Card("5-Spades", 5),
                new Card("6-Spades", 6),
                new Card("7-Spades", 7),
                new Card("8-Spades", 8),
                new Card("9-Spades", 9),
                new Card("10-Spades", 10),
                new Card("Jack-Spades", 10),
                new Card("Queen-Spades", 10),
                new Card("King-Spades", 10),
            };
        }
    }
}
