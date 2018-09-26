using Microsoft.AspNet.Identity;
using MultiPoker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows;

namespace MultiPoker.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        MultiPokerContext db = new MultiPokerContext();

        private Room RoomById(int id)
        {
            Room room = null;

            foreach (Room r in MvcApplication.rooms)
                if (r.RoomId == id)
                {
                    room = r;
                    break;
                }

            return room;
        }

        public ActionResult Index(int room_id, String game)
        {
            ViewBag.Title = game;
            Room room = RoomById(room_id);

            String userID = User.Identity.GetUserId();
            if (room.IsPlayer(userID) == null)
            {
                Player player = db.Players.Find(userID);
                room.Players.Add(player);
            }

            ViewBag.Room = room;
            return View();
        }

        /// <summary>
        /// Возвращает частичное представление игрового места для player по Place
        /// </summary>
        /// <param name="player"></param>
        /// <param name="flag">true - append, false - remove</param>
        /// <returns></returns>
        public PartialViewResult GetInterfaceByPlace(Player player, bool flag)
        {
            String partial = "";
            switch(player.Game.Place)
            {
                case 0: partial = "Interface/Interface1"; break;
                case 1: partial = "Interface/Interface2"; break;
                case 2: partial = "Interface/Interface3"; break;
                case 3: partial = "Interface/Interface4"; break;
                case 4: partial = "Interface/Interface5"; break;
                case 5: partial = "Interface/Interface6"; break;
                case 6: partial = "Interface/Interface7"; break;
                case 7: partial = "Interface/Interface8"; break;
                case 8: partial = "Interface/Interface9"; break;
                default: break;
            }

            if (partial == "")
                return null;

            if(flag)
                return PartialView(partial, player);
            else
                return PartialView(partial, null);
        }

        public JsonResult Info(int room_id)
        {
            Room room = null;
            Player player = null;

            room = RoomById(room_id);
            player = room.IsPlayer(User.Identity.GetUserId());

            JsonResult json = new JsonResult();
            json.MaxJsonLength = 2000000000;
            json.Data = new { room, player};

            return json;
        }
    }
}