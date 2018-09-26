using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiPoker.Tools
{
    public class Card
    {
        public Card(int id, String name, String suit, int value, String path)
        {
            this.Id = id;
            this.Name = name;
            this.Suit = suit;
            this.Value = value;
            this.ImagePath = path;
        }
        public int Id { set; get; }
        public String Suit { set; get; }
        public String Name { set; get; }
        public int Value { set; get; }
        public String ImagePath { set; get; }

        /// <summary>
        /// Возвращает строку карт для сохранения в БД. Пример: 10-Diamonds|J-Diamonds|Q-Diamonds|K-Diamonds|A-Diamonds
        /// </summary>
        /// <param name="cards">Список карт</param>
        /// <returns></returns>
        public static String StringFormat(List<Card> cards)
        {
            String str = "";
            List<Card> list = cards.ToList();

            if (list.Count == 5)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i + 1 < list.Count)
                        str += list[i].Name + "-" + list[i].Suit + "|";
                    else
                        str += list[i].Name + "-" + list[i].Suit;
                }
            }

            return str;
        }

        /// <summary>
        /// Возвращает список карт из строки
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<Card> CardsFormat(String str)
        {
            List<Card> cards = new List<Card>();

            if (str != "")
            {
                String[] cs = str.Split(new String[] { "|" }, StringSplitOptions.None);
                for (int i = 0; i < 5; i++)
                {
                    String[] c = cs[i].Split(new String[] { "-" }, StringSplitOptions.None);

                    int value = 0;
                    switch (c[0])
                    {
                        case "2": value = 2; break;
                        case "3": value = 3; break;
                        case "4": value = 4; break;
                        case "5": value = 5; break;
                        case "6": value = 6; break;
                        case "7": value = 7; break;
                        case "8": value = 8; break;
                        case "9": value = 9; break;
                        case "10": value = 10; break;
                        case "J": value = 11; break;
                        case "Q": value = 12; break;
                        case "K": value = 13; break;
                        case "A": value = 14; break;
                    }
                    Card card = new Card(0, c[0], c[1], value, "");
                    cards.Add(card);
                }
            }

            return cards;
        }

        public static String[] CardsByString(String str)
        {
            String[] mass = new String[6];
            String[] cards = str.Split(new String[] { "|"}, StringSplitOptions.None);
            List<Card> combName = new List<Card>();
            
            for(int i = 0; i < 5; i++)
            {
                String[] card = cards[i].Split(new String[] { "-"}, StringSplitOptions.None);
                mass[i] = "/Images/Cards/" + card[1] + "/" + card[0] + ".jpg";

                int value = 0;
                switch (card[0])
                {
                    case "2": value = 2; break;
                    case "3": value = 3; break;
                    case "4": value = 4; break;
                    case "5": value = 5; break;
                    case "6": value = 6; break;
                    case "7": value = 7; break;
                    case "8": value = 8; break;
                    case "9": value = 9; break;
                    case "10": value = 10; break;
                    case "J": value = 11; break;
                    case "Q": value = 12; break;
                    case "K": value = 13; break;
                    case "A": value = 14; break;
                }
                Card c = new Card(0, card[0], card[1], value, "");
                combName.Add(c);
            }

            if (combName.Count != 0)
            {
                String name = Deck.CombinationName(Deck.GetCombination(combName));
                mass[5] = name;
            }
            else
                mass[5] = "No Result";

            return mass;
        }
    }
}