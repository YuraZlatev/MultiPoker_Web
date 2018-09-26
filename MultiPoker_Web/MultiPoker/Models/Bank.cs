using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiPoker.Models
{
    public class Bank
    {
        public Bank()
        {
            this.Index = -1;
            this.IsBankCurrent = true;
            this.CurrentBank = 0;
            this.CurrentPlayers = new List<Player>();
        }
        public int Index { set; get; }
        public bool IsBankCurrent { set; get; }
        public int CurrentBank { set; get; }
        public List<Player> CurrentPlayers { set; get; }
    }
}