namespace QuotationManager.Models.ViewModels
{
    public class AddProductViewModel
    {
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public IFormFile? Image { get; set; }
        public string ProductDetailsHtml { get; set; }  // HTML from WYSIWYG
    }
}
