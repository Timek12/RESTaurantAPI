using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTaurantAPI.Data;
using RESTaurantAPI.Models;
using System.Net;

namespace RESTaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        public PaymentController(IConfiguration configuration, ApplicationDbContext db)
        {
            _configuration = configuration;
            _db = db;
            _response = new();
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId)
        {
            ShoppingCart? shoppingCart = await _db.ShoppingCarts
                .Include(u => u.CartItems).ThenInclude(u => u.MenuItem)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (shoppingCart == null || shoppingCart.CartItems is null || shoppingCart.CartItems.Count() == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            #region Create Payment Intent

            #endregion
            _response.Result = shoppingCart;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
    }
}
