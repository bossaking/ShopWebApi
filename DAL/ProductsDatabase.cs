using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ShopWebApi.Models;

namespace ShopWebApi.DAL
{
    public class ProductsDatabase : IDatabase
    {

        private List<Product> _products;

        public ProductsDatabase()
        {
            if (!LoadFile())
            {
                this._products = new List<Product>();
            }
        }
        
        public bool LoadFile()
        {
            try
            {
                var data = System.IO.File.ReadAllText(@".\Database\products.json");
                this._products = JsonConvert.DeserializeObject<List<Product>>(data);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool SaveFile()
        {
            var data = JsonConvert.SerializeObject(this._products);
            try
            {
                System.IO.File.WriteAllText(@".\Database\products.json", data);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public void Create(Model model)
        {
            this._products.Add((Product)model);
        }

        public Model GetById(Guid id)
        {
            foreach (var product in this._products)
            {
                if (product.Id.Equals(id))
                {
                    return product;
                }
            }

            return null;
        }

        public IEnumerable<Model> GetAll()
        {
            return this._products;
        }

        public bool Update(Guid oldId, Model model)
        {
            var newProduct = (Product) model;
            var product = (Product)GetById(oldId);
            if (product == null) return false;

            product.Title = newProduct.Title;
            product.Description = newProduct.Description;
            product.Price = newProduct.Price;
            return true;
        }

        public bool Delete(Guid id)
        {
            for (var i = 0; i < this._products.Count; i++)
            {
                if (!this._products[i].Id.Equals(id)) continue;
                this._products.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}