using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShopWebApi.DAL;
using ShopWebApi.DTO;
using ShopWebApi.Enums;
using ShopWebApi.Helpers;
using ShopWebApi.Models;

namespace ShopWebApi.Controllers
{
    [Route("api/users/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatabase _database;
        private readonly JwtConfig _jwtConfig;

        public UsersController(IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _database = new UsersDatabase();
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
        {
            if (ModelState.IsValid)
            {
                if (((UsersDatabase) _database).GetByEmail(registerUser.Email) != null)
                {
                    return BadRequest(new SimpleResponse()
                    {
                        Result = false,
                        Messages = new List<string>()
                        {
                            "Konto z podanym adresem e-mail już istnieje"
                        }
                    });
                }

                var user = new User()
                {
                    Id = Guid.NewGuid(),
                    Email = registerUser.Email,
                    Name = registerUser.Name,
                    Password = registerUser.Password,
                    Surname = registerUser.Surname,
                    Role = Roles.User
                };

                if (registerUser.Password.Equals("Admin123@")) user.Role = Roles.Admin;

                var token = await GenerateJwtToken(user);

                _database.Create(user);
                _database.SaveFile();

                return Ok(new LoginResponse()
                {
                    Result = true,
                    Messages = new List<string>()
                    {
                        "Rejestracja przebegła pomyślnie"
                    },
                    Token = token
                });
            }

            return BadRequest(new SimpleResponse()
            {
                Result = false,
                Messages = new List<string>()
                {
                    "Niepoprawne dane"
                }
            });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
        {
            if (ModelState.IsValid)
            {
                var user = ((UsersDatabase) _database).GetByEmail(loginUser.Email);
                if (user == null)
                {
                    return BadRequest(new SimpleResponse()
                    {
                        Result = false,
                        Messages = new List<string>()
                        {
                            "Konto z podanym adresem e-mail nie istnieje"
                        }
                    });
                }

                if (!user.Password.Equals(loginUser.Password))
                {
                    return BadRequest(new SimpleResponse()
                    {
                        Result = false,
                        Messages = new List<string>()
                        {
                            "Niepoprawne hasło"
                        }
                    });
                }

                var token = await GenerateJwtToken(user);

                return Ok(new LoginResponse()
                {
                    Result = true,
                    Token = token
                });
            }

            return BadRequest(new SimpleResponse()
            {
                Result = false,
                Messages = new List<string>()
                {
                    "Niepoprawne dane"
                }
            });
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        [Route("cart")]
        public async Task<IActionResult> GetUserCart()
        {
            var id = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            var products = ((UsersDatabase) _database).GetUserCart(Guid.Parse(id));
            foreach (var product in products)
            {
                product.CalculateDiscountForUser(products.Count);
            }
            return Ok(products);
        }
        
        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        [Route("cart/cost")]
        public async Task<IActionResult> GetUserCartCost()
        {
            var id = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            return Ok(((UsersDatabase) _database).GetUserCartCost(Guid.Parse(id)));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(_database.GetAll());
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var claims = new List<Claim>()
            {
                new Claim("Id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRole = user.Role;
            claims.Add(new Claim(ClaimTypes.Role, userRole.ToString()));


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}