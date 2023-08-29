using System.ComponentModel.DataAnnotations;

namespace OtobitProjectTask.Models
{
    public class SellerResponce
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int SellerId { get; set; }
        public bool IsReceived { get; set; }
    }
}
