using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using quizTool.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace quizTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public QuizTool_Dbcontext _DbContext;

        public UserController(QuizTool_Dbcontext dbContext)
        {
            _DbContext = dbContext;
        }


        // [HttpPost("LoginUser")]

        // public async Task<IActionResult> LoginUser([FromBody] LoginModel userobj)
        // {
        //     if (userobj == null || string.IsNullOrEmpty(userobj.email) || string.IsNullOrEmpty(userobj.password))
        //     {
        //         return BadRequest(new { message = "Email and password are required." });
        //     }

        //     var user = await _DbContext.Users
        //.FirstOrDefaultAsync(u => u.email == userobj.email && u.password == userobj.password);

        //     if (user == null)
        //     {
        //         return Unauthorized(new { message = "Invalid email or password." });
        //     }

        //     user.Token = CreateJWT(user);

        //     return Ok(new
        //     {
        //         userId = user.userid,
        //         name = user.name,
        //         email = user.email,
        //         role = user.role
        //     });
        // }

        [AllowAnonymous]
        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser([FromBody] LoginModel userobj)
        {
            if (userobj == null || string.IsNullOrEmpty(userobj.email) || string.IsNullOrEmpty(userobj.password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var user = await _DbContext.Users
                .FirstOrDefaultAsync(u => u.email == userobj.email && u.password == userobj.password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var token = CreateJWT(user);

            var response = new LoginResponseModel
            {
                userId = user.userid,
                name = user.name,
                email = user.email,
                role = user.role,
                token = token
            };

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] UserDataModel newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.email) || string.IsNullOrWhiteSpace(newUser.password) || string.IsNullOrWhiteSpace(newUser.name))
            {
                return BadRequest(new { message = "Name, Email, and Password are required." });
            }

            var userExists = await _DbContext.Users.AnyAsync(u => u.email == newUser.email);
            if (userExists)
            {
                return Conflict(new { message = "User with this email already exists." });
            }

            newUser.role = "basic"; // default role
            newUser.createddate = DateTime.UtcNow;

            _DbContext.Users.Add(newUser);
            await _DbContext.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _DbContext.Users
                .Select(u => new
                {
                    u.userid,
                    u.name,
                    u.email,
                    u.role,
                    u.createddate
                })
                .ToListAsync();

            return Ok(users);
        }

        private string CreateJWT(UserDataModel user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("underworldmeinjigrayaarakenaamka");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.role),
                new Claim(ClaimTypes.Name, user.email)
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);

        }



    }
}
