using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samolut_Fintech_Application.Data;
using System.Linq; //this is for when using the db

namespace Samolut_Fintech_Application.Controllers
{
    public class AccountController : Controller
    {
        //need these two epices of code whenever db is used in controller
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }




        //same as every othewr view
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult LoginPassword()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }



        //the post for form like i would in php
        [HttpPost]
        public async Task<IActionResult> Login(string phonenumber, string password)
        {
            var user = await _context.Customer.FirstOrDefaultAsync(u => u.PHONE_NUMBER == phonenumber && u.PASSWORD == password);   //first or default is like fetch assoc in php and the u is c sharps like for i. Have to use async version.
            
            if (user == null) { 
                ViewBag.ErrorMessage = "Invalid phonenumber or password."; //this ViewBag is for how i would echo in php. ill style it later.
                return View();
            }

            //if users found
            HttpContext.Session.SetInt32("UserId", user.CUSTOMER_ID); //idk why it hgas to be 32
            HttpContext.Session.SetString("Name", user.FIRST_NAME);
            return RedirectToAction("ApplicationHome", "Application"); //so where its going and in which controller class

        }

        
    }
}
