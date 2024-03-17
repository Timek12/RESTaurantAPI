using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTaurantAPI.Data;
using RESTaurantAPI.Models;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace RESTaurantAPI.Controllers
{
    [Route("api/ShoppingCart")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly ApplicationDbContext _db;
        public ShoppingCartController(ApplicationDbContext db)
        {
            _response = new();
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
        {
            try
            {
                ShoppingCart shoppingCart;
                if (string.IsNullOrEmpty(userId))
                {
                    shoppingCart = new();
                }
                else
                {
                    shoppingCart = await _db.ShoppingCarts
                    .Include(u => u.CartItems)
                    .ThenInclude(u => u.MenuItem)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                    if (shoppingCart is null)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        return BadRequest(_response);
                    }
                }

                if (shoppingCart.CartItems is not null && shoppingCart.CartItems.Count > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);
                }

                _response.Result = shoppingCart;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(shoppingCart);

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Errors.Add(ex.Message);
            }

            return _response;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {
            ShoppingCart? shoppingCart = _db.ShoppingCarts
                .Include(u => u.CartItems)
                .FirstOrDefault(u => u.UserId == userId);

            MenuItem? menuItem = _db.MenuItems.FirstOrDefault(u => u.Id == menuItemId);

            if (menuItem is null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            if(shoppingCart is null && updateQuantityBy > 0)
            {
                // create a shopping cart & add cart item
                ShoppingCart newCart = new()
                {
                    UserId = userId,
                };

                _db.ShoppingCarts.Add(newCart);
                await _db.SaveChangesAsync();

                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null,
                };

                _db.CartItems.Add(newCartItem);
                await _db.SaveChangesAsync();
            }
            else
            {
                // shopping cart exists, only add cart item
                CartItem? cartItemInCart = shoppingCart.CartItems
                    .FirstOrDefault(u => u.MenuItemId == menuItemId);

                if (cartItemInCart is null)
                {
                    // item does not exists in current cart
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null,
                    };

                    _db.CartItems.Add(newCartItem);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // item already exists in the cart, only update quantity
                    int newQuantity = cartItemInCart.Quantity + updateQuantityBy;
                    if(updateQuantityBy == 0 || newQuantity <= 0)
                    {
                        // remove cart item from cart and if only item remove cart
                        _db.CartItems.Remove(cartItemInCart);
                        if(shoppingCart.CartItems.Count() == 1)
                        {
                            _db.ShoppingCarts.Remove(shoppingCart);
                        }
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        cartItemInCart.Quantity = newQuantity;
                        await _db.SaveChangesAsync();
                    }
                }

            }
            return Ok(_response);
        }

        
    }
}
