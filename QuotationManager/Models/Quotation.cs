namespace QuotationManager.Models
{
    public class Quotation
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string ReferenceNumber { get; set; }

        public DateTime QuotationDate { get; set; }

        public decimal GSTPercentage { get; set; }

        public decimal GSTAmount { get; set; }

        public decimal SubTotal { get; set; }   //before applying GST

        public decimal Total { get; set; }  

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }

        public string? QuotationFIlePath { get; set; }   

        public ICollection<QuotationItem> QuotationItems { get; set; }
    }

}
