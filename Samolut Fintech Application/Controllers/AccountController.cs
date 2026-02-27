using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samolut_Fintech_Application.Data;
using Samolut_Fintech_Application.Models.DatabaseModels;
using Samolut_Fintech_Application.Models.LoginSignUpModels;
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
        [ValidateAntiForgeryToken] //only using this on the posts

        public async Task<IActionResult> Login(LoginModel Data) //i use data as it uses customer table and whatever i posted is in Data to check against, and i used loginm modewl as thats what i want to check the data is valid against. like when i post my data in php
        {
            if (ModelState.IsValid)
            {

                var user = await _context.Customer.FirstOrDefaultAsync(i => i.PHONE_NUMBER == Data.PHONE_NUMBER && i.PASSWORD == Data.PASSWORD);   //first or default is like fetch assoc in php and the u is c sharps like for i. Have to use async version.

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Invalid phonenumber or password."; //this ViewBag is for how i would echo in php.
                    return View(Data); //puytting data back in the view means that the data will still be there once refresh
                }

                //if users found
                HttpContext.Session.SetInt32("UserId", user.CUSTOMER_ID); 
                HttpContext.Session.SetString("Name", user.FIRST_NAME);
                return RedirectToAction("ApplicationHome", "Application"); //so where its going and in which controller class
            }
            return View(Data); //if it doesnt pass the asp-validation stuff
        }
    
        
    }
}
