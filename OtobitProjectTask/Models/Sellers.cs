using System.ComponentModel.DataAnnotations;

namespace Otobit.Models.Domain
{
    public class Sellers
    {
        [Key]
        public int SellerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }
        public bool OfferAccepted { get; set; }
    }
}
