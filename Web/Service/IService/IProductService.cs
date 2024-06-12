using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IProductService
    {
        Task<ResponseDto?> GetProductAsync(string productName);
        Task<ResponseDto?> GetAllProductAsync();
        Task<ResponseDto?> GetProductByIdAsync(int Id);
        Task<ResponseDto?> CreateProductAsync(ProductDTO productDTO);
        Task<ResponseDto?> UpdateProductAsync(ProductDTO productDTO);
        Task<ResponseDto?> DeleteProductAsync(int Id);
    }
}
