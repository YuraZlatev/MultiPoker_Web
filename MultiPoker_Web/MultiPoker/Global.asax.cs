using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MultiPoker.Models;
using System.Windows;

namespace MultiPoker
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static List<Room> rooms = new List<Room>();
        MultiPokerContext db = new MultiPokerContext();
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ///Для каждого вида игры по 10 игровых комнат для 3-х видов ставок/////////////////////////////
            if (db.Games.Where((x) => x.Name == "Texas Holdem Poker").FirstOrDefault() == null)
                db.Games.Add(new Game { Name = "Texas Holdem Poker" });
            if (db.Games.Where((x) => x.Name == "Draw Poker").FirstOrDefault() == null)
                db.Games.Add(new Game { Name = "Draw Poker" });
            db.SaveChanges();

            List<Game> games = db.Games.ToList();

            foreach (Game g in games)
            {
                if (g.Name == "Draw Poker")
                    continue;

                for (int i = 0; i < 30; i++)
                {
                    if (i < 10)
                        rooms.Add(new Room(rooms.Count, RoomBet.Easy, g.Name));
                    else if (i >= 10 && i < 20)
                        rooms.Add(new Room(rooms.Count, RoomBet.Normal, g.Name));
                    else
                        rooms.Add(new Room(rooms.Count, RoomBet.Hard, g.Name));
                }
            }
        }
    }
}
