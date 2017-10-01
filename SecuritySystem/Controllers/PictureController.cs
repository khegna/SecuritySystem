using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SecuritySystem.Data;
using SecuritySystem.Business.Repositories;
using SecuritySystem.Business.Encryption;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography;

namespace SecuritySystem.Controllers
{
    public class PictureController : Controller
    {
        private PictureRepository _pictureRepository;
        public PictureController() {
            _pictureRepository = new PictureRepository();
        }

        //Add image to database
        public ActionResult AddImage()
        {
            Picture picture = new Picture();
            ViewBag.success = "null";
            return View(picture);
        }
        [HttpPost]
        public ActionResult AddImage(Picture pictureObject, HttpPostedFileBase image1)
        {
            var db = new SecuritySystemEntities();
            var _encryptor = new EncryptorPicture();
            if (Session["user"] != null && image1 != null)
            {
                var user = (User)Session["user"];
                pictureObject.UserId = user.UserId;
                byte[] key;
                using (var md5 = MD5.Create())
                {
                    key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(user.UserPassword));
                }
                var iv = _encryptor.GenerateRandomNumber(16);


                pictureObject.Vector = iv;


                pictureObject.PictureFile = new byte[image1.ContentLength];
                image1.InputStream.Read(pictureObject.PictureFile, 0, image1.ContentLength);

                var encryptedImage = _encryptor.Encrypt(pictureObject.PictureFile, key, iv);
                pictureObject.PictureFile = encryptedImage;
            }
            else
            {
                return View();
            }
            ViewBag.success = "Your image has been successfully uploaded";
            db.Pictures.Add(pictureObject);
            db.SaveChanges();
            return View(pictureObject);
        }


        //Display image for database
        public ActionResult DisplayImage()
        {
            var _encryptor = new EncryptorPicture();
            if (Session["user"] != null)
            {
                var userSession = (User)Session["user"];
                var pictures = _pictureRepository.GetPicturesByUserId(userSession.UserId);
                List<byte[]> decryptedPictures = new List<byte[]>();
    
                foreach (var item in pictures)
                {
                    var iv = item.Vector;
                    byte[] key;
                    using (var md5 = MD5.Create())
                    {
                        key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(userSession.UserPassword));
                    }

                    var decryptedPicture = _encryptor.Decrypt(item.PictureFile, key, iv);
                    decryptedPictures.Add(decryptedPicture);

                }
                ViewBag.pictureList = decryptedPictures;
                
                return View(pictures);
            }
            else
            {
                return RedirectToAction("login", "User");
            }
        }

    }
}