using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SecuritySystem.Data;
using SecuritySystem.Business.Encryption;
using SecuritySystem.Business.Repositories;
using System.Security.Cryptography;
using System.Text;
using SecuritySystem.Models;

namespace SecuritySystem.Controllers
{
    public class PicturesIndexController : Controller
    {
        private SecuritySystemEntities db = new SecuritySystemEntities();
        private PictureRepository _pictureRepository;
        public PicturesIndexController()
        {
            _pictureRepository = new PictureRepository();
        }

        // GET: PicturesIndex
        public ActionResult Index()
        {
           
            var _encryptor = new EncryptorPicture();
            if (Session["user"] != null)
            {
                var userSession = (User)Session["user"];
                var pictures = _pictureRepository.GetPicturesByUserId(userSession.UserId);
                List<byte[]> decryptedPictures = new List<byte[]>();
                List<int> decryptedPicturesId = new List<int>();
                List<TemporaryPictureClass> tempData = new List<TemporaryPictureClass>();

                foreach (var item in pictures)
                {
                    var iv = item.Vector;
                    byte[] key;
                    using (var md5 = MD5.Create())
                    {
                        key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(userSession.UserPassword));
                    }

                    var decryptedPicture = _encryptor.Decrypt(item.PictureFile, key, iv);
                    var pictureDecryptId = item.PictureID;
                    TemporaryPictureClass temporary = new TemporaryPictureClass();
                    temporary.PictureID = item.PictureID;
                    temporary.PictureFile = decryptedPicture;
                    decryptedPicturesId.Add(pictureDecryptId);
                    decryptedPictures.Add(decryptedPicture);
                    tempData.Add(temporary);

                }
                ViewBag.pictureListId = decryptedPicturesId;
                ViewBag.temp = tempData;
                ViewBag.pictureList = decryptedPictures;
                ViewData["pictureIds"] = decryptedPicturesId;
                ViewBag.count = 0;

                return View(pictures);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }
            // GET: PicturesIndex/Details/5
            public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return HttpNotFound();
            }
            return View(picture);
        }

        // GET: PicturesIndex/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName");
            return View();
        }

        // POST: PicturesIndex/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PictureID,PictureFile,ImageName,UserId,DecryptKey,Vector")] Picture picture)
        {
            if (ModelState.IsValid)
            {
                db.Pictures.Add(picture);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", picture.UserId);
            return View(picture);
        }

        // GET: PicturesIndex/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", picture.UserId);
            return View(picture);
        }

        // POST: PicturesIndex/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PictureID,PictureFile,ImageName,UserId,DecryptKey,Vector")] Picture picture)
        {
            if (ModelState.IsValid)
            {
                db.Entry(picture).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "UserId", "UserName", picture.UserId);
            return View(picture);
        }

        // GET: PicturesIndex/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Picture picture = db.Pictures.Find(id);
            if (picture == null)
            {
                return HttpNotFound();
            }
            return View(picture);
        }

        // POST: PicturesIndex/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Picture picture = db.Pictures.Find(id);
            db.Pictures.Remove(picture);
            db.SaveChanges();
            return RedirectToAction("Index");
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
