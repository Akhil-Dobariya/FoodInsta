namespace Mango.Services.EmailAPI.Models
{
    public class CartDTO
    {
        public CartHeaderDTO? CartHeader { get; set; }
        public IEnumerable<CartDetailsDTO>? CartDetails { get; set; }
    }
}
