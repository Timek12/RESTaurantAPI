using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTaurantAPI.Data;
using RESTaurantAPI.Models;
using RESTaurantAPI.Models.Dto;
using RESTaurantAPI.Utility;
using System.Net;

namespace RESTaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
            _response = new();
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId)
        {
            try
            {
                var orderHeaders = _db.OrderHeaders.Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem)
                    .OrderByDescending(u => u.OrderHeaderId);

                if (!string.IsNullOrEmpty(userId))
                {
                    _response.Result = orderHeaders.Where(u => u.ApplicationUserId == userId);
                }
                else
                {
                    _response.Result = orderHeaders;
                }

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors.Add(ex.Message);
            }

            return _response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrder(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var orderHeader = await _db.OrderHeaders.Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem)
                    .FirstOrDefaultAsync(u => u.OrderHeaderId == id);

                if (orderHeader is null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = orderHeader;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors.Add(ex.Message);
            }

            return _response;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeaderDTO)
        {
            try
            {
                OrderHeader order = new()
                {
                    ApplicationUserId = orderHeaderDTO.ApplicationUserId,
                    PickupEmail = orderHeaderDTO.PickupEmail,
                    PickupName = orderHeaderDTO.PickupName,
                    PickupPhoneNumber = orderHeaderDTO.PickupPhoneNumber,
                    OrderTotal = orderHeaderDTO.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentID = orderHeaderDTO.StripePaymentIntentID,
                    TotalItems = orderHeaderDTO.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeaderDTO.Status) ? SD.status_Pending : orderHeaderDTO.Status,
                };

                if (ModelState.IsValid)
                {
                    _db.OrderHeaders.Add(order);
                    await _db.SaveChangesAsync();

                    foreach (var orderDetailDTO in orderHeaderDTO.OrderDetailsDTO)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDTO.ItemName,
                            MenuItemId = orderDetailDTO.MenuItemId,
                            Price = orderDetailDTO.Price,
                            Quantity = orderDetailDTO.Quantity,
                        };
                        _db.OrderDetails.Add(orderDetails);
                    }

                    await _db.SaveChangesAsync();
                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.StatusCode = HttpStatusCode.Created;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add(ex.Message.ToString());
            }

            return _response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDTO orderHeaderUpdateDTO)
        {
            try
            {
                if(orderHeaderUpdateDTO is null || id != orderHeaderUpdateDTO.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                OrderHeader orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == id);
                if(orderFromDb is null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                if(!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupName)) {
                    orderFromDb.PickupName = orderHeaderUpdateDTO.PickupName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupPhoneNumber)) {
                    orderFromDb.PickupPhoneNumber = orderHeaderUpdateDTO.PickupPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupEmail)) {
                    orderFromDb.PickupEmail = orderHeaderUpdateDTO.PickupEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.Status)) {
                    orderFromDb.Status = orderHeaderUpdateDTO.Status;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.StripePaymentIntentID)) {
                    orderFromDb.StripePaymentIntentID = orderHeaderUpdateDTO.StripePaymentIntentID;
                }

                await _db.SaveChangesAsync();
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add(ex.Message.ToString());
            }

            return _response;
        }
    }
}
