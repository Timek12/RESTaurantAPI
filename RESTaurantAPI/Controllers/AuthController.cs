using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RESTaurantAPI.Data;
using RESTaurantAPI.Models;
using RESTaurantAPI.Models.Dto;
using RESTaurantAPI.Services;
using RESTaurantAPI.Utility;
using System.Net;

namespace RESTaurantAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private string secretKey;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private ApiResponse _response;
        public AuthController(ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
            _response = new ApiResponse();
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {
            try
            {
                ApplicationUser userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                if (userFromDb is not null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.Errors.Add("User already exists");
                    return BadRequest(_response);
                }

                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Name = model.Name,
                    Email = model.UserName,
                    NormalizedEmail = model.UserName.ToUpper(),
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(SD.SD_Role_Admin).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.SD_Role_Admin));
                        await _roleManager.CreateAsync(new IdentityRole(SD.SD_Role_Customer));
                    }

                    if (model.Role.ToLower() == SD.SD_Role_Admin.ToLower())
                    {
                        await _userManager.AddToRoleAsync(user, SD.SD_Role_Admin);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.SD_Role_Customer);
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }

            }
            catch (Exception e)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Errors.Add("Error while registering user");
            }

            return BadRequest(_response);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            ApplicationUser? userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
            if(userFromDb is null || userFromDb.Email is null)
            {
                _response.Result = new LoginResponseDTO();
                _response.StatusCode=HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Errors.Add("Username or password is incorrect.");
                return BadRequest(_response);
            }

            bool isValidPassword = await _userManager.CheckPasswordAsync(userFromDb, model.Password);
            if(isValidPassword is false)
            {
                _response.Result = new LoginResponseDTO();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Errors.Add("Username or password is incorrect.");
                return BadRequest(_response);
            }

            // here we have to generate new JWT 

            LoginResponseDTO loginResponse = new LoginResponseDTO()
            {
                Email = userFromDb.Email,
                Token = "NOT ACTUAL JWT TOKEN",
            };

            if(string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.Result = new LoginResponseDTO();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Errors.Add("Username or password is incorrect.");
                return BadRequest(_response);
            }

            _response.Result = new LoginResponseDTO();
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }
    }

}
