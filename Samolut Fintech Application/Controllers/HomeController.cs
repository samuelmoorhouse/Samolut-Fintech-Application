using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using MySqlConnector;
using Samolut_Fintech_Application.Data;
using Samolut_Fintech_Application.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Samolut_Fintech_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        
        
        public IActionResult About()
        {
            return View();
        }

        //login stuff

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    //so im making a new controller basically new folder for all my admin files. using the same code above but from my admin folder
    public class AdminController : Controller
    {
        //this needs to be at the top of every file
        private readonly ApplicationDbContext _context;

        //setting up the contellers stuff
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        //to view the customers
        public async Task<IActionResult> ViewCustomers()
        {
            var customers = await _context.Customer.ToListAsync();
            return View(customers);
        }

    }

    public class Application : Controller
    {

        private readonly ApplicationDbContext _context;
        public Application(ApplicationDbContext context)
        {
            _context = context;
        }


        
        public async Task<IActionResult> ApplicationHome(int? selectedAccountId) //the selcted account id is from the name of select html statemt, so i know hat accounts in use and when
        {
            //checks session on every application file
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            

            //so my user id from the session
            int? userId = HttpContext.Session.GetInt32("UserId");
            //get list of accounts for dropdown
            var accounts = await _context.Account
                .Where(i => i.CUSTOMER_ID == userId)
                .ToListAsync();

            //make list if Currencies as in my db i made it so the accounts like currency name like GBP was in a seperate table so its 3nf
            var currencies = await _context.CurrentCurrency.ToListAsync();
            //find the balance that matches the selected account in the html select tag
            var balance = accounts.Where(i=> i.ACCOUNT_ID == selectedAccountId).Select(i=> i.ACCOUNT_BALANCE).FirstOrDefault();

            //need currency icon so it looks cool
            //i need in currencies table  where currency id on the slected account and select icon.
            var currencyId = accounts.Where(i => i.ACCOUNT_ID == selectedAccountId).Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefault();
            var currencyIcon = currencies.Where(i => i.COUNTRY_CURRENCY_ID == currencyId).Select(i => i.CURRENCY_ICON).FirstOrDefault();

            


            ViewBag.Currencies = currencies;
            ViewBag.balance = balance; //puts the id thats in the html select tag in viewbag so i can change balance and transactions and stuff
            ViewBag.currencyIcon = currencyIcon;
            ViewBag.selected = selectedAccountId;

            return View(accounts);
        }

        public async Task<IActionResult> Account()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();

        }

        public IActionResult Payments()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }
        public IActionResult Add()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }


    }
}
