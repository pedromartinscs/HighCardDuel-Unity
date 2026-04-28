using System;
using System.Collections.Generic;

namespace HighCardDuel
{
    public sealed class Deck
    {
        private readonly List<Card> cards;

        public Deck(IEnumerable<Card> cards)
        {
            this.cards = new List<Card>(cards);
        }

        public int Count => cards.Count;

        public static Deck CreateStandard52()
        {
            var cards = new List<Card>(52);

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card(rank, suit));
                }
            }

            return new Deck(cards);
        }

        public void Shuffle(Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            for (var i = cards.Count - 1; i > 0; i--)
            {
                var swapIndex = random.Next(i + 1);
                var current = cards[i];
                cards[i] = cards[swapIndex];
                cards[swapIndex] = current;
            }
        }

        public Tuple<Deck, Deck> SplitEvenly()
        {
            if (cards.Count % 2 != 0)
            {
                throw new InvalidOperationException("Cannot split an odd number of cards evenly.");
            }

            var half = cards.Count / 2;
            var first = cards.GetRange(0, half);
            var second = cards.GetRange(half, half);

            return Tuple.Create(new Deck(first), new Deck(second));
        }

        public Card Draw()
        {
            if (cards.Count == 0)
            {
                throw new InvalidOperationException("Cannot draw from an empty deck.");
            }

            var index = cards.Count - 1;
            var card = cards[index];
            cards.RemoveAt(index);
            return card;
        }
    }
}
