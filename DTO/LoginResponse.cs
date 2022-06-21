using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ShopWebApi.DTO
{
    public class LoginResponse : IActionResult
    {
        public bool Result { get; set; }
        public List<string> Messages { get; set; }
        public Task ExecuteResultAsync(ActionContext context)
        {
            throw new System.NotImplementedException();
        }
        public string Token { get; set; }
    }
}