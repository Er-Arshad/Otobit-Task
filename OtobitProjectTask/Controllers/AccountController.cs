
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Otobit.Models.Domain;
using OtobitProjectTask.ContextDb;
using OtobitProjectTask.Models;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace OtobitProjectTask.Controllers
{
    [Route("api/[Action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly NotificationHub hub;
        private static List<Notification> notifications = new List<Notification>();

        public AccountController(AppDbContext db, IConfiguration configuration, IHubContext<NotificationHub> hubContext,NotificationHub hub)
        {
            _db = db;
            _configuration = configuration;
            _hubContext = hubContext;
            this.hub = hub;
        }
        // [Authorize]
        [HttpPost]
        public IActionResult Signup(CustomersModel customers)
        {
            int minLength = 8;
            int maxLength = 20;
            string passpattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$%^&+=!])(?!.*\s).{" + minLength + "," + maxLength + "}$";
            // Use Regex.IsMatch to check if the password matches the pattern
            var IsValidPasswrod = Regex.IsMatch(customers.Password, passpattern);
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            // Use Regex.IsMatch to check if the email matches the pattern
            var IsEmail = Regex.IsMatch(customers.Email, pattern);
            if (!IsEmail) { return BadRequest("Not valid Email"); }
            if (!IsValidPasswrod) { return BadRequest("Password not valid : Password must:\n• Be 8-20 characters long\n• Contain at least one lowercase letter\n• Contain at least one uppercase letter\n• Contain at least one digit\n• Contain at least one special character (@, #, $, %, ^, &, +, =, !)\n• Not contain whitespace "); }
            Customers cust = new Customers();
            cust.UserName = customers.Email;
            cust.Password = customers.Password;
            _db.Cutomers.Add(cust);
            _db.SaveChanges();
            return Ok("User Registration successfull");
        }
        [HttpPost]
        public IActionResult Login(CustomersModel customers)
        {
            
            var user = _db.Cutomers.FirstOrDefault(s => s.UserName == customers.Email);
            if (user == null) { return NotFound("User Not Found"); }
            if (user.Password != customers.Password) { return BadRequest("UserName or Password Incorrect"); }

            var issuer = _configuration["JWT:ValidIssuer"];
            var audience = _configuration["JWT:ValidAudience"];
            var key = Encoding.ASCII.GetBytes
            (_configuration["JWT:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
             }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
           
            return Ok(jwtToken);
        }
        [HttpPost]
        public IActionResult MakeBooksOffer(int CustomerId, int BookId)
        {
            var customer = _db.Cutomers.FirstOrDefault(s => s.Id == CustomerId);
            if (customer == null) { return BadRequest("invalid customerId"); }
            var Books = _db.Books.FirstOrDefault(s => s.BookId == BookId);
            if (Books == null) { return BadRequest("Book is not available"); }
            var seller = _db.Sellers.FirstOrDefault(s => s.SellerId == Books.fk_Sellers);
            if (seller == null) { return BadRequest("seller is not available"); }
            CustomerRequest pb = new CustomerRequest();
            pb.SellerId = seller.SellerId;
            pb.CustomerId=customer.Id;
            pb.Message = $"{customer.UserName} maked an offer of {Books.Title} in {Books.Price}";
            pb.BookId = Books.BookId;
            pb.IsActive = true;
            _db.PurchesedBooks.Add(pb);
            _db.SaveChanges();
            //var responce = new PurchesResponce() { CustomerId = customer.Id, SellerId = seller.SellerId   };
            //Send notification to sellers
            return Ok("Offer maked successfully");
        }
        [HttpPost]
        public IActionResult AcceptOffer(int sellerId)
        {
            var seller = _db.Sellers.FirstOrDefault(s => s.SellerId == sellerId);
            seller.OfferAccepted = true;
            _db.SaveChanges();
            var offer  = _db.PurchesedBooks.FirstOrDefault(s=>s.SellerId==sellerId);
            if (offer == null) { return BadRequest("No purches for this seller"); }
            offer.IsActive = false;
            _db.SaveChanges();
            SellerResponce sr = new SellerResponce();
            sr.SellerId = seller.SellerId;
            sr.CustomerId = offer.CustomerId;
            sr.IsReceived = true;
            _db.OfferAccepted.Add(sr);
            _db.SaveChanges();
            //notifiy the customer
            return Ok("seller has accepted the offerc");
        }

        [HttpPost]
        public async Task<ActionResult> SendNotification(Notification notification)
        {
            if (notification == null)
                return BadRequest("Invalid notification.");

            notifications.Add(notification);

            // Send the notification to connected clients via SignalR
            //await _hubContext.Clients.All.SendMessageToClient("ReceiveNotification", notification.Message);
            await _hubContext.Clients.Client(notification.ConnectionId).SendAsync("ReceiveMessage", notification.Message);
            return Ok("Notification sent successfully.");
        }
        [HttpGet]
        public ActionResult<IEnumerable<Notification>> GetNotifications()
        {
            return Ok(notifications);
        }
   
    }

}

