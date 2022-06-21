namespace ShopWebApi.DTO
{
    public class CreateProductRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
    }
}