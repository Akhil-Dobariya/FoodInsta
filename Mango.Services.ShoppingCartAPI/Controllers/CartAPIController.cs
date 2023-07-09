using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;

        public CartAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                CartDTO cart = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDTO>(_db.CartHeaders.First(t => t.UserId == userId))
                };
                cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDTO>>(_db.CartDetails.Where(t=>t.CartHeaderId==cart.CartHeader.CartHeaderId));

                foreach (var item in cart.CartDetails)
                {
                    cart.CartHeader.CartTotal += item.Count * item.Product.Price;
                }

                _response.Result=cart;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [HttpPost("Cartupsert")]
        public async Task<ResponseDto> CartUpsert(CartDTO cartDTO)
        {
            try
            {
                var cartHeaderFromDB = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(t => t.UserId == cartDTO.CartHeader.UserId);
                if (cartHeaderFromDB == null)
                {
                    //create header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDTO.CartHeader);
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();
                    cartDTO.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                {
                    //check if details has the same product
                    var cartDetailsFromDB = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(t => t.ProductId == cartDTO.CartDetails.First().ProductId && t.CartDetailsId == cartHeaderFromDB.CartHeaderId);

                    if (cartDetailsFromDB == null)
                    {
                        //create cartdetails
                        cartDTO.CartDetails.First().CartHeaderId = cartDetailsFromDB.CartHeaderId;
                        _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        //update count in cartdetails
                        cartDTO.CartDetails.First().Count += cartDetailsFromDB.Count;
                        cartDTO.CartDetails.First().CartHeaderId += cartDetailsFromDB.CartHeaderId;
                        cartDTO.CartDetails.First().CartDetailsId += cartDetailsFromDB.CartDetailsId;

                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                        await _db.SaveChangesAsync();

                    }
                }
                _response.Result = cartDTO;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost("Removecart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails = _db.CartDetails.First(t=>t.CartDetailsId == cartDetailsId);

                int totalCountOfCartItem = _db.CartDetails.Where(u=>u.CartHeaderId==cartDetails.CartHeaderId).Count();
                _db.CartDetails.Remove(cartDetails);

                if (totalCountOfCartItem==1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders.FirstOrDefaultAsync(t => t.CartHeaderId == cartDetails.CartHeaderId);
                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _db.SaveChangesAsync();

                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
