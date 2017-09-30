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

namespace SecuritySystem.Controllers
{
    public class UserController : Controller
    {
        private UserRepository _userRepository;

        public UserController()
        {
            _userRepository = new UserRepository();
        }

        // GET: User
        public ActionResult Index()
        {
            var users = _userRepository.GetUsers();
            return View(users);
        }

        // GET: User/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userRepository.GetUserById(id.Value);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: User/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,UserName,LastName,FirstName,UserPassword,Email")] User user)
        {
            bool Status = false;
            string message = "";
            if (ModelState.IsValid)
            {
                #region//Email is already in use
                var isExist = IsEmailExist(user.Email);
                if (isExist) {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion
                #region//Username is already in use
                var isUNExist = IsUsernameExist(user.UserName);
                if (isUNExist)
                {
                    ModelState.AddModelError("UsernameExist", "Username already exist");
                    return View(user);
                }
                #endregion

                //password is hashed in the userRepository before added to database
                _userRepository.CreateUser(user);

                //send email to user
                SendEmail(user.Email);
                message = "Registration successfully done. Validation email has been sent to your email address";
                Status = true;
             //   return RedirectToAction("Index");
            }
            else {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        // User login
        public ActionResult login() {
            if (Session["user"] != null)
            {
                return RedirectToAction("Welcomepage", "User", new { user = Session["user"] });
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult login(User user)
        {
            SecuritySystemEntities db = new SecuritySystemEntities();
            Encryptor encryptor = new Encryptor();
            var userPasswordRaw = user.UserPassword;
            var userPasswordEncrypted = encryptor.Encrypt(user.UserPassword);
            //user.UserPassword = encryptor.Encrypt(user.UserPassword);
            var userLoggedIn = db.Users.FirstOrDefault(x => x.UserName == user.UserName && x.UserPassword == userPasswordEncrypted);
            if (userLoggedIn != null)
            {
                ViewBag.message = "loggedin";
                ViewBag.triedOnce = "yes";

                // Session["username"] = user.UserName;
                Session["user"] = userLoggedIn;
                return RedirectToAction("Welcomepage", "User", new { username = userLoggedIn?.UserName});
            }
            else
            {
                ViewBag.triedOnce = "yes";
                return View();
            }
        }
        // GET: User/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = _userRepository.GetUserById(id.Value);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,UserName,LastName,FirstName,UserPassword,Email")] User user)
        {
            if (ModelState.IsValid)
            {
                _userRepository.EditUser(user);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: User/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = _userRepository.GetUserById(id.Value);
            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _userRepository.DeleteUser(id);
            return RedirectToAction("Index");
        }

        public ActionResult Welcomepage(User user)
        {
            return View(user);
        }

        //Check if email is in use in another profile
        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (SecuritySystemEntities dc = new SecuritySystemEntities())
            {
                var v = dc.Users.Where(x => x.Email == emailID).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
        public bool IsUsernameExist(string username)
        {
            using (SecuritySystemEntities dc = new SecuritySystemEntities())
            {
                var v = dc.Users.Where(x => x.UserName == username).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
        public void SendEmail(string email) {
            var fromEmail = new MailAddress("kriheg1234@gmail.com", "Security System");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "Hegkri89";
            string subject = "Your account has been successfully created!";
            string body = "<br/><br/>We are excited to tell you that you account at Security System" +
                "has been successfully created";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials= new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            smtp.Send(message);

        }
    }
}
