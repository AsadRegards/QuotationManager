namespace QuotationManager.Models.ViewModels
{
    public class CreateQuotationViewModel
    {
        #region Customer Details
        public string? CustomerName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }    
        public string? AddressLine3 { get; set; }    
        public string? Email { get; set; }   
        public string? PhoneNumber { get; set; } 
        #endregion
        public string ReferenceNumber { get; set; }
        public DateTime QuotationDate { get; set; } = DateTime.Now;
        public decimal GSTPercentage { get; set; } = 18;

        public List<QuotationProductLine> Products { get; set; }

        public class QuotationProductLine
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }

}
