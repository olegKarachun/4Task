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
                    ModelState.AddModelError("", "Incorrect name or(and) email");
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
                    ModelState.AddModelError("", "You are blocked");
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
                    ModelState.AddModelError("", "Enter correct Email");
                }
                else if (user == null && model.Name == model.Password)
                {
                    ModelState.AddModelError("", "Name and password must not match");
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
                    ModelState.AddModelError("", "Incorrect name or(and) email");
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

        public async Task action(string typeOfAction, Dictionary<string, bool> check)
        {
            foreach (var item in check)
            {
                try
                {
                    User user = await db.Users.FirstOrDefaultAsync(u => u.Email == item.Key);
                    if (typeOfAction == "Delete"){
                        db.Users.Remove(user);
                    }else if (typeOfAction == "Block"){
                        user.IsBlocked = true;
                    }else if (typeOfAction == "Unblock"){
                        user.IsBlocked = false;
                    }
                    await db.SaveChangesAsync();
                }
                catch
                {
                }
            }                                 
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Dictionary<string, bool> check)
        {
            await action("Delete", check);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Block(Dictionary<string, bool> check)
        {
            await action("Block", check);
            return RedirectToAction("Index", "Home"); 
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(Dictionary<string, bool> check)
        {
            await action("Unblock", check);
            return RedirectToAction("Index", "Home");               
        }

        public async Task<IActionResult> Logout()
        {           
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);            
            return RedirectToAction("Login", "Account");
        }
    }
}