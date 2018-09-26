using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MultiPoker.Tools;
using System.Windows;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;

namespace MultiPoker.Models
{
    public enum RoomBet { Easy, Normal, Hard}

    public class Room
    {
        private MultiPokerContext db = new MultiPokerContext();

        public Room(int roomId, RoomBet rb, String game)
        {
            this.RoomId = roomId;
            if(game == "Draw Poker")
                this.Places = new int[5] { 0, 0, 0, 0, 0 }; // 0 - свободно
            else
                this.Places = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0}; // 0 - свободно
            this.RoomGame = game;

            if (rb == RoomBet.Easy)
            {
                this.BigBlind = 200;
                this.SmallBlind = 100;
                this.MinimumBalance = 1000;
                this.MaximumBalance = 10000;
            }
            else if(rb == RoomBet.Normal)
            {
                this.BigBlind = 1000;
                this.SmallBlind = 500;
                this.MinimumBalance = 20000;
                this.MaximumBalance = 80000;
            }
            else
            {
                this.BigBlind = 2000;
                this.SmallBlind = 1000;
                this.MinimumBalance = 100000;
                this.MaximumBalance = 500000;
            }

            this.CurrentBet = 0;
            this.roomBet = rb;
            this.IsGameOn = false;
            this.CurPlaceBigBlind = -1;
            this.Bank = new List<Bank>();
            this.Players = new List<Player>();
            this.Deck = Tools.Deck.CreateDeck();
            this.Cards = new List<Card>();

            //инициализация обьектов в тиблицах
            db.Games.ToList();
            db.Players.ToList();
        }
        public String RoomGame { set; get; }
        public RoomBet roomBet { set; get; }
        public int RoomId { set; get; }
        public int[] Places { set; get; }   //игровых мест - 9, в некоторых видах меньше
        public int CurrentBet { set; get; }
        public int BigBlind { set; get; }
        public int SmallBlind { set; get; }
        public int MinimumBalance { get; }
        public int MaximumBalance { get; }
        public bool IsGameOn { set; get; } //идеи ли игра в данный момент
        public int CurPlaceBigBlind { set; get; }
        public List<Bank> Bank { set; get; }
        public List<Player> Players { set; get; }
        public Stack<Card> Deck { set; get; } //колода карт
        public List<Card> Cards { set; get; }


        /// <summary>
        /// Значения по умолчанию
        /// </summary>
        public void ResetData()
        {
            this.Bank = new List<Bank>();
            this.Deck = Tools.Deck.CreateDeck();
            this.Cards = new List<Tools.Card>();
            this.CurrentBet = 0;
        }

        /// <summary>
        /// Возвращает player если он сейчас в комнате
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Player IsPlayer(String id)
        {
            foreach (Player player in Players)
                if (player.Id == id)
                    return player;

            return null;
        }
        public Player PlayerByPlace(int place)
        {
            Player player = null;

            foreach (Player p in this.Players)
                if (p.Game.Place == place)
                {
                    player = p;
                    break;
                }

            return player;
        }
        public bool mySelf(String userID)
        {
            foreach (Player player in this.Players)
                if (player.Id == userID && player.Game.Place != -1)
                    return true;

            return false;
        }
        public static RoomBet RBet(String type)
        {
            switch(type)
            {
                case "normal": return RoomBet.Normal;
                case "hard": return RoomBet.Hard;
                default: return RoomBet.Easy;
            }            
        }

        /// <summary>
        /// Сохранение результатов для каждого игрока после окончания игры
        /// </summary>
        public void SaveGame()
        {

            foreach(Player player in this.Players)
            {
                //если игрок играл игру
                if(player.Game.IsPlayNow && player.Game.Place != -1)
                {
                    int exp = 10;
                    Statistic stat = db.Statistics.Where(s => s.player.Id == player.Id && s.game.Name == this.RoomGame).FirstOrDefault();
                    stat.TotalGames++;
                    if (player.Game.IsWinGame)
                        stat.Wins++;
                    if (player.Game.Gain > stat.MaxGain)
                        stat.MaxGain = player.Game.Gain;
                    if (stat.BestHand == "")
                    {
                        String name = Tools.Card.StringFormat(player.Game.BestHand);
                        stat.BestHand = name;
                        int cur = Tools.Deck.GetCombination(player.Game.BestHand);
                        //увеличение exp в зависимости от комбинации
                        if (player.Game.IsWinGame)
                        {
                            if (cur == 0)
                                exp += 50;
                            else
                                exp += 100 * cur;
                        }
                        else
                        {
                            if (cur == 0)
                                exp += 25;
                            else
                                exp += 30 * cur;
                        }
                    }
                    else if (stat.BestHand != "" && player.Game.BestHand.Count != 0)
                    {
                        List<Card> cards = Tools.Card.CardsFormat(stat.BestHand);
                        int best = Tools.Deck.GetCombination(cards);
                        int cur = Tools.Deck.GetCombination(player.Game.BestHand);
                        //если текущая комбинация больше лучшей
                        if (cur > best)
                            cards = player.Game.BestHand;
                        else if (cur == best) //если комбинации одинаковые
                        {
                            bool IsBest = Tools.Deck.IsCurrentBest(cards, player.Game.BestHand);
                            if (IsBest)
                                cards = player.Game.BestHand;
                        }

                        //увеличение exp в зависимости от комбинации
                        if (player.Game.IsWinGame)
                        {
                            if (cur == 0)
                                exp += 50;
                            else
                                exp += 100 * cur;
                        }
                        else
                        {
                            if (cur == 0)
                                exp += 25;
                            else
                                exp += 30 * cur;
                        }

                        String str = Card.StringFormat(cards);
                        stat.BestHand = str;
                    }

                    player.Game.Exp = exp;
                    Player p = db.Players.Find(player.Id);
                    UpdateExperience(p, exp);
                }
                else if(player.Game.Place != -1 && player.Game.IsFold)
                {
                    int exp = 40;
                    player.Game.Exp = exp;
                    Player p = db.Players.Find(player.Id);
                    UpdateExperience(p, exp);
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Начисляет опыт за игру и при необходимости увеличивает level игрока
        /// </summary>
        private void UpdateExperience(Player player, int exp)
        {
            //необходимый опыт для следующего уровня
            int needExp = player.Level * 500;
            int curExp = player.Experience;
            if(exp + curExp < needExp)
                player.Experience += exp;
            else if(exp + curExp >= needExp)
            {
                int newExp = exp + curExp - needExp;
                player.Level++;
                player.Experience = newExp;
            }
        }
    }
}