
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
using System.Security.Cryptography;
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
        
        //main admin page for reviewing activity
        public async Task<IActionResult> Activity()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            
            // if admin take to admin page, always user id 1

            if (userId != 1)
            {
                return RedirectToAction("Login", "Account");
            }
            
            //get all suspicous transactions to display for admin
            var suspisiousTransactions = await _context.SuspiciousTransaction.ToListAsync();
             ViewBag.SuspisiousTransactions = suspisiousTransactions;
            
            return View();
            
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnSuspend(SuspiciousTransaction data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId != 1)
            {
                return RedirectToAction("Login", "Account");
            }
            
            //remalking the transaction and make it go through
            var suspendedTransaction = _context.SuspiciousTransaction.Where(i=>i.SUSPENDED_TRANSACTION_ID == data.SUSPENDED_TRANSACTION_ID).FirstOrDefault();
            var unSuspendedTransaction = new Transaction
            {
                SENDER_ACCOUNT_ID = suspendedTransaction.SENDER_ACCOUNT_ID,
                RECEIVER_ACCOUNT_ID = suspendedTransaction.RECEIVER_ACCOUNT_ID,
                AMOUNT = suspendedTransaction.AMOUNT,
                EXCHANGE_RATE = suspendedTransaction.EXCHANGE_RATE,
                START_CURRENCY = suspendedTransaction.START_CURRENCY,
                END_CURRENCY = suspendedTransaction.END_CURRENCY,
                TRANSACTION_TIME = DateTime.Now
            };
            
            _context.Transaction.Add(unSuspendedTransaction);
            
            //make balances change, taken friom confirm changes
            
            var SenderAccount = await _context.Account.Where(i=>i.ACCOUNT_ID == suspendedTransaction.SENDER_ACCOUNT_ID).FirstOrDefaultAsync();
            var ReceiverAccount = await _context.Account.Where(i=>i.ACCOUNT_ID == suspendedTransaction.RECEIVER_ACCOUNT_ID).FirstOrDefaultAsync();
            SenderAccount.ACCOUNT_BALANCE -= suspendedTransaction.ORIGINAL_AMOUNT; //so take away gbp and then below add jpy cause i accidentally subtracted thiusands from gbp
            ReceiverAccount.ACCOUNT_BALANCE += data.AMOUNT;
            
            
            //make user un suspended
            var accountID = suspendedTransaction.SENDER_ACCOUNT_ID;
            var customerID = await _context.SuspiciousTransaction
                .Include(i=>i.SenderAccountIdForeignKey)
                .Where(i=>i.SENDER_ACCOUNT_ID == accountID)
                .Select(i=>i.SenderAccountIdForeignKey.CUSTOMER_ID).FirstOrDefaultAsync();
            var customer = await _context.Customer
                .Where(i => i.CUSTOMER_ID == customerID).FirstOrDefaultAsync();
            customer.SUSPENDED = 0;
            
            _context.Customer.Update(customer);
            await _context.SaveChangesAsync();
            
            //delete suspicous transaction as it been added back to normla transaction
            _context.SuspiciousTransaction.Remove(suspendedTransaction);
            await _context.SaveChangesAsync();

            
            
            return RedirectToAction("Activity");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanAccount(SuspiciousTransaction data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId != 1)
            {
                return RedirectToAction("Login", "Account");
            }
            
            //ban customer from theere transaction
            var suspendedTransaction = _context.SuspiciousTransaction.Where(i=>i.SUSPENDED_TRANSACTION_ID == data.SUSPENDED_TRANSACTION_ID).FirstOrDefault();
            var accountID = suspendedTransaction.SENDER_ACCOUNT_ID;
            var customerID = await _context.SuspiciousTransaction
                .Include(i=>i.SenderAccountIdForeignKey)
                .Where(i=>i.SENDER_ACCOUNT_ID == accountID)
                .Select(i=>i.SenderAccountIdForeignKey.CUSTOMER_ID).FirstOrDefaultAsync();
            var customer = await _context.Customer
                .Where(i => i.CUSTOMER_ID == customerID).FirstOrDefaultAsync();
            
            
            customer.SUSPENDED = 2; //0 is fine 1 is suspoended a nd 2 baned
            _context.Customer.Update(customer);
            await _context.SaveChangesAsync();
            
            //delete suspicous transaction
            _context.SuspiciousTransaction.Remove(suspendedTransaction);
            await _context.SaveChangesAsync();
            
            
            return RedirectToAction("Activity");
        }


    }

    public class Application : Controller
    {

        private readonly ApplicationDbContext _context;

        public Application(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> BannedAccount()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            return View();
        }

        public IActionResult LogOut()
        {
            //log them out
            HttpContext.Session.Remove("UserId");
            
            //check
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
            
            // if admin take to admin page, always user id 1

            if (userId == 1)
            {
                return RedirectToAction("Activity", "Admin");
            }
            
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            }  if (suspended == 2)
            {
               return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            
            //start every account on a likle default gbp, this will be made on sign up.
            
            int DefaultAccount = 1;
            var DefaultAccountDetails = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Include(i=>i.AccountTypeIdForeignKey)
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.CurrencyIdForeignKey.COUNTRY_CURRENCY_ID == 1).FirstOrDefaultAsync(); //as gbp is always made first
            
            
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
                .Where(i => i.SENDER_ACCOUNT_ID == 1 || i.RECEIVER_ACCOUNT_ID == 1)
                .Where(i=>i.SenderAccountIdForeignKey.CUSTOMER_ID == userId || i.ReceiverAccountIdForeignKey.CUSTOMER_ID == userId).ToListAsync();
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
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
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
            int? userId = HttpContext.Session.GetInt32("UserId");
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
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
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            
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
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } 
            
            
            

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
                    endcurrencyID = ReciverCurrencyID,
                    SenderGBPRate
                    
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
        public async Task<IActionResult> ConfirmTransfer(double beforeAmount, int senderID, int receiverID, double exchangeRate, double currencyAmount, int startcurrencyID, int  endcurrencyID, double SenderGBPRate)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
                
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } 
            
            
            
            
            
            var data = new InternalCurrencyModel
            {
                ORIGINAL_AMOUNT = beforeAmount,
                SENDER_ACCOUNT_ID = senderID,
                RECEIVER_ACCOUNT_ID = receiverID,
                AMOUNT = currencyAmount,
                EXCHANGE_RATE = exchangeRate,
                START_CURRENCY = startcurrencyID,
                END_CURRENCY = endcurrencyID,
                SENDER_GBP_EXCHANGE_RATE = SenderGBPRate,
                
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
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } 
            
            
            
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
                
                var SenderCurrencyID = await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.ACCOUNT_ID == data.SENDER_ACCOUNT_ID)
                    .Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefaultAsync();
                
                var ReciverCurrencyID= await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.ACCOUNT_ID == data.RECEIVER_ACCOUNT_ID)
                    .Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefaultAsync();
                
                
                //check suspicion before going ahead
                
                //for the suspicion validation
                
                var GBPAmount = data.ORIGINAL_AMOUNT* data.SENDER_GBP_EXCHANGE_RATE; 
                var Customer = await _context.Customer
                    .Where(i => i.CUSTOMER_ID == userId)
                    .Select(i=>i.CUSTOMER_ID).FirstOrDefaultAsync();
                var ReceiversCustomer = await _context.Account
                    .Where(i => i.ACCOUNT_ID == data.RECEIVER_ACCOUNT_ID)
                    .Select(i => i.CUSTOMER_ID).FirstOrDefaultAsync();
                //so if its external then for suspicous check otherwise just go with transaction
                if (Customer != ReceiversCustomer)
                {
                    if (GBPAmount > 10000)// so suspicous if a single transaction is above 10000
                    {   
                        var suspicusCustomer = await _context.Customer
                            .Where(i => i.CUSTOMER_ID == userId).FirstOrDefaultAsync();
                        suspicusCustomer.SUSPENDED = 1;
                        _context.Customer.Update(suspicusCustomer);
                        
                        var suspendedData = new SuspiciousTransaction
                        {
                            SENDER_ACCOUNT_ID = data.SENDER_ACCOUNT_ID,
                            RECEIVER_ACCOUNT_ID = data.RECEIVER_ACCOUNT_ID,
                            AMOUNT = data.AMOUNT,
                            EXCHANGE_RATE = data.EXCHANGE_RATE,
                            START_CURRENCY = SenderCurrencyID,
                            END_CURRENCY = ReciverCurrencyID,
                            ORIGINAL_AMOUNT = data.ORIGINAL_AMOUNT,
                            TRANSACTION_TIME = DateTime.Now,
                            
                        };
                        _context.SuspiciousTransaction.Add(suspendedData);
                        await _context.SaveChangesAsync();
                        
                        return RedirectToAction("Suspension", "Application");
                            
                    } 
                }
                
                
                
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
                
                //as suspended van make withgdrawals, after the witdrawal transaction suspended must go back to banned page
                if (suspended == 2)
                {
                    return RedirectToAction("BannedAccount", "Application");
                } 
                
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
        
        
        
        //BANK---
        //ok so for bank transfer to currency account its same code as above but changed slightly for bank accounts
        
        public async Task<IActionResult> TransferBankToCurrency()
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            
            
            
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
        
        //EXTERNALLLL ----------
        
        public async Task<IActionResult> ChooseExternal()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            
            
            
            
            return View();
                
        }
        
        //post for choose page to make sure the account matches who they wanna send money too
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceiveExternalCustomer(ExternalCustomerModel data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                {
                return RedirectToAction("Login", "Account");
                }
            int? userId = HttpContext.Session.GetInt32("UserId");

            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            if (ModelState.IsValid)
            {   
                //see if theres matcvhing account
                var matchingAccountId = await _context.Customer
                    .Where(i => i.PHONE_NUMBER == data.PHONE_NUMBER)
                    .Select(i => i.CUSTOMER_ID).FirstOrDefaultAsync();

                if (matchingAccountId != 0)
                {
                    var matchingAccountFirstName = await _context.Customer
                        .Where(i => i.PHONE_NUMBER == data.PHONE_NUMBER)
                        .Select(i => i.FIRST_NAME).FirstOrDefaultAsync();
                    var matchingAccountLastName = await _context.Customer
                        .Where(i => i.PHONE_NUMBER == data.PHONE_NUMBER)
                        .Select(i => i.LAST_NAME).FirstOrDefaultAsync();

                    var matchingFullName = matchingAccountFirstName + " " + matchingAccountLastName;
                    
                    if (matchingFullName == data.FULL_NAME)
                    {
                        ViewBag.ConfirmMessage = "Account Found and Names Match!";
                        ViewBag.ExternalName = matchingFullName;
                        ViewBag.ExternalID = matchingAccountId;
                        
                    }
                    else
                    {
                        ViewBag.ConfirmMessage = "Account Found but Names don't Match. This accounts Full name is: " + matchingFullName + ". If correct continue with transaction, or try another phone number.";
                        ViewBag.ExternalName = matchingFullName;
                        ViewBag.ExternalID = matchingAccountId;

                    }
                    
                    
                    
                }
                else
                {
                    ViewBag.ConfirmMessage = "Account Not Found.";
                }
                
            

            }
            

            return View("ChooseExternal", data);
            

        }
        
        public async Task<IActionResult> TransferToExternal(ExternalTransferModel data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            
            var accounts = await _context.Account
                .Include(i => i.CurrencyIdForeignKey) //added a  foreign key in my db, so i can read off trhe currency names as i made it to be 3nf so its in seperate table
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
            
            ViewBag.accounts = accounts;

            int externalAccountId = data.CUSTOMER_ID;
            
            var recipientsAccounts = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.CUSTOMER_ID == externalAccountId).ToListAsync();
                //.Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync(); //was only currency accounts but the project brief says it should be both
            
            ViewBag.recipientsAccounts =  recipientsAccounts;
            ViewBag.externalName = data.FULL_NAME;
                
            return View(data);
                
        }
        
        
        
        //All Addd money page stuff -------------------------
        public async Task<IActionResult> Add()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");

            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            

            return View();
        }
        
        
        public async Task<IActionResult> ConnectBank()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            

            var bankAccounts = await _context.BankAccounts
                .Include(i => i.CurrencyIdForeignKey)
                .ToListAsync();
            
            var banksAlreadyGot = await _context.Account
                .Where(i=>i.ACCOUNT_TYPE_ID == 2)
                .Where(i=>i.CUSTOMER_ID == userId)
                .Select(i=>i.ACCOUNT_NAME).ToListAsync();

            //remove all if in banks already got
            bankAccounts.RemoveAll(i => banksAlreadyGot.Contains(i.ACCOUNT_NAME));
            ViewBag.bankAccounts = bankAccounts;
            
            return View();
        }
        
        //post for connect bank, so to actuall connect chosen bank
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectBank(AddBankModel data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = HttpContext.Session.GetInt32("UserId").Value; //used . value to get exact value so i can put in sql
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            
            
            //depending on what bank ill choose how much mioney is in
            // ill make it random but banks differ
            
            //llods is 0-10000, ill make nationwide 10000-50000, and HSBC Private Account 250,000 to 500,000
            if (ModelState.IsValid)
            {
                double bankBalance = 0;
                if (data.BANK_ID == 1)
                {
                    //lloyds 0-10000
                    bankBalance = RandomNumberGenerator.GetInt32(0, 10000);
                }
                else if (data.BANK_ID == 2)
                {
                    //nationwide 10000-50000
                    bankBalance = RandomNumberGenerator.GetInt32(10000, 50000);
                }
                else if (data.BANK_ID == 3)
                {
                    //HSBC private 250,000 to 500,000
                    bankBalance = RandomNumberGenerator.GetInt32(250000, 500000);
                }

                //get bank details
                var bankSelected = await _context.BankAccounts
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.BANK_ID == data.BANK_ID).FirstOrDefaultAsync();

                //add posted bank to there accounts page

                var addBankAccount = new Account
                {
                    CUSTOMER_ID = userId,
                    COUNTRY_CURRENCY_ID = bankSelected.COUNTRY_CURRENCY_ID,
                    ACCOUNT_BALANCE = bankBalance,
                    ACCOUNT_TYPE_ID = bankSelected.ACCOUNT_TYPE_ID,
                    ACCOUNT_NAME = bankSelected.ACCOUNT_NAME
                };
                _context.Add(addBankAccount);
                await _context.SaveChangesAsync();
                ViewBag.ErrorMessage = "Success!";
                
                return RedirectToAction("ApplicationHome");
            }
            var bankAccounts = await _context.BankAccounts
                .Include(i => i.CurrencyIdForeignKey)
                
                .ToListAsync();
            ViewBag.bankAccounts = bankAccounts;
            ViewBag.ErrorMessage = "Invalid Input!";
            return View();
        }
        
        //basicaly same code as the add bank
        public async Task<IActionResult> AddCurrency()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");

            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            //exact same as add bank but for add currency and the currencies thy already have
            var currencyAccounts = await _context.CurrencyAccounts
                .Include(i => i.CurrencyIdForeignKey)
                .ToListAsync();
            
            var currenciesAlreadyGot = await _context.Account
                .Where(i=>i.ACCOUNT_TYPE_ID == 1) //1 fir all currency accounts not bansk
                .Where(i=>i.CUSTOMER_ID == userId)
                .Select(i=>i.ACCOUNT_NAME).ToListAsync();

            currencyAccounts.RemoveAll(i => currenciesAlreadyGot.Contains(i.ACCOUNT_NAME));
            ViewBag.currencyAccounts = currencyAccounts;
            
            return View();
        }
        
         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCurrency(AddCurrencyModel data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = HttpContext.Session.GetInt32("UserId").Value; //used . value to get exact value so i can put in sql
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } else if (suspended == 2)
            {
                return RedirectToAction("BannedAccount", "Application");
            }
            
            
            
            
            //same code to add currency to bank without adding a balance.
            if (ModelState.IsValid)
            {
                double bankBalance = 0;

                //get bank details
                var currencyAccountSelected = await _context.CurrencyAccounts
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.CURRENCY_ACCOUNT_ID == data.CURRENCY_ACCOUNT_ID).FirstOrDefaultAsync();

                //add posted bank to there accounts page

                var addBankAccount = new Account
                {
                    CUSTOMER_ID = userId,
                    COUNTRY_CURRENCY_ID = currencyAccountSelected.COUNTRY_CURRENCY_ID,
                    ACCOUNT_BALANCE = bankBalance,
                    ACCOUNT_TYPE_ID = currencyAccountSelected.ACCOUNT_TYPE_ID,
                    ACCOUNT_NAME = currencyAccountSelected.ACCOUNT_NAME
                };
                _context.Add(addBankAccount);
                await _context.SaveChangesAsync();
                ViewBag.ErrorMessage = "Success!";
                
                return RedirectToAction("ApplicationHome");
            }
            var bankAccounts = await _context.BankAccounts
                .Include(i => i.CurrencyIdForeignKey)
                
                .ToListAsync();
            ViewBag.bankAccounts = bankAccounts;
            ViewBag.ErrorMessage = "Invalid Input!";
            return View();
        }



        public async Task<IActionResult> Suspension()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //some details of the suspention
            var suspentionData = await _context.SuspiciousTransaction
                .Include(i=>i.SenderAccountIdForeignKey)
                .Where(i => i.SenderAccountIdForeignKey.CUSTOMER_ID == userId).FirstOrDefaultAsync();
            var reciverID = await _context.SuspiciousTransaction
                .Include(i => i.ReceiverAccountIdForeignKey)
                .Where(i => i.SUSPENDED_TRANSACTION_ID == suspentionData.SUSPENDED_TRANSACTION_ID)
                .Where(i => i.RECEIVER_ACCOUNT_ID == suspentionData.RECEIVER_ACCOUNT_ID)
                .Select(i => i.SenderAccountIdForeignKey.CUSTOMER_ID).FirstOrDefaultAsync();
            var reciver = await  _context.Customer.Where(i => i.CUSTOMER_ID == userId).FirstOrDefaultAsync();
            var receiverName = reciver.FIRST_NAME + " " +reciver.LAST_NAME;
            
            ViewBag.Reason =  suspentionData.SUSPENDED_TRANSACTION_REASON;
            ViewBag.Time = suspentionData.TRANSACTION_TIME;
            ViewBag.Amount = suspentionData.AMOUNT;
            ViewBag.endCurrency =  suspentionData.END_CURRENCY;
            ViewBag.sentTo = receiverName;
            
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspension(suspendedReasonModel data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //stuff from before post
            var suspentionData = await _context.SuspiciousTransaction
                .Include(i=>i.SenderAccountIdForeignKey)
                .Where(i => i.SenderAccountIdForeignKey.CUSTOMER_ID == userId).FirstOrDefaultAsync();
            var reciverID = await _context.SuspiciousTransaction
                .Include(i => i.ReceiverAccountIdForeignKey)
                .Where(i => i.SUSPENDED_TRANSACTION_ID == suspentionData.SUSPENDED_TRANSACTION_ID)
                .Where(i => i.RECEIVER_ACCOUNT_ID == suspentionData.RECEIVER_ACCOUNT_ID)
                .Select(i => i.SenderAccountIdForeignKey.CUSTOMER_ID).FirstOrDefaultAsync();
            var reciver = await  _context.Customer.Where(i => i.CUSTOMER_ID == userId).FirstOrDefaultAsync();
            var receiverName = reciver.FIRST_NAME + " " +reciver.LAST_NAME;
            
            ViewBag.Reason =  suspentionData.SUSPENDED_TRANSACTION_REASON;
            ViewBag.Time = suspentionData.TRANSACTION_TIME;
            ViewBag.Amount = suspentionData.AMOUNT;
            ViewBag.endCurrency =  suspentionData.END_CURRENCY;
            ViewBag.sentTo = receiverName;
            // -------------------------
            
            
            var reason = data.SUSPENDED_TRANSACTION_REASON;
            
            suspentionData.SUSPENDED_TRANSACTION_REASON = reason;
            _context.SuspiciousTransaction.Update(suspentionData);
            await _context.SaveChangesAsync();
            
            return View();
        }
        
        public async Task<IActionResult> TransferWithdrawToBank()
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            
            //check if accounts suspended or banned on every page
            var suspended = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i=>i.SUSPENDED).FirstOrDefaultAsync();
            if (suspended == 1)
            {
                return RedirectToAction("Suspension", "Application");
            } 
            
            //can be accessed by people withdrawing funds.
            
            
            
            
            var countryCurrencies = await _context.CurrentCurrency.ToListAsync();


            var accounts = await _context.Account
                .Include(i =>
                    i.CurrencyIdForeignKey) //added a  foreign key in my db, so i can read off trhe currency names as i made it to be 3nf so its in seperate table
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 1).ToListAsync();
            
            //add bank acounts
            var bankAccounts = await _context.Account
                .Include(i =>
                    i.CurrencyIdForeignKey) //added a  foreign key in my db, so i can read off trhe currency names as i made it to be 3nf so its in seperate table
                .Where(i => i.CUSTOMER_ID == userId)
                .Where(i=>i.ACCOUNT_TYPE_ID == 2).ToListAsync();

            ViewBag.accounts = accounts;
            ViewBag.bankAccounts = bankAccounts;
            
            return View();
        }
        
        

    }
    
    
    
}    
    