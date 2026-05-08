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

        public IActionResult SignUp()
        {
            return View();
        }



        //the post for form like i would in php
        [HttpPost]
        [ValidateAntiForgeryToken] //only using this on the posts

        public async Task<IActionResult> Login(LoginModel data) //i use data as it uses customer table and whatever i posted is in Data to check against, and i used loginm modewl as thats what i want to check the data is valid against. like when i post my data in php
        {
            if (ModelState.IsValid)
            {

                var user = await _context.Customer.FirstOrDefaultAsync(i => i.PHONE_NUMBER == data.PHONE_NUMBER && i.PASSWORD == data.PASSWORD);   //first or default is like fetch assoc in php and the u is c sharps like for i. Have to use async version.

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Invalid phonenumber or password."; //this ViewBag is for how i would echo in php.
                    return View(data); //puytting data back in the view means that the data will still be there once refresh
                }

                //if users found
                HttpContext.Session.SetInt32("UserId", user.CUSTOMER_ID); 
                HttpContext.Session.SetString("Name", user.FIRST_NAME);
                return RedirectToAction("ApplicationHome", "Application"); //so where its going and in which controller class
            }
            return View(data); //if it doesn't pass the asp-validation stuff
        }
        
        
        //post for signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(Customer data)
        {
            if (ModelState.IsValid)
            {
                var newCustomer = new Customer
                {
                    FIRST_NAME = data.FIRST_NAME,
                    MIDDLE_NAME = data.MIDDLE_NAME,
                    LAST_NAME = data.LAST_NAME,
                    PASSWORD = data.PASSWORD,
                    PHONE_NUMBER = data.PHONE_NUMBER
                };
            
                _context.Customer.Add(newCustomer);
                await _context.SaveChangesAsync();
            
            
                //make them a default gbp currency account.
                var gdpAccount = _context.CurrencyAccounts.Where(i => i.CURRENCY_ACCOUNT_ID == 1).FirstOrDefault();
                var defaultAccount = new Account
                {
                    CUSTOMER_ID = newCustomer.CUSTOMER_ID,
                    COUNTRY_CURRENCY_ID = gdpAccount.COUNTRY_CURRENCY_ID,
                    ACCOUNT_BALANCE = 0,
                    ACCOUNT_TYPE_ID = gdpAccount.ACCOUNT_TYPE_ID,
                    ACCOUNT_NAME = gdpAccount.ACCOUNT_NAME
                };
            
            
                _context.Account.Add(defaultAccount);
                await _context.SaveChangesAsync();
            
            
                //give them a little message to show them where to add account and add funds.
                ViewBag.WelcomeMessage = "Welcome to Samolut. To deposit money into this currency account connect a bank account in the Add Page and go to Connect Bank. To add more Currencies, go to the Add Page and go to Add Currency Account.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.ErrorMessage = "Please fix form errors!";
            return View();
        }
    
        
    }
}
