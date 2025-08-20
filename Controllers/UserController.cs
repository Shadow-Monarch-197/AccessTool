using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using quizTool.Models;
using System.IdentityModel.Tokens.Jwt;                   // CHANGED
using System.Security.Claims;
using System.Text;

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

        [AllowAnonymous]
        [HttpPost("LoginUser")]
        public async Task<IActionResult> LoginUser([FromBody] LoginModel userobj)
        {
            if (userobj == null || string.IsNullOrEmpty(userobj.email) || string.IsNullOrEmpty(userobj.password))
                return BadRequest(new { message = "Email and password are required." });

            var user = await _DbContext.Users
                .FirstOrDefaultAsync(u => u.email == userobj.email && u.password == userobj.password);

            if (user == null) return Unauthorized(new { message = "Invalid email or password." });

            var token = CreateJWT(user);

            return Ok(new LoginResponseModel
            {
                userId = user.userid,
                name = user.name,
                email = user.email,
                role = user.role,
                token = token
            });
        }

        [AllowAnonymous]
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] UserDataModel newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.email) ||
                string.IsNullOrWhiteSpace(newUser.password) ||
                string.IsNullOrWhiteSpace(newUser.name))
            {
                return BadRequest(new { message = "Name, Email, and Password are required." });
            }

            var exists = await _DbContext.Users.AnyAsync(u => u.email == newUser.email);
            if (exists) return Conflict(new { message = "User with this email already exists." });

            newUser.role = "basic";
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
                .Select(u => new { u.userid, u.name, u.email, u.role, u.createddate })
                .ToListAsync();

            return Ok(users);
        }

        // NEW: who-am-I for quick debugging of your token in Postman/Swagger/Angular
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                name = User.Identity?.Name,
                roles = User.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray(),
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray()
            });
        }

        private string CreateJWT(UserDataModel user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("underworldmeinjigrayaarakenaamka");

            // IMPORTANT: emit the exact names Program.cs is configured for
            var claims = new List<Claim>
            {
                new Claim("role", user.role),                               // NEW (instead of ClaimTypes.Role)
                new Claim(JwtRegisteredClaimNames.UniqueName, user.email)    // NEW (instead of ClaimTypes.Name)
            };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
    }
}
