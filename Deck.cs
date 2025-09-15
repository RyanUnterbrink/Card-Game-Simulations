using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Card_Game_Simulations
{
    public class Deck
    {
        List<Card> cards = new List<Card>();
        List<Card> cardsDealt = new List<Card>();

        public List<Card> Cards
        {
            get
            {
                return cards;
            }
        }

        public Deck()
        {
            int cardsPerSuit = 13;
            int minCardValue = 2;

            foreach (Card.CardSuit s in Enum.GetValues(typeof(Card.CardSuit)))
            {
                if (s == Card.CardSuit.None || s == Card.CardSuit.Joker)
                    continue;

                for (int j = minCardValue; j < minCardValue + cardsPerSuit; j++)
                    cards.Add(new Card(s, j));
            }

            cards.Add(new Card(Card.CardSuit.Joker, 1, Card.CardColor.Red));
            cards.Add(new Card(Card.CardSuit.Joker, 1, Card.CardColor.Black));
        }

        public void Shuffle()
        {
            Random random = new Random();

            List<Card> newCards = new List<Card>();

            while (cards.Count > 0)
            {
                int c = random.Next(0, cards.Count);
                Card cardToShuffle = cards[c];
                cards.RemoveAt(c);

                int nc = random.Next(0, newCards.Count + 1);
                newCards.Insert(nc, cardToShuffle);
            }

            cards = newCards;
        }
        public void RemoveJokers(int num = 1)
        {
            if (num > 2)
                num = 2;

            for (int i = cards.Count - 1; i >= 0 && num > 0; i--)
            {
                Card c = cards[i];

                if (c.Suit == Card.CardSuit.Joker)
                {
                    num--;
                    cards.RemoveAt(i);
                }
            }
        }
        public Card DealCard()
        {
            if (cards.Count > 0)
            {
                Card card = cards[cards.Count - 1];
                card.Deal();

                cards.RemoveAt(cards.Count - 1);

                cardsDealt.Add(card);

                return card;
            }

            return null;
        }
        public void CollectCards()
        {
            foreach (var c in cardsDealt)
            {
                c.Collect();
                cards.Add(c);
            }

            cardsDealt.Clear();
        }
    }
}
