using System;

namespace HighCardDuel
{
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    public enum Rank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public readonly struct Card : IEquatable<Card>
    {
        public Card(Rank rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public Rank Rank { get; }
        public Suit Suit { get; }
        public int Value => (int)Rank;
        public bool IsRed => Suit == Suit.Diamonds || Suit == Suit.Hearts;
        public string Label => RankLabel + SuitLabel;

        public bool Equals(Card other)
        {
            return Rank == other.Rank && Suit == other.Suit;
        }

        public override bool Equals(object obj)
        {
            return obj is Card other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Rank * 397) ^ (int)Suit;
            }
        }

        public override string ToString()
        {
            return Label;
        }

        private string RankLabel
        {
            get
            {
                switch (Rank)
                {
                    case Rank.Ace:
                        return "A";
                    case Rank.King:
                        return "K";
                    case Rank.Queen:
                        return "Q";
                    case Rank.Jack:
                        return "J";
                    case Rank.Ten:
                        return "10";
                    default:
                        return ((int)Rank).ToString();
                }
            }
        }

        private string SuitLabel
        {
            get
            {
                switch (Suit)
                {
                    case Suit.Clubs:
                        return "♣";
                    case Suit.Diamonds:
                        return "♦";
                    case Suit.Hearts:
                        return "♥";
                    case Suit.Spades:
                        return "♠";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
