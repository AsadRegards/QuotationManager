namespace QuotationManager.Models
{
    public class Product
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; } // stored in HTML format

        public string? ImagePath { get; set; }

        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }

    }

}
