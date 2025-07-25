namespace QuotationManager.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string SignatureImagePath { get; set; }

        public string StampImagePath { get; set; }

        public string Designation { get; set; }

        public string? LetterheadPdfPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; }

        public ICollection<Quotation> Quotations { get; set; }
    }

}
