using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MultiPoker.Models;
using System.Windows;
using Microsoft.AspNet.Identity;

namespace MultiPoker.Controllers
{
    public class HomeController : Controller
    {
        MultiPokerContext db = new MultiPokerContext();

        public ActionResult Index()
        {
            return View();
        }


        [Authorize]
        public String GetTIme()
        {
            Player player = null;
            try
            {
                player = db.Players.Find(User.Identity.GetUserId());
            }
            catch { }

            if (player == null)
                return "";
            else
            {
                String format = player.Bonus.ToString();
                String[] mass = format.Split(' ');
                String[] date = mass[0].Split('.');
                String[] time = mass[1].Split(':');
                String form = date[2] + "|" + date[1] + "|" + date[0] + "|" + time[0] + "|" + time[1] + "|" + time[2];

                return form;
            }
        }

        [Authorize]
        public String GetBonus()
        {
            String id = User.Identity.GetUserId();
            int bonus = new Random().Next(50, 101) * 1000;
            try
            {
                Player player = db.Players.Find(id);
                player.Bonus = DateTime.Now;
                player.Balance += bonus;
                db.SaveChanges();

                return bonus.ToString();
            }
            catch { return null; }
        }

        [Authorize]
        public ActionResult GameLobby(int game_id)
        {
            Game game = db.Games.Find(game_id);
            ViewBag.Game = game;

            return View();
        }

        [Authorize]
        public ActionResult BackToLobby(String gameName)
        {
            Game game = db.Games.Where(x => x.Name == gameName).FirstOrDefault();

            return RedirectToAction("GameLobby", new { game_id = game.Id});
        }

        [Authorize]
        public JsonResult GetRoomsByType(String type, String game)
        {
            RoomBet bet = Room.RBet(type);
            List<Room> rooms = MvcApplication.rooms.Where(x => x.roomBet == bet && x.RoomGame == game).ToList();

            JsonResult json = new JsonResult();
            json.MaxJsonLength = 2000000000;
            json.Data = rooms;
            
            return json;
        }
    }
}