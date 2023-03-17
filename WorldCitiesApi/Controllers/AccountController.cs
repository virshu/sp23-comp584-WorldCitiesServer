using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using WorldCitiesApi.Dtos;
using WorldModel;

namespace WorldCitiesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<WorldCitiesUser> _userManager;
        private readonly JwtHandler _jwtHandler;

        public AccountController(UserManager<WorldCitiesUser> userManager, JwtHandler jwtHandler)
        {
            _userManager = userManager;
            _jwtHandler = jwtHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            WorldCitiesUser? user = await _userManager.FindByNameAsync(loginRequest.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password)) {
                return Unauthorized(new LoginResult {
                    Success = false,
                    Message = "Invalid Username or Password."
                });
            }

            JwtSecurityToken secToken = await _jwtHandler.GetTokenAsync(user);
            string? jwt = new JwtSecurityTokenHandler().WriteToken(secToken);
            return Ok(new LoginResult {
                Success = true,
                Message = "Login successful",
                Token = jwt
            });
        }
    }
}
