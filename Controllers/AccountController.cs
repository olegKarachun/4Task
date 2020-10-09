using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task4.ViewModels;
using Task4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;


namespace Task4.Controllers
{
    public class AccountController : Controller
    {
        private UserContext db;
        public AccountController(UserContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        { 
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
                DateTime logTime = DateTime.Now;

                if (user == null)
                {
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль");
                }
                else if (user != null && !user.IsBlocked)
                {
                    await Authenticate(model.Email);
                    user.LogDate = logTime.ToLongDateString();                    
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Вы заблокированы");
                }             
            }            
            return View(model);                       
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                string mailPattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";
                User user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null && !Regex.IsMatch(model.Email, mailPattern, RegexOptions.IgnoreCase))
                {
                    ModelState.AddModelError("", "Введите корректный Email");
                }
                else if (user == null && model.Name == model.Password)
                {
                    ModelState.AddModelError("", "Имя и пароль не должны совпадать");
                }
                else if(user == null && model.Name != model.Password)
                {                    
                    User newUser = new User { Email = model.Email, Password = model.Password, Name = model.Name };                    
                    db.Users.Add(newUser);                    
                    await db.SaveChangesAsync();
                    await Authenticate(model.Email);
                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            db.SaveChanges();            
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };            
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Dictionary<string, bool> check)
        {            
            foreach (var item in check)
            {
                try
                {
                    User user = await db.Users.FirstOrDefaultAsync(u => u.Email == item.Key);
                    db.Users.Remove(user);
                    db.SaveChanges();
                }
                catch (NullReferenceException )
                {                    
                }                
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Block(Dictionary<string, bool> check)
        {
            foreach (var item in check)
            {
                try
                {
                    User user = await db.Users.FirstOrDefaultAsync(u => u.Email == item.Key);
                    user.IsBlocked = true;
                    db.SaveChanges();
                }
                catch (NullReferenceException)
                {                    
                }                
            }
            return RedirectToAction("Index", "Home");
 
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(Dictionary<string, bool> check)
        {           
            foreach (var item in check)
            {
                try
                {
                    User user = await db.Users.FirstOrDefaultAsync(u => u.Email == item.Key);
                    user.IsBlocked = false;
                    db.SaveChanges();
                }
                catch (NullReferenceException)
                {
                }                
            }            
            return RedirectToAction("Index", "Home");               
        }

        public async Task<IActionResult> Logout()
        {           
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);            
            return RedirectToAction("Login", "Account");
        }
    }
}