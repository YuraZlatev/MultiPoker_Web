using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MultiPoker.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Windows;

namespace MultiPoker.Controllers
{
    [Authorize]
    public class PlayersController : Controller
    {
        private MultiPokerContext db = new MultiPokerContext();

        // GET: Players/Edit/5
        public ActionResult Edit(string id)
        {
            id = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }
            return View(player);
        }

        // POST: Players/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nick,Balance,Avatar,Level,Experience,Bonus")] Player player, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                if (upload != null)
                {
                    if (upload.ContentLength <= 2097152) //меньше равно 2 МБ
                    {
                        MemoryStream ms = new MemoryStream();
                        upload.InputStream.CopyTo(ms);
                        byte[] mass = ms.ToArray();
                        player.Avatar = mass;
                        ms.Close();
                    }
                    else
                        return RedirectToAction("Edit");
                }

                db.Entry(player).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(player);
        }

        // GET: Players/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Player player = db.Players.Find(id);
            if (player == null)
            {
                return HttpNotFound();
            }

            db.Players.Remove(player);
            db.Statistics.RemoveRange(db.Statistics.Where((x) => x.player.Id == player.Id));

            ApplicationDbContext apDb = new ApplicationDbContext();
            ApplicationUser user = apDb.Users.Find(id);
            apDb.Users.Remove(user);

            apDb.SaveChanges();
            db.SaveChanges();

            return RedirectToAction("RemoveUser", "Account");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}