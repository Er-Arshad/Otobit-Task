using System.ComponentModel.DataAnnotations;

namespace OtobitProjectTask.Models
{
    public class CustomerRequest
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public int SellerId { get; set; }
        public int BookId { get; set; }
        public bool IsActive { get; set; }
    }
}
