using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Otobit.Models.Domain;
using OtobitProjectTask.ContextDb;
using OtobitProjectTask.Models;
using System.Net;

namespace OtobitProjectTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _hubContext;

        public HomeController(AppDbContext db, IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
            _db = db;

        }
        // [HttpPost]
        //public IActionResult PurchBooks(int CustomerId,int BookId)
        //{
        //    var customer = _db.Cutomers.FirstOrDefault(s => s.Id == CustomerId);
        //    var Books = _db.Books.Where(s => s.BookId == BookId).ToList();
        //    var sellers = new List<Sellers>();
        //    foreach(var book in Books)
        //    {
        //        var seller =_db.Sellers.Where(s => s.SellerId == book.fk_Sellers).ToList();
        //        sellers.AddRange(seller);
        //    }

        //    //Send notification to sellers
        //    return Json(sellers ,customer.Id);
        //}
        //[HttpPost]
        //public IActionResult AcceptOffer(int sellerId,int customerId)
        //{
        //    var serller = _db.Sellers.FirstOrDefault(s => s.SellerId == sellerId);
        //    serller.OfferAccepted = true;
        //    _db.SaveChanges();
        //   //notifiy the customer

        //    return Json(serller);
        //}

        [HttpPost]
        public IActionResult PurchBooks(int customerId, int bookId)
        {
            try
            {
                var customer = _db.Cutomers.FirstOrDefault(s => s.Id == customerId);
                if (customer == null)
                {
                    return BadRequest("Invalid customerId");
                }

                var book = _db.Books.FirstOrDefault(s => s.BookId == bookId);
                if (book == null)
                {
                    return BadRequest("Book is not available");
                }

                var seller = _db.Sellers.FirstOrDefault(s => s.SellerId == book.fk_Sellers);
                if (seller == null)
                {
                    return BadRequest("Seller is not available");
                }

                // Send notification to sellers

                return Json(seller);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost]
        public IActionResult AcceptOffer(int sellerId, int customerId)
        {
            try
            {
                var seller = _db.Sellers.FirstOrDefault(s => s.SellerId == sellerId);
                if (seller == null)
                {
                    return BadRequest("Seller not found");
                }

                seller.OfferAccepted = true;
                _db.SaveChanges();

                // Notify the customer

                return Json(seller);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal Server Error");
            }
        }

        public IActionResult Index(string UserName)
        {
            if (UserName == null) { return RedirectToAction("Login"); }
            var customer = _db.Cutomers.FirstOrDefault(s => s.UserName == UserName);
            ViewBag.Customer = _db.Cutomers.Where(s=>s.UserName==UserName).ToList();
            ViewBag.Books = _db.Books.ToList();
            ViewBag.Check = false;
            ViewBag.AcceptedOffers = _db.OfferAccepted.Where(s=>s.IsReceived==true&& s.CustomerId==customer.Id) ;
            var AcceptedOffers = _db.OfferAccepted.FirstOrDefault(s => s.IsReceived == false && s.CustomerId == customer.Id);
            if(AcceptedOffers == null) { ViewBag.CheckOffer = false; }
            else
            {

            ViewBag.CheckOffer = AcceptedOffers.IsReceived==false?true:false;
            }
            return View();
        }
        [HttpPost]
         public IActionResult Index(MakeBookOfferModel offerModel)
        {
            var customer = _db.Cutomers.FirstOrDefault(s => s.Id == offerModel.CustomerId);
            if (customer == null) { return BadRequest("invalid customerId"); }
            var Books = _db.Books.FirstOrDefault(s => s.BookId == offerModel.BookId);
            if (Books == null) { return BadRequest("Book is not available"); }
            var seller = _db.Sellers.FirstOrDefault(s => s.SellerId == Books.fk_Sellers);
            if (seller == null) { return BadRequest("seller is not available"); }
            CustomerRequest pb = new CustomerRequest();
            pb.SellerId = seller.SellerId;
            pb.CustomerId = customer.Id;
            pb.Message = $"{customer.UserName} maked an offer of {Books.Title} in {Books.Price}";
            pb.BookId = Books.BookId;
            pb.IsActive = true;
            _db.PurchesedBooks.Add(pb);
            _db.SaveChanges();
            ViewBag.Customer = _db.Cutomers.Where(s => s.UserName == customer.UserName).ToList();
            ViewBag.Books = _db.Books.ToList();
            ViewBag.Check = true;
            ViewBag.Responce = $"{customer.UserName} maked an offer of {Books.Title} in {Books.Price}";
            ViewBag.CheckOffer = false;
            return View();
        }

        // In your controller
        public async Task<IActionResult> SendMessageToClient(string targetConnectionId, string message)
        {
            await _hubContext.Clients.Client(targetConnectionId).SendAsync("ReceiveMessage", message);
            return Json("Run");
        }
        [HttpGet]
        public IActionResult OfferAccepted(int sellerId)
        {
            var list = _db.PurchesedBooks.Where(s => s.SellerId == sellerId);
            if (list == null) { return BadRequest("No offer for you"); }
            var listView = new List<CustomerRequest>();
            foreach(var item in list)
            {
                listView.Add(new CustomerRequest() { Id = item.Id, Message = item.Message, CustomerId = item.CustomerId, SellerId = item.SellerId, BookId = item.BookId, IsActive = item.IsActive });
            }
            
            return View(listView);
        }
         [HttpGet]
        public IActionResult OfferAccepte(int Id)
        {
            var item =_db.PurchesedBooks.FirstOrDefault(s => s.Id == Id);
            item.IsActive = false;
            _db.SaveChanges();
            SellerResponce seller = new SellerResponce();
            seller.SellerId = item.SellerId;
            seller.CustomerId = item.CustomerId;
            seller.IsReceived = false;
            _db.OfferAccepted.Add(seller);
            _db.SaveChanges();
            var GetSeller = _db.Sellers.FirstOrDefault(s => s.SellerId == item.SellerId);
            GetSeller.OfferAccepted = true;
            _db.SaveChanges();
            
            return RedirectToAction("OfferAccepted");
        }
        [HttpGet]
        public IActionResult Login()
        {
          
            return View();
        }
        [HttpPost]
        public IActionResult ReceivedOffer(int Id)
        {
            var offers = _db.OfferAccepted.FirstOrDefault(s => s.CustomerId == Id);
            offers.IsReceived=true;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Login(LoginModel User)
        {
            if (User == null) return BadRequest("UserName & Password required");
          var customer =  _db.Cutomers.FirstOrDefault(s => s.UserName == User.UserName && s.Password == User.Password);
            if (customer == null) return BadRequest("UserName or Password is invalid");
           
            return RedirectToAction("Index", "Home", new { @UserName = customer.UserName });
        }
          [HttpGet]
        public IActionResult GetSeller()
        {
            var sellerIds = _db.PurchesedBooks.ToList().Select(s=>s.SellerId);
            var sellers = _db.Sellers.ToList().Where(s=>sellerIds.Contains(s.SellerId));
            return View(sellers);
        }

    }
}
