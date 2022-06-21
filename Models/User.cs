using System;
using System.Collections;
using System.Collections.Generic;
using ShopWebApi.Enums;

namespace ShopWebApi.Models
{
    [Serializable]
    public class User : Model
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Roles Role { get; set; }
        public List<Product> Cart { get; set; }

        public void AddProductToCart(Product product)
        {
            if (this.Cart == null)
            {
                this.Cart = new List<Product>();
            }
            this.Cart.Add(product);
        }

        public void RemoveProductFromCart(Guid productId)
        {
            for (var i = 0; i < this.Cart.Count; i++)
            {
                if (!this.Cart[i].Id.Equals(productId)) continue;
                this.Cart.RemoveAt(i);
                return;
            }
        }
    }
}