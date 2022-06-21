using System.Collections.Generic;

namespace ShopWebApi.Helpers
{
    public class ActionResponse
    {
        public string Token { get; set; }
        public bool Result { get; set; }
        public IEnumerable<string> Messages { get; set; }
    }
}