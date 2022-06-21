using System;

namespace ShopWebApi.Models
{
    [Serializable]
    public class Product : Model
    {
        public string Title { get; set; }
        public string Description { get; set; }
        private double _price;
        public double Price
        {
            get => this._price - this._price / 100 * this.Discount;
            set => this._price = value;
        }

        public double Discount { get; set; }
        
        public void CalculateDiscountForUser(int productsCount)
        {
            this.Discount = productsCount * 0.5;
        }
        
    }
}