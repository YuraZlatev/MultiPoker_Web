using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MultiPoker.Models;
using MultiPoker.Tools;

namespace MultiPoker.Models
{
    public class GameInfo
    {
        public GameInfo()
        {
            this.ConnectionId = null;
            this.RoomId = -1;
            this.Place = -1;
            this.IsInsolvent = false;
            this.IsPlayNow = false;
            this.IsActive = false;
            this.IsFold = false;
            this.IsWasMove = false;
            this.IsBigBlind = false;
            this.IsSmallBlind = false;
            this.IsWinGame = false;
            this.Gain = 0;
            this.Bet = 0;
            this.CurrentBalance = 0;
            this.Exp = 0;
            this.Cards = new Stack<Card>();
            this.BestHand = new List<Card>();
        }

        public String ConnectionId { set; get; }
        public int RoomId { set; get; }
        public int Place { set; get; }  //игровое место за столом
        public bool IsInsolvent { set; get; } //банкрот
        public bool IsPlayNow { set; get; } //играет ли игрок текущую раздачу
        public bool IsActive { set; get; }
        public bool IsFold { set; get; } //игрок не может ходить если он сказал "Fold"
        public bool IsWasMove { set; get; } //делал ли игрок ход на новом этапе игры
        public bool IsBigBlind { set; get; }
        public bool IsSmallBlind { set; get; }
        public bool IsWinGame { set; get; } //выиграл ли игрок данную игру
        public int Gain { set; get; } //банк, который игрок выиграл за игру
        public int Bet { set; get; }
        public int CurrentBalance { set; get; }
        public int Exp { set; get; } //опыт после окончания игры
        public Stack<Card> Cards { set; get; } //2 карты на руках
        public List<Card> BestHand { set; get; } //лучшие 5 карт из 7

        /// <summary>
        /// Значения по умолчанию
        /// </summary>
        public void ResetData()
        {
            if (!this.IsInsolvent)
                this.IsPlayNow = true;
            else
                this.IsPlayNow = false;

            this.IsActive = false;
            this.IsFold = false;
            this.IsWasMove = false;
            this.IsSmallBlind = false;
            this.IsWinGame = false;
            this.Gain = 0;
            this.Bet = 0;
            this.Exp = 0;
            this.Cards = new Stack<Tools.Card>();
            this.BestHand = new List<Tools.Card>();
        }
    }
}