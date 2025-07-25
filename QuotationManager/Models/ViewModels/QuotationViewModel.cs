namespace QuotationManager.Models.ViewModels
{
    public class QuotationViewModel
    {
        public string ReferenceNumber { get; set; }
        public DateTime Date { get; set; }
        public string ProductName { get; set; }
        public string DetailsHtml { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal GSTValue { get; set; }
        public decimal Total { get; set; }
        public byte[]? ImageBytes { get; set; } //Product Image Bytes

        public string Designation { get; set; }
        public string UserName { get; set; }


        public string SignatureImagePath { get; set; }
        public string StampImagePath { get; set; }
        public string LetterheadImagePath { get; set; }


        #region
        public string CustomerName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        #endregion

    }
}
