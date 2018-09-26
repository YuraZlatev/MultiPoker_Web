using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MultiPoker.Models
{
    public class Player
    {
        public Player()
        {
            this.Game = new GameInfo();
        }

        [Key]
        [MaxLength(255)]
        public String Id { set; get; }

        [MaxLength(12)]
        public String Nick { set; get; }
        public int Balance { set; get; }

        [MaxLength(2097152)] // 2 мегабайта в байтах
        public byte[] Avatar { set; get; }

        public int Level { set; get; }
        public int Experience { set; get; }
        public DateTime Bonus { set; get; }
        
        [NotMapped]
        public GameInfo Game { set; get; }
    }
}