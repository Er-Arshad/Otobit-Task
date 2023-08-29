using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OtobitProjectTask.Models
{
    public class Customers
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class CustomersModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
