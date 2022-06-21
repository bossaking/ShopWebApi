using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ShopWebApi.Models;

namespace ShopWebApi.DAL
{
    public class UsersDatabase : IDatabase
    {
        
        private List<User> Users { get; set; }

        public UsersDatabase()
        {
            if (!LoadFile())
            {
                this.Users = new List<User>();
            }
        }
        
        public bool LoadFile()
        {
            try
            {
                var data = System.IO.File.ReadAllText(@".\Database\users.json");
                this.Users = JsonConvert.DeserializeObject<List<User>>(data);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool SaveFile()
        {
            var data = JsonConvert.SerializeObject(this.Users);
            try
            {
                System.IO.File.WriteAllText(@".\Database\users.json", data);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public void Create(Model model)
        {
            Users.Add((User)model);
        }

        public Model GetById(Guid id)
        {
            foreach (var u in this.Users)
            {
                if (u.Id.Equals(id))
                {
                    return u;
                }
            }

            return null;
        }

        public User GetByEmail(string email)
        {
            foreach (var user in this.Users)
            {
                if (user.Email.Equals(email)) return user;
            }

            return null;
        }

        public IEnumerable<Model> GetAll()
        {
            return this.Users;
        }

        public bool Update(Guid oldId, Model model)
        {
            var newUser = (User) model;
            var user = (User)GetById(oldId);
            if (user == null) return false;

            user.Name = newUser.Name;
            user.Email = newUser.Email;
            user.Surname = newUser.Surname;
            return true;
        }

        public bool Delete(Guid id)
        {
            for (var i = 0; i < this.Users.Count; i++)
            {
                if (!this.Users[i].Id.Equals(id)) continue;
                this.Users.RemoveAt(i);
                return true;
            }

            return false;
        }

        public void AddProductToCart(Guid userId, Product product)
        {
            var user = GetById(userId);
            ((User)user).AddProductToCart(product);
        }

        public void RemoveProductFromCart(Guid userId, Guid productId)
        {
            var user = GetById(userId);
            ((User)user).RemoveProductFromCart(productId);
        }

        public int GetUserCartProductsCount(Guid userId)
        {
            var user = (User)GetById(userId);
            return user.Cart == null ? 0 : user.Cart.Count;
        }
        
        public double GetUserCartCost(Guid userId)
        {
            var user = (User)GetById(userId);
            if (user.Cart == null) return 0;
            var cartCost = 0.0;
            foreach (var product in user.Cart)
            {
                cartCost += product.Price;
            }

            return cartCost;
        }

        public List<Product> GetUserCart(Guid userId)
        {
            var user = (User)GetById(userId);
            return user.Cart;
        }
    }
}