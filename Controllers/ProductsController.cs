using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopWebApi.DAL;
using ShopWebApi.DTO;
using ShopWebApi.Enums;
using ShopWebApi.Models;

namespace ShopWebApi.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/products/")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IDatabase _database;
        private readonly IDatabase _usersDatabase;

        public ProductsController()
        {
            _database = new ProductsDatabase();
            _usersDatabase = new UsersDatabase();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("create")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest createProductRequest)
        {
            if (ModelState.IsValid)
            {
                var product = new Product()
                {
                    Id = Guid.NewGuid(),
                    Title = createProductRequest.Title,
                    Description = createProductRequest.Description,
                    Price = createProductRequest.Price
                };
                this._database.Create(product);
                this._database.SaveFile();

                return Ok(new SimpleResponse()
                {
                    Result = true,
                    Messages = new List<string>()
                    {
                        "Produkt został dodany"
                    }
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
        
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("all")]
        public async Task<IActionResult> GetAllProducts()
        {
            User user = null;
            var id = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            if (id != null)
            {
                user = (User) _usersDatabase.GetById(Guid.Parse(id));
            }

            var products = (List<Product>) _database.GetAll();
            if (user != null)
            {
                var productsCount = ((UsersDatabase) _usersDatabase).GetUserCartProductsCount(user.Id);
                foreach (var product in products)
                {
                    product.CalculateDiscountForUser(productsCount);
                }
            }

            return Ok(products);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Microsoft.AspNetCore.Mvc.Route("update")]
        public async Task<IActionResult> UpdateProduct([FromBody] CreateProductRequest createProductRequest,
            [FromQuery] Guid id)
        {
            if (ModelState.IsValid)
            {
                var product = new Product()
                {
                    Title = createProductRequest.Title,
                    Description = createProductRequest.Description,
                    Price = createProductRequest.Price
                };
                this._database.Update(id, product);
                this._database.SaveFile();

                return Ok(new SimpleResponse()
                {
                    Result = true,
                    Messages = new List<string>()
                    {
                        "Produkt został zaktualizowany"
                    }
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

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Microsoft.AspNetCore.Mvc.Route("delete")]
        public async Task<IActionResult> DeleteProduct([FromQuery] Guid id)
        {
            var product = this._database.GetById(id);
            if (product == null)
            {
                return BadRequest(new SimpleResponse()
                {
                    Result = false,
                    Messages = new List<string>()
                    {
                        "Produkt nie istanieje"
                    }
                });
            }

            this._database.Delete(id);
            this._database.SaveFile();

            return Ok(new SimpleResponse()
            {
                Result = true,
                Messages = new List<string>()
                {
                    "Produkt został usunięty"
                }
            });
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("cart/add")]
        public async Task<IActionResult> AddProductToCard([FromQuery] Guid productId)
        {
            var product = _database.GetById(productId);
            if (product == null)
            {
                return BadRequest(new SimpleResponse()
                {
                    Result = false,
                    Messages = new List<string>()
                    {
                        "Niepoprawne dane"
                    }
                });
            }

            var id = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            var user = (User) _usersDatabase.GetById(Guid.Parse(id));
            
            user.AddProductToCart((Product)product);
            _usersDatabase.SaveFile();
            
            return Ok(new SimpleResponse()
            {
                Result = true,
                Messages = new List<string>()
                {
                    "Produkt został dodany do koszyka"
                }
            });
        }
        
        [Authorize(Roles = "Admin, User")]
        [HttpDelete]
        [Microsoft.AspNetCore.Mvc.Route("cart/remove")]
        public async Task<IActionResult> RemoveProductFromCart([FromQuery] Guid productId)
        {
            var product = _database.GetById(productId);
            if (product == null)
            {
                return BadRequest(new SimpleResponse()
                {
                    Result = false,
                    Messages = new List<string>()
                    {
                        "Niepoprawne dane"
                    }
                });
            }

            var id = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
            var user = (User) _usersDatabase.GetById(Guid.Parse(id));
            
            user.RemoveProductFromCart(productId);
            _usersDatabase.SaveFile();
            
            return Ok(new SimpleResponse()
            {
                Result = true,
                Messages = new List<string>()
                {
                    "Produkt został usunięty z koszyka"
                }
            });
        }
    }
}