using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTaurantAPI.Data;
using RESTaurantAPI.Models;
using RESTaurantAPI.Models.Dto;
using System.Net;

namespace RESTaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ApiResponse _response;
        public OrderController(ApplicationDbContext db, ApiResponse response)
        {
            _db = db;
            _response = response;
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
                if(id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode=HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var orderHeader = await _db.OrderHeaders.Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem)
                    .FirstOrDefaultAsync(u => u.OrderHeaderId == id);

                if (orderHeader is null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode=HttpStatusCode.NotFound;
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

        
    }
}
