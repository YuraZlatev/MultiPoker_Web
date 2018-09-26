using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MultiPoker.Models
{
    public class Statistic
    {
        public Statistic()
        {
            
        }

        [Key]
        public int Id { set; get; }
        public int TotalGames { set; get; }
        public int Wins { set; get; }
        public int MaxGain { set; get; } //макс. выигрышь за игру
        public String BestHand { set; get; } //пример: 10-Diamonds|J-Diamonds|Q-Diamonds|K-Diamonds|A-Diamonds

        [Required]
        public Player player { set; get; }
        [Required]
        public Game game { set; get; }
    }
}