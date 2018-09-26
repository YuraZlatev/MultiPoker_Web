using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MultiPoker.Models
{
    public class Game
    {
        [Key]
        public int Id { set; get; }

        [Required]
        [MaxLength(128)]
        [Index(IsUnique =true)]
        public String Name { set; get; } //название покерной игры в таблице
    }
}