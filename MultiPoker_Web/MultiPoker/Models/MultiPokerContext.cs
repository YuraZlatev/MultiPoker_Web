using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MultiPoker.Models
{
    public class MultiPokerContext : DbContext
    {
        public MultiPokerContext() : base("DefaultConnection") { }

        public DbSet<Game> Games { set; get; }
        public DbSet<Statistic> Statistics { set; get; }
        public DbSet<Player> Players { set; get; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}