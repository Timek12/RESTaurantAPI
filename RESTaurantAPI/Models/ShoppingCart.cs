using System.ComponentModel.DataAnnotations.Schema;

namespace RESTaurantAPI.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public double CartTotal { get; set; }
        [NotMapped]
        public ICollection<CartItem> CartItems { get; set; }
        [NotMapped]
        public string StripePaymentIntentId { get; set; }
        [NotMapped]
        public string ClientSecret { get; set; }
    }
}
