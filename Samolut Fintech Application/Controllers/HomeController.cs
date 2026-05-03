
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using MySqlConnector;
using Samolut_Fintech_Application.Data;
using Samolut_Fintech_Application.Models;
using Samolut_Fintech_Application.Models.DatabaseModels;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionTranslators.Internal;
using Samolut_Fintech_Application.Models.Transfers;

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


        public IActionResult Add()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }
        
        //all home page stuff

        public async Task<IActionResult> ApplicationHome()
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //start every account on a likle default gbp
            int DefaultAccount = 1;
            var DefaultAccountDetails = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Include(i=>i.AccountTypeIdForeignKey)
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_ID == 1).FirstOrDefaultAsync();
            
            
            //default account stuff
            ViewBag.ACCOUNT_ID = DefaultAccountDetails.ACCOUNT_ID;
            ViewBag.COUNTRY_CURRENCY_NAME = DefaultAccountDetails.CurrencyIdForeignKey.COUNTRY_CURRENCY_NAME;
            ViewBag.COUNTRY_CURRENCY_ID = DefaultAccountDetails.COUNTRY_CURRENCY_ID;
            ViewBag.ACCOUNT_BALANCE = DefaultAccountDetails.ACCOUNT_BALANCE;
            ViewBag.ACCOUNT_TYPE_ID = DefaultAccountDetails.ACCOUNT_TYPE_ID;
            
            var customerName = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i => i.FIRST_NAME).FirstOrDefaultAsync();
            ViewBag.CustomerName = customerName;
            
            
            //for filling out the form
            var accounts = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
            
            var defaultSelectedTransactions = await _context.Transaction
                .Include(i =>
                    i.SenderAccountIdForeignKey) //include the foreign keys so i know the details of each accoujnt
                .Include(i => i.ReceiverAccountIdForeignKey)
                .Where(i => i.SENDER_ACCOUNT_ID == 1 || i.RECEIVER_ACCOUNT_ID == 1).ToListAsync();
                //the || means or

            ViewBag.defaultSelectedTransactions = defaultSelectedTransactions;

            return View(accounts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplicationHome(Account data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
            return RedirectToAction("Login", "Account");
            }

            return View();
        }
        
        
        
    
        
        
        //all payments page stuff -----------------------------------------------------------
        public async Task<IActionResult> Payments()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        
        public async Task<IActionResult> TransferInternalCurrency()
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            var countryCurrencies = await _context.CurrentCurrency.ToListAsync();


            var accounts = await _context.Account
                .Include(i =>
                    i.CurrencyIdForeignKey) //added a  foreign key in my db, so i can read off trhe currency names as i made it to be 3nf so its in seperate table
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();

            ViewBag.accounts = accounts;
            //post hasnt happened yet so make it false when page first loads
            
            return View();
        }
        
        //so this is for the initial calculation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalculateCurrency(InternalCurrencyModel data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (ModelState.IsValid)
            {
                //validation check before calculation, will do same in next post
                //so im gonna check if the accounts were the same first
                if (data.SENDER_ACCOUNT_ID == data.RECEIVER_ACCOUNT_ID)
                {
                    ViewBag.accounts = await _context.Account
                        .Include(i => i.CurrencyIdForeignKey)
                        .Where(i => i.CUSTOMER_ID == userId)
                        .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
                    ViewBag.ErrorMessage = "Cannot send money to the same account! Choose a different account to receive funds.";
                    return View("TransferInternalCurrency", data);
                }
                
                //check if they have enough money by comparing to balance
                var SenderBalance = await _context.Account
                    .Where(i => i.ACCOUNT_ID == data.SENDER_ACCOUNT_ID)
                    .Select(i => i.ACCOUNT_BALANCE).FirstOrDefaultAsync();


                if (SenderBalance < data.AMOUNT) //so not enough moneys
                {
                    ViewBag.accounts = await _context.Account
                        .Include(i => i.CurrencyIdForeignKey)
                        .Where(i => i.CUSTOMER_ID == userId)
                        .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
                    ViewBag.ErrorMessage = "Insufficient Funds!";
                    return View("TransferInternalCurrency", data);
                }
                
                //calculate exchange rate, so get sender and receiver and calculate it against the uks one
                //ill store all the rates to gbp
                // formula is sender gbp rate / receiver gbp rate
                var SenderCurrencyID = await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.ACCOUNT_ID == data.SENDER_ACCOUNT_ID)
                    .Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefaultAsync();
                
                var ReciverCurrencyID= await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.ACCOUNT_ID == data.RECEIVER_ACCOUNT_ID)
                    .Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefaultAsync();

                double SenderGBPRate = 0;
                double ReceiverGBPRate = 0;
                //though id use switch as its less code for this
                switch(SenderCurrencyID)
                {
                    case 1: //gbp so no convert needed
                        SenderGBPRate = 1;
                        break;
                    case 2: //eur to gbp
                        SenderGBPRate = 0.8624;
                        break;
                    case 3:
                        SenderGBPRate = 0.0046;
                        break;
                }

                switch (ReciverCurrencyID)
                {
                    case 1: //gbp so no convert needed
                        ReceiverGBPRate = 1;
                        break;
                    case 2: //eur to gbp
                        ReceiverGBPRate = 0.8624;
                        break;
                    case 3:
                        ReceiverGBPRate = 0.0046;
                        break;
                }

                double ExchangeRate = SenderGBPRate / ReceiverGBPRate;
                
                double NewCurrencyAmount = Math.Round((data.AMOUNT * ExchangeRate),2); //so to 2dp
                
                
                
                return RedirectToAction("ConfirmTransfer", new
                {
                    senderID = data.SENDER_ACCOUNT_ID,
                    receiverID = data.RECEIVER_ACCOUNT_ID,
                    beforeAmount = data.AMOUNT,
                    currencyAmount = NewCurrencyAmount, //new amount just made
                    exchangeRate = ExchangeRate,
                    startcurrencyID = SenderCurrencyID,
                    endcurrencyID = ReciverCurrencyID
                    
                });
                
            }

            //if not need to relaoad page with eveythign it had before, i use this code below allot abovewhenever invalid data
            ViewBag.accounts = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
            ViewBag.ErrorMessage = "Model Invalid";
            return View("TransferInternalCurrency", data); //becuase my post is named differently to the file i have to tell it where to go back to
        }
        
        
        //this is for after calculation and to confirm before transaction
        public async Task<IActionResult> ConfirmTransfer(double beforeAmount, int senderID, int receiverID, double exchangeRate, double currencyAmount, int startcurrencyID, int  endcurrencyID)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
                
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            var data = new InternalCurrencyModel
            {
                ORIGINAL_AMOUNT = beforeAmount,
                SENDER_ACCOUNT_ID = senderID,
                RECEIVER_ACCOUNT_ID = receiverID,
                AMOUNT = currencyAmount,
                EXCHANGE_RATE = exchangeRate,
                START_CURRENCY = startcurrencyID,
                END_CURRENCY = endcurrencyID
                
            };
            
            
            //just things to make page look good, like getting the pound signs from db
            ViewBag.SenderAccountSymbol = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.ACCOUNT_ID == senderID)
                .Select(i => i.CurrencyIdForeignKey.CURRENCY_ICON).FirstOrDefaultAsync();
            ViewBag.ReceiverAccountSymbol = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.ACCOUNT_ID == receiverID)
                .Select(i => i.CurrencyIdForeignKey.CURRENCY_ICON).FirstOrDefaultAsync();
            //and the names aswell cause why not
            ViewBag.SenderAccountName = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.ACCOUNT_ID == senderID)
                .Select(i => i.CurrencyIdForeignKey.COUNTRY_CURRENCY_NAME).FirstOrDefaultAsync();
            ViewBag.ReceiverAccountName = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.ACCOUNT_ID == receiverID)
                .Select(i => i.CurrencyIdForeignKey.COUNTRY_CURRENCY_NAME).FirstOrDefaultAsync();
            
            return View(data);
        }
        
        //post to finally go through with transaction from the confirm of the thing above
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransfer(InternalCurrencyModel data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
                
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //same validation as above for initial calculation
            if (ModelState.IsValid)
            {
                //validation check before calculation, will do same in next post
                //so im gonna check if the accounts were the same first
                if (data.SENDER_ACCOUNT_ID == data.RECEIVER_ACCOUNT_ID)
                {
                    ViewBag.accounts = await _context.Account
                        .Include(i => i.CurrencyIdForeignKey)
                        .Where(i => i.CUSTOMER_ID == userId)
                        .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
                    ViewBag.ErrorMessage = "Cannot send money to the same account! Choose a different account to receive funds.";
                    return View("TransferInternalCurrency", data);
                }
                
                //check if they have enough money by comparing to balance
                var SenderBalance = await _context.Account
                    .Where(i => i.ACCOUNT_ID == data.SENDER_ACCOUNT_ID)
                    .Select(i => i.ACCOUNT_BALANCE).FirstOrDefaultAsync();

                if (SenderBalance < data.ORIGINAL_AMOUNT) //so not enough moneys
                {
                    ViewBag.accounts = await _context.Account
                        .Include(i => i.CurrencyIdForeignKey)
                        .Where(i => i.CUSTOMER_ID == userId)
                        .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
                    ViewBag.bankAccounts = await _context.Account
                        .Include(i => i.CurrencyIdForeignKey)
                        .Where(i => i.CUSTOMER_ID == userId)
                        .Where(i=>i.ACCOUNT_TYPE_ID == 2).ToListAsync();
                    ViewBag.ErrorMessage = "Insufficient Funds!"+SenderBalance;
                    return View("TransferInternalCurrency", data);
                }
                
                //calculate exchange rate, so get sender and receiver and calculate it against the uks one
                //ill store all the rates to gbp
                // formula is sender gbp rate / receiver gbp rate
                var SenderCurrencyID = await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.ACCOUNT_ID == data.SENDER_ACCOUNT_ID)
                    .Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefaultAsync();
                
                var ReciverCurrencyID= await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.ACCOUNT_ID == data.RECEIVER_ACCOUNT_ID)
                    .Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefaultAsync();
                
                //changing accounts balance then addin a trnsaction to table
                
                var SenderAccount = await _context.Account.Where(i=>i.ACCOUNT_ID == data.SENDER_ACCOUNT_ID).FirstOrDefaultAsync();
                var ReceiverAccount = await _context.Account.Where(i=>i.ACCOUNT_ID == data.RECEIVER_ACCOUNT_ID).FirstOrDefaultAsync();
                SenderAccount.ACCOUNT_BALANCE -= data.ORIGINAL_AMOUNT; //so take away gbp and then below add jpy cause i accidentally subtracted thiusands from gbp
                ReceiverAccount.ACCOUNT_BALANCE += data.AMOUNT;
                //so transaction
                var TransactionData = new Transaction
                {
                    SENDER_ACCOUNT_ID = data.SENDER_ACCOUNT_ID,
                    RECEIVER_ACCOUNT_ID = data.RECEIVER_ACCOUNT_ID,
                    AMOUNT = data.AMOUNT,
                    EXCHANGE_RATE = data.EXCHANGE_RATE,
                    START_CURRENCY = SenderCurrencyID,
                    END_CURRENCY = ReciverCurrencyID,
                    TRANSACTION_TIME = DateTime.Now,
                };
                
                _context.Transaction.Add(TransactionData);
                await _context.SaveChangesAsync();
                
                
                ViewBag.accounts = await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.CUSTOMER_ID == userId || i.ACCOUNT_TYPE_ID == 1).ToListAsync();

                ViewBag.ErrorMessage = "Success!";
                
                return View(data);
            }

            //if not need to relaoad page with eveythign it had before, i use this code below allot abovewhenever invalid data
            ViewBag.accounts = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.CUSTOMER_ID == userId || i.ACCOUNT_TYPE_ID == 1).ToListAsync();
            ViewBag.bankAccounts = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 2).ToListAsync();
            ViewBag.ErrorMessage = "Model Invalid";
            return View(data);
            
        }
        
        
        
        //BANK---------------------------------------------------------------------
        //ok so for bank transfer to currency account its same code as above but changed slightly for bank accounts
        
        public async Task<IActionResult> TransferBankToCurrency()
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            var countryCurrencies = await _context.CurrentCurrency.ToListAsync();


            var accounts = await _context.Account
                .Include(i =>
                    i.CurrencyIdForeignKey) //added a  foreign key in my db, so i can read off trhe currency names as i made it to be 3nf so its in seperate table
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
            
            //added bank accounts
            var bankAccounts = await _context.Account
                .Include(i =>
                    i.CurrencyIdForeignKey) //added a  foreign key in my db, so i can read off trhe currency names as i made it to be 3nf so its in seperate table
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 2).ToListAsync();
            
            ViewBag.bankAccounts = bankAccounts;
            ViewBag.accounts = accounts;
            //post hasnt happened yet so make it false when page first loads
            
            return View();
        }
        
        
        
    }
    
    
    
}    
    