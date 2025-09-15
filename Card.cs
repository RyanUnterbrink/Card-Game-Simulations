
namespace Card_Game_Simulations
{
    public class Card
    {
        public enum CardSuit
        {
            None = 0,
            Spades,
            Hearts,
            Clubs,
            Diamonds,
            Joker
        }
        public enum CardColor
        {
            None = 0,
            Black,
            Red
        }

        public CardSuit Suit
        {
            get;
            private set;
        }
        public CardColor Card_Color
        {
            get;
            private set;
        }
        public int Rank
        {
            get;
            private set;
        }
        public string RankSymbol
        {
            get;
            private set;
        }
        public int ID
        {
            get;
            private set;
        }
        public bool IsJoker
        {
            get
            {
                return Suit == CardSuit.Joker;
            }
        }
        public bool Dealt
        {
            get;
            private set;
        }

        public Card(CardSuit suit, int rank, CardColor color = CardColor.None)
        {
            Suit = suit;
            if (color == CardColor.None)
            {
                if (suit == CardSuit.Spades || suit == CardSuit.Clubs)
                    Card_Color = CardColor.Black;
                else
                    Card_Color = CardColor.Red;
            }
            else
                Card_Color = color;

            Rank = rank;

            if (Rank > 0)
            {
                string[] symbols = new string[]
                {
                    "Jk", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"
                };
                RankSymbol = symbols[rank - 1];
            }
            else
                RankSymbol = "";

            ID |= 1 << 14 + Rank;
        }

        public void Deal()
        {
            Dealt = true;
        }
        public void Collect()
        {
            Dealt = false;
        }
    }
}



//int OnBit(int num, int bitPos)
//{
//    int bitMask = 1 << bitPos;

//    return num | bitMask;
//}
//int OffBit(int num, int bitPos)
//{
//    int bitMask = 1 << bitPos;

//    return num | bitMask;
//}
//int OnOffBit(int num, int bitPos, bool on)
//{
//    if (on)
//        return OnBit(num, bitPos);
//    else
//        return OffBit(num, bitPos);
//}
//int ToggleBit(int num, int bitPos)
//{
//    int bitMask = 1 << bitPos;

//    return num ^= bitMask;
//}
//bool IsBitOn(int num, int bitPos)
//{
//    return (num & (1 << bitPos)) != 0;
//}