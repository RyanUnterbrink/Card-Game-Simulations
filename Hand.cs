using System;
using System.Collections.Generic;

namespace Card_Game_Simulations
{
    public class Hand
    {
        public enum WinStatus
        {
            Win = 0,
            Lose = 1,
            Tie = 2
        }
        public enum PokerHandRank
        {
            High_Card = 0,
            Pair,
            Two_Pair,
            Trips,
            Straight,
            Flush,
            Full_House,
            Quads,
            Straight_Flush,
            Royal_Flush
        }
        public PokerHandRank handRank = PokerHandRank.High_Card;
        public Card[] cards = new Card[5];
        public List<List<Card>> combos = new List<List<Card>>();

        public Hand(Card[] board, Tuple<Card, Card> _cards = null)
        {
            // assign cards
            int numCards = board.Length;
            if (_cards != null)
                numCards += 2;

            Card[] allCards = new Card[numCards];
            bool[] analyzed = new bool[numCards];

            int allCardsNum;
            for (allCardsNum = 0; allCardsNum < board.Length; allCardsNum++)
                allCards[allCardsNum] = board[allCardsNum];

            if (_cards != null)
            {
                allCards[allCardsNum++] = _cards.Item1;
                allCards[allCardsNum] = _cards.Item2;
            }
            
            // gather cards into int bits
            int[] cardData = new int[4];
            int[] suits = new int[4];
            for (int i = 0; i < allCards.Length; i++)
            {
                Card c = allCards[i];

                cardData[(int)c.Suit - 1] |= 1 << c.Rank - 2;

                suits[(int)c.Suit - 1]++;
            }

            // check for flush, straight flush, and royal flush
            int highCard;

            // find suit with 5 or more cards
            for (int i = 0; i < cardData.Length; i++)
            {
                if (suits[i] >= 5)
                {
                    handRank = PokerHandRank.Flush;
                    Card.CardSuit suit = (Card.CardSuit)Enum.GetValues(typeof(Card.CardSuit)).GetValue(i + 1);

                    // check for straight flush or royal flush
                    int suitedCards = cardData[i];
                    highCard = StraightHelper(suitedCards, true);
                    
                    // if straight flush or flush, collect cards
                    if (highCard != 0)
                        CardCollector_Straight(allCards, highCard, true, suit);
                    else
                        CardCollector_Flush(allCards, suit);

                    return;
                }
            }

            // check for combos (pair, two pair, trips, full house, quads)
            for (int i = 0; i < allCards.Length; i++)
            {
                Card cardToAnalyze = allCards[i];

                // if card has already been analyzed, skip it
                if (analyzed[i])
                    continue;

                analyzed[i] = true;

                for (int j = 0; j < allCards.Length; j++)
                {
                    Card cardToCompare = allCards[j];

                    if (i != j && !analyzed[j] && cardToAnalyze.Rank == cardToCompare.Rank)
                    {
                        bool added = false;
                        foreach (var combo in combos)
                        {
                            if (combo[0].Rank == cardToAnalyze.Rank)
                            {
                                combo.Add(cardToCompare);
                                analyzed[j] = true;
                                added = true;
                                break;
                            }
                        }

                        if (!added)
                        {
                            combos.Add(new List<Card>());
                            combos[combos.Count - 1].Add(cardToAnalyze);
                            combos[combos.Count - 1].Add(cardToCompare);
                            analyzed[j] = true;
                        }
                    }
                }
            }

            // check for quads
            for (int i = 0; i < combos.Count; i++)
            {
                if (combos[i].Count == 4)
                {
                    handRank = PokerHandRank.Quads;

                    for (int j = 0; j < 4; j++)
                        cards[j] = combos[i][j];

                    Card kicker = new Card(Card.CardSuit.None, 2);
                    for (int j = 0; j < allCards.Length; j++)
                    {
                        if (allCards[j].Rank != cards[0].Rank && (kicker.Suit == Card.CardSuit.None || allCards[j].Rank >= kicker.Rank))
                            kicker = allCards[j];
                    }

                    cards[4] = kicker;

                    return;
                }
            }

            // check for straight
            if (combos.Count < 3)
            {
                int allCardData = cardData[0] | cardData[1] | cardData[2] | cardData[3];
                highCard = StraightHelper(allCardData, false);

                if (highCard != 0)
                    CardCollector_Straight(allCards, highCard, false);
            }

            // check for trips
            Card[] trip = new Card[3];
            bool hasTrip = false;
            for (int i = 0; i < combos.Count; i++)
            {
                if (combos[i].Count == 3 && (trip[0] == null || combos[i][0].Rank > trip[0].Rank))
                {
                    hasTrip = true;

                    for (int j = 0; j < 3; j++)
                        trip[j] = combos[i][j];
                }
            }

            if (hasTrip)
            {
                // check for full house
                if (combos.Count > 1)
                {
                    handRank = PokerHandRank.Full_House;

                    Card[] pair = new Card[2];
                    for (int i = 0; i < combos.Count; i++)
                    {
                        if (combos[i][0].Rank != trip[0].Rank && (pair[0] == null || combos[i][0].Rank > pair[0].Rank))
                        {
                            for (int j = 0; j < 2; j++)
                                pair[j] = combos[i][j];
                        }
                    }

                    cards[0] = trip[0];
                    cards[1] = trip[1];
                    cards[2] = trip[2];

                    cards[3] = pair[0];
                    cards[4] = pair[1];

                    return;
                }
                // check for straight with trips
                else if (handRank == PokerHandRank.Straight)
                    return;
                else if (combos.Count == 1)
                {
                    // check for trip
                    handRank = PokerHandRank.Trips;

                    cards[0] = trip[0];
                    cards[1] = trip[1];
                    cards[2] = trip[2];

                    List<Card> kickers = new List<Card>();
                    for (int i = 0; i < allCards.Length; i++)
                    {
                        Card c = allCards[i];

                        if (c.Rank != trip[0].Rank)
                        {
                            if (kickers.Count == 0)
                                kickers.Add(c);
                            else
                            {
                                for (int j = 0; j < kickers.Count; j++)
                                {
                                    if (c.Rank > kickers[j].Rank)
                                    {
                                        kickers.Insert(j, c);
                                        break;
                                    }
                                    else if (j == kickers.Count - 1)
                                    {
                                        kickers.Add(c);
                                        break;
                                    }

                                }
                            }
                        }
                    }

                    cards[3] = kickers[0];
                    cards[4] = kickers[1];

                    return;
                }
            }

            // check for straight
            if (handRank == PokerHandRank.Straight)
                return;
            else if (combos.Count > 1)
            {
                // check for two pair
                handRank = PokerHandRank.Two_Pair;

                Card[] highPair = new Card[2];
                Card[] lowPair = new Card[2];
                for (int i = 0; i < combos.Count; i++)
                {
                    if (i == 0)
                    {
                        highPair[0] = combos[0][0];
                        highPair[1] = combos[0][1];
                    }
                    else if (combos[i][0].Rank > highPair[0].Rank)
                    {
                        lowPair[0] = highPair[0];
                        lowPair[1] = highPair[1];

                        highPair[0] = combos[i][0];
                        highPair[1] = combos[i][1];
                    }
                    else if (lowPair[0] == null || combos[i][0].Rank > lowPair[0].Rank)
                    {
                        lowPair[0] = combos[i][0];
                        lowPair[1] = combos[i][1];
                    }
                }

                Card kicker = new Card(Card.CardSuit.None, 2);
                for (int j = 0; j < allCards.Length; j++)
                {
                    Card c = allCards[j];

                    if (c.Rank != highPair[0].Rank && c.Rank != lowPair[0].Rank && (kicker.Suit == Card.CardSuit.None || c.Rank >= kicker.Rank))
                        kicker = c;
                }

                cards[0] = highPair[0];
                cards[1] = highPair[1];
                cards[2] = lowPair[0];
                cards[3] = lowPair[1];
                cards[4] = kicker;
            }
            else if (combos.Count == 1)
            {
                // check for pair
                handRank = PokerHandRank.Pair;

                Card[] pair = new Card[2];
                pair[0] = combos[0][0];
                pair[1] = combos[0][1];

                List<Card> kickers = new List<Card>();
                for (int i = 0; i < allCards.Length; i++)
                {
                    Card c = allCards[i];

                    if (c.Rank != pair[0].Rank)
                    {
                        if (kickers.Count == 0)
                            kickers.Add(c);
                        else
                        {
                            for (int j = 0; j < kickers.Count; j++)
                            {
                                if (c.Rank > kickers[j].Rank)
                                {
                                    kickers.Insert(j, c);
                                    break;
                                }
                                else if (j == kickers.Count - 1)
                                {
                                    kickers.Add(c);
                                    break;
                                }
                            }
                        }
                    }
                }

                cards[0] = pair[0];
                cards[1] = pair[1];
                cards[2] = kickers[0];
                cards[3] = kickers[1];
                cards[4] = kickers[2];
            }
            else
            {
                // check high card
                handRank = PokerHandRank.High_Card;

                List<Card> kickers = new List<Card>();
                for (int i = 0; i < allCards.Length; i++)
                {
                    Card c = allCards[i];

                    if (kickers.Count == 0)
                        kickers.Add(c);
                    else
                    {
                        for (int j = 0; j < kickers.Count; j++)
                        {
                            if (c.Rank > kickers[j].Rank)
                            {
                                kickers.Insert(j, c);
                                break;
                            }
                            else if (j == kickers.Count - 1)
                            {
                                kickers.Add(c);
                                break;
                            }
                        }
                    }
                }

                cards[0] = kickers[0];
                cards[1] = kickers[1];
                cards[2] = kickers[2];
                cards[3] = kickers[3];
                cards[4] = kickers[4];
            }
        }
        int StraightHelper(int cardData, bool flush)
        {
            for (int i = 0; i < 9; i++)
            {
                int bitMask = 31 << 8 - i;

                if ((cardData & bitMask) == bitMask)
                {
                    if (flush)
                    {
                        if (i == 0)
                            handRank = PokerHandRank.Royal_Flush;
                        else
                            handRank = PokerHandRank.Straight_Flush;
                    }
                    else
                        handRank = PokerHandRank.Straight;

                    return 14 - i;
                }
            }

            if ((cardData & 4111) == 4111)
            {
                if (flush)
                    handRank = PokerHandRank.Straight_Flush;
                else
                    handRank = PokerHandRank.Straight;

                return 5;
            }

            return 0;
        }
        void CardCollector_Straight(Card[] allCards, int highCard, bool flush, Card.CardSuit suit = Card.CardSuit.None)
        {
            int cardCount = 0;
            for (int i = 0; i < cards.Length; i++)
            {
                // if 5 high straight, highcard will be 1 with ace, but ace equals 14
                if (highCard == 1)
                    highCard = 14;

                // find the card that matches the rank and, if there's a flush, the suit
                for (int j = 0; j < allCards.Length; j++)
                {
                    Card c = allCards[j];

                    if (c.Rank == highCard)
                    {
                        if (flush)
                        {
                            if (c.Suit == suit)
                            {
                                highCard--;
                                cards[cardCount++] = c;

                                break;
                            }
                        }
                        else
                        {
                            highCard--;
                            cards[cardCount++] = c;

                            break;
                        }
                    }
                }
            }
        }
        void CardCollector_Flush(Card[] allCards, Card.CardSuit suit)
        {
            List<Card> _cards = new List<Card>();
            for (int i = 0; i < allCards.Length && _cards.Count < 5; i++)
            {
                Card c = allCards[i];

                if (c.Suit == suit)
                {
                    if (_cards.Count == 0)
                        _cards.Add(c);
                    else
                    {
                        for (int j = 0; j < _cards.Count; j++)
                        {
                            if (_cards[j].Rank < c.Rank)
                            {
                                _cards.Insert(j, c);
                                break;
                            }
                            else if (j == _cards.Count - 1)
                            {
                                _cards.Add(c);
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                cards[i] = _cards[i];
            }
        }
        public static WinStatus HandComparison(Hand hand1, Hand hand2)
        {
            if (hand1.handRank > hand2.handRank)
                return WinStatus.Win;
            else if (hand1.handRank == hand2.handRank)
            {
                for (int i = 0; i < hand1.cards.Length; i++)
                {
                    if (hand1.cards[i].Rank > hand2.cards[i].Rank)
                        return WinStatus.Win;
                    else if (hand1.cards[i].Rank < hand2.cards[i].Rank)
                        return WinStatus.Lose;
                }

                return WinStatus.Tie;
            }

            return WinStatus.Lose;
        }
    }
}
