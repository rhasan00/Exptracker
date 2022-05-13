using ExpenceTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExpenceTracker.Controllers
{
    public class UserController : Controller
    {
      
        private readonly DBContext db;

        public UserController( DBContext _db)
        {
           
            db = _db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["msg"] = "";
            return View();
        }
        [HttpPost]
        public IActionResult Login([Bind] User user)
        {
          string pass=  GeneratePassword(user.password);
          var isuser = db.Users.Where(w=> w.username.Trim()==user.username.Trim() && w.password==pass).FirstOrDefault();

            if (isuser != null)
            {
                var userClaims = new List<Claim>()
                {
                   
                    new Claim(ClaimTypes.Name, isuser.fullname),
                    new Claim(ClaimTypes.Email, isuser.username),                   
                    new Claim(ClaimTypes.Role, isuser.role)
                 };

                var userIdentity = new ClaimsIdentity(userClaims, "User Identity");

                var userPrincipal = new ClaimsPrincipal(new[] { userIdentity });
                HttpContext.SignInAsync(userPrincipal);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["msg"] = "Username or password incorrect";
            }

            return View(user);
        }
      
        public  IActionResult Logout()
        {
             HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        static string GeneratePassword(string pass)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(pass));
                StringBuilder stringbuilder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    stringbuilder.Append(bytes[i].ToString("x2"));
                }
                return stringbuilder.ToString();
            }
        }
    }
}
