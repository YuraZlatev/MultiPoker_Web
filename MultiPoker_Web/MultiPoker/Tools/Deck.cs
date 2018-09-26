using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiPoker.Tools
{
    public class Deck
    {
        static String[] Suits = { "Clubs", "Diamonds", "Hearts", "Spades" };
        public static Stack<Card> CreateDeck()
        {
            Stack<Card> deck = new Stack<Card>();

            for(int i = 0; i < 4; i++)
            {
                Card c1 = new Card(i * 13 + 1, "2", Suits[i], 2, "/Images/Cards/" + Suits[i] + "/2.jpg");
                Card c2 = new Card(i * 13 + 2, "3", Suits[i], 3, "/Images/Cards/" + Suits[i] + "/3.jpg");
                Card c3 = new Card(i * 13 + 3, "4", Suits[i], 4, "/Images/Cards/" + Suits[i] + "/4.jpg");
                Card c4 = new Card(i * 13 + 4, "5", Suits[i], 5, "/Images/Cards/" + Suits[i] + "/5.jpg");
                Card c5 = new Card(i * 13 + 5, "6", Suits[i], 6, "/Images/Cards/" + Suits[i] + "/6.jpg");
                Card c6 = new Card(i * 13 + 6, "7", Suits[i], 7, "/Images/Cards/" + Suits[i] + "/7.jpg");
                Card c7 = new Card(i * 13 + 7, "8", Suits[i], 8, "/Images/Cards/" + Suits[i] + "/8.jpg");
                Card c8 = new Card(i * 13 + 8, "9", Suits[i], 9, "/Images/Cards/" + Suits[i] + "/9.jpg");
                Card c9 = new Card(i * 13 + 9, "10", Suits[i], 10, "/Images/Cards/" + Suits[i] + "/10.jpg");
                Card c10 = new Card(i * 13 + 10, "J", Suits[i], 11, "/Images/Cards/" + Suits[i] + "/J.jpg");
                Card c11 = new Card(i * 13 + 11, "Q", Suits[i], 12, "/Images/Cards/" + Suits[i] + "/Q.jpg");
                Card c12 = new Card(i * 13 + 12, "K", Suits[i], 13, "/Images/Cards/" + Suits[i] + "/K.jpg");
                Card c13 = new Card(i * 13 + 13, "A", Suits[i], 14, "/Images/Cards/" + Suits[i] + "/A.jpg");

                deck.Push(c1);
                deck.Push(c2);
                deck.Push(c3);
                deck.Push(c4);
                deck.Push(c5);
                deck.Push(c6);
                deck.Push(c7);
                deck.Push(c8);
                deck.Push(c9);
                deck.Push(c10);
                deck.Push(c11);
                deck.Push(c12);
                deck.Push(c13);
            }

            Random r = new Random();
            deck = new Stack<Card>(deck.OrderBy(x => r.Next())); //сортировка колоды

            return deck;
        }

        public static List<Card> SortCards(List<Card> cards)
        {
            List<Card> newCards = new List<Card>();
            foreach (Card c in cards)
                newCards.Add(c);

            //карты от меньшего к большему по Value
            for(int i = 0; i < newCards.Count; i++)
            {
                for(int j = i + 1; j < newCards.Count; j++)
                {
                    if(newCards[i].Value > newCards[j].Value)
                    {
                        Card c = newCards[i];
                        newCards[i] = newCards[j];
                        newCards[j] = c;
                    }
                }
            }
            return newCards;
        }

        /// <summary>
        /// ROYAL FLUSH, STRAIGHT FLUSH, BACK STRAIGHT FLUSH, FOUR OF A KIND, FULL HOUSE, FLUSH, STRAIGHT, BACK STRAIGHT, THREE OF A KIND, TWO PAIR, PAIR, HIGH CARD
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static int GetCombination(List<Card> cards)
        {
            //royal flush
            int royal = cards.Count(c => c.Suit == cards[0].Suit);
            if(royal == 5)
            {
                if (cards[0].Name == "10" && cards[1].Name == "J" && cards[2].Name == "Q" && cards[3].Name == "K" && cards[4].Name == "A")
                    return 11;
            }
            //straight flush
            int strFlush = cards.Count(c => c.Suit == cards[0].Suit);
            if(strFlush == 5)
            {
                if (cards[0].Value == cards[1].Value - 1 && cards[1].Value == cards[2].Value - 1 && cards[2].Value == cards[3].Value - 1 && cards[3].Value == cards[4].Value - 1)
                    return 10;
            }
            //back straight flush
            int backStrFlush = strFlush;
            if(backStrFlush == 5)
            {
                if (cards[0].Name == "2" && cards[1].Name == "3" && cards[2].Name == "4" && cards[3].Name == "5" && cards[4].Name == "A")
                    return 9;
            }
            //four of a kind
            for(int i=0; i<cards.Count; i++)
            {
                int count = cards.Count(c => c.Value == cards[i].Value);
                if (count == 4)
                    return 8;
            }
            //full house
            bool fullhouse = false;
            if (cards[0].Value == cards[1].Value && cards[1].Value == cards[2].Value && cards[3].Value == cards[4].Value)
                fullhouse = true;
            else if(cards[0].Value == cards[1].Value && cards[2].Value == cards[3].Value && cards[3].Value == cards[4].Value)
                fullhouse = true;

            if (fullhouse)
                return 7;
            //flush
            int flush = cards.Count(c => c.Suit == cards[0].Suit);
            if (flush == 5)
                return 6;
            //straight
            if (cards[0].Value == cards[1].Value - 1 && cards[1].Value == cards[2].Value - 1 && cards[2].Value == cards[3].Value - 1 && cards[3].Value == cards[4].Value - 1)
                return 5;
            //back straight
            if (cards[0].Name == "2" && cards[1].Name == "3" && cards[2].Name == "4" && cards[3].Name == "5" && cards[4].Name == "A")
                return 4;
            //three of a kind
            for (int i = 0; i < cards.Count; i++)
            {
                int count = cards.Count(c => c.Value == cards[i].Value);
                if (count == 3)
                    return 3;
            }
            //two pair
            bool twoPair = false;
            if (cards[0].Value == cards[1].Value && cards[2].Value == cards[3].Value)
                twoPair = true;
            else if (cards[1].Value == cards[2].Value && cards[3].Value == cards[4].Value)
                twoPair = true;
            else if (cards[0].Value == cards[1].Value && cards[3].Value == cards[4].Value)
                twoPair = true;

            if (twoPair)
                return 2;
            //pair
            for (int i = 0; i < cards.Count; i++)
            {
                int count = cards.Count(c => c.Value == cards[i].Value);
                if (count == 2)
                    return 1;
            }

            //high card
            return 0;
        }

        /// <summary>
        /// Проверяет является текущая комбинация лучше топовой
        /// </summary>
        /// <param name="best">Топовая комбинация</param>
        /// <param name="cur">Текущая комбинация</param>
        /// <returns></returns>
        public static bool IsCurrentBest(List<Card> best, List<Card> cur)
        {
            int bestCount = 0;
            int curCount = 0;

            foreach (Card card in best)
                bestCount += card.Value;

            foreach (Card card in cur)
                curCount += card.Value;

            if (curCount > bestCount)
                return true;

            return false;
        }

        public static String CombinationName(int p)
        {
            switch (p)
            {
                case 1: return "Pair";
                case 2: return "Two Pair";
                case 3: return "Three of a kind";
                case 4: return "Back Straight";
                case 5: return "Straight";
                case 6: return "Flush";
                case 7: return "Full House";
                case 8: return "Four of a kind";
                case 9: return "Back Straight Flush";
                case 10: return "Straight Flush";
                case 11: return "Royal Flush";
                default: return "High Card";
            }
        }
    }
}