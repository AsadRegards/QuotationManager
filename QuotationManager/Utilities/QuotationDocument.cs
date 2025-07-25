using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuotationManager.Models;
using QuotationManager.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace QuotationManager.Utilities
{
    public class QuotationDocument : IDocument
    {
        private readonly List<QuotationViewModel> Models;

        public QuotationDocument(List<QuotationViewModel> models)
        {
            Models = models.OrderBy(x=>x.ImageBytes != null).ToList();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            List<QuotationViewModel> accessoriesList = new();
            foreach (var model in Models) {
                if ((model.ImageBytes==null || model.ImageBytes.Length == 0))
                {
                    accessoriesList.Add(model);
                    continue;
                }
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Background().Image("wwwroot/" + model.LetterheadImagePath);
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontSize(11));
                    page.Header().Element(header => ComposeHeader(header, model));
                    page.Content().Element(content => ComposeContent(content, model));
                });
            }
            if(accessoriesList.Count > 0)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Background().Image("wwwroot/" + accessoriesList[0].LetterheadImagePath);
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Header().Element(header => ComposeAsscessoriesHeader(header, accessoriesList[0]));
                    page.Content().Element(content => ComposeAccessoriesContent(content, accessoriesList));
                });
            }

        }

        void ComposeAsscessoriesHeader(IContainer header, QuotationViewModel Model)
        {
            header.PaddingTop(100).PaddingBottom(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignMiddle().Text($"Date: {DateTime.Now:dd-MM-yyyy}").FontSize(10);
                    row.RelativeItem().AlignCenter().Text($"Quotation For Accessories").Bold().Underline().FontSize(11);
                    row.RelativeItem().AlignRight().Text($"Ref: {Model.ReferenceNumber}").FontSize(10);
                });
            });
        }


        void ComposeHeader(IContainer header, QuotationViewModel Model)
        {
            header.PaddingTop(100).PaddingBottom(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignMiddle().Text($"Date: {DateTime.Now:dd-MM-yyyy}").FontSize(10);
                    row.RelativeItem().AlignCenter().Text($"Quotation For {Model.ProductName}").Bold().Underline().FontSize(10);
                    row.RelativeItem().AlignRight().Text($"Ref: {Model.ReferenceNumber}").FontSize(10);
                });
            });
        }


        void ComposeAccessoriesTable(IContainer container, List<QuotationViewModel> accModel)
        {
            var headerStyle = TextStyle.Default.Bold();
            container.Table(table =>
            {
                static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderRight(1).BorderLeft(1).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Padding(2);

                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(300);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                var headerFontSize = 10;
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Sr. #").Style(headerStyle).FontSize(headerFontSize);
                    header.Cell().Element(CellStyle).Text("Description").Style(headerStyle).FontSize(headerFontSize);
                    header.Cell().Element(CellStyle).AlignRight().Text("Qty").Style(headerStyle).FontSize(headerFontSize);
                    header.Cell().Element(CellStyle).AlignRight().Text("Unit Price (PKR)").Style(headerStyle).FontSize(headerFontSize);
                    header.Cell().Element(CellStyle).AlignRight().Text("Total Price (PKR)").Style(headerStyle).FontSize(headerFontSize);
                });

                var contentSize = 9;
                for(int i=0; i<accModel.Count; i++)
                {
                    table.Cell().Row((uint)(i + 1)).Column(1).Element(CellStyle).Text((i + 1).ToString()).FontSize(contentSize);
                    table.Cell().Row((uint)(i + 1)).Column(2).Element(CellStyle).Text(accModel[i].ProductName).FontSize(contentSize);
                    table.Cell().Row((uint)(i + 1)).Column(3).Element(CellStyle).Text(accModel[i].Quantity.ToString()).FontSize(contentSize);
                    table.Cell().Row((uint)(i + 1)).Column(4).Element(CellStyle).Text(accModel[i].UnitPrice.ToString()).FontSize(contentSize);
                    table.Cell().Row((uint)(i + 1)).Column(5).Element(CellStyle).Text((accModel[i].UnitPrice * accModel[i].Quantity).ToString()).FontSize(contentSize);
                }

                table.Cell().Row((uint)(accModel.Count + 1)).Column(1).ColumnSpan(2).RowSpan(3).Element(CellStyle).Text("Delivery: 4 to 6 Business Weeks.").Style(headerStyle).FontSize(10);
                var TotalGSTValue = accModel.Sum(x => x.GSTValue);
                var TotalAmountWithGST = accModel.Sum(x => x.Total);
                var NetTotal = TotalAmountWithGST - TotalGSTValue;
                //Net Total
                table.Cell().Row((uint)(accModel.Count + 1)).Column(3).ColumnSpan(2).Element(CellStyle).Text("Net Total:").Style(headerStyle).FontSize(contentSize);
                table.Cell().Row((uint)(accModel.Count + 1)).Column(5).Element(CellStyle).Text($"Rs {NetTotal.ToString("N0")}").FontSize(contentSize);
                //GST percentage and GST value
                table.Cell().Row((uint)(accModel.Count+2)).Column(3).ColumnSpan(2).Element(CellStyle).Text($"GST {accModel[0].GSTPercentage}%").Style(headerStyle).FontSize(contentSize);
                table.Cell().Row((uint)(accModel.Count+2)).Column(5).Element(CellStyle).Text($"Rs {TotalGSTValue.ToString("N0")}").FontSize(contentSize);
                //Grand Total
                table.Cell().Row((uint)(accModel.Count + 3)).Column(3).ColumnSpan(2).Element(CellStyle).Text($"Total Payable:").Style(headerStyle).FontSize(contentSize);
                table.Cell().Row((uint)(accModel.Count + 3)).Column(5).Element(CellStyle).Text($"Rs {TotalAmountWithGST.ToString("N0")}").FontSize(contentSize);
            });

        }
        
        void ComposeTable(IContainer container, QuotationViewModel Model) 
        {
            var headerStyle = TextStyle.Default.Bold();
            container.Table(table =>
            {
                static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderRight(1).BorderLeft(1).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Padding(2);

                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(125);
                    columns.ConstantColumn(250);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                var headerFontSize = 10;
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Border(0).Text("");
                    header.Cell().Element(CellStyle).Text("Description").Style(headerStyle).FontSize(headerFontSize);
                    header.Cell().Element(CellStyle).AlignRight().Text("Qty").Style(headerStyle).FontSize(headerFontSize);
                    header.Cell().Element(CellStyle).AlignRight().Text("Unit Price (PKR)").Style(headerStyle).FontSize(headerFontSize);
                    header.Cell().Element(CellStyle).AlignRight().Text("Total Price (PKR)").Style(headerStyle).FontSize(headerFontSize);
                });

                var contentSize = 9;
                table.Cell().Row(1).Column(1).RowSpan(3).Element(CellStyle).Image(Model.ImageBytes).FitArea();
                table.Cell().Row(1).Column(2).RowSpan(3).Element(CellStyle).Column(column =>
                {
                    var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.LoadHtml(Model.DetailsHtml ?? "");

                    foreach (var node in htmlDoc.DocumentNode.ChildNodes)
                    {
                        switch (node.Name.ToLower())
                        {
                            case "p":
                                column.Item().Text(text =>
                                {
                                    foreach (var child in node.ChildNodes)
                                    {
                                        var decodedText = System.Net.WebUtility.HtmlDecode(child.InnerText);
                                        if (child.Name.ToLower() == "b" || child.Name.ToLower() == "strong")
                                            text.Span(decodedText).Bold().FontSize(contentSize);
                                        else
                                            text.Span(decodedText).FontSize(contentSize);
                                    }
                                });
                                break;

                            case "ul":
                                foreach (var li in node.Elements("li"))
                                {
                                    var decoded = System.Net.WebUtility.HtmlDecode(li.InnerText);
                                    column.Item().Text("• " + decoded).FontSize(contentSize);
                                }
                                break;

                            case "ol":
                                int index = 1;
                                foreach (var li in node.Elements("li"))
                                {
                                    var decoded = System.Net.WebUtility.HtmlDecode(li.InnerText);
                                    column.Item().Text($"{index++}. {decoded}").FontSize(contentSize);
                                }
                                break;
                        }
                    }
                });

                    table.Cell().Element(CellStyle).MinHeight(180).Text(Model.Quantity.ToString()).FontSize(contentSize);
                table.Cell().Element(CellStyle).Text(Model.UnitPrice.ToString("N0")).FontSize(contentSize);
                table.Cell().Element(CellStyle).Text((Model.UnitPrice*Model.Quantity).ToString("N0")).FontSize(contentSize);

                table.Cell().Row(2).Column(3).ColumnSpan(2).Element(CellStyle).Text($"GST {Model.GSTPercentage}%").FontSize(contentSize);  
                table.Cell().Row(2).Column(5).Element(CellStyle).Text($"Rs {Model.GSTValue.ToString("N0")}").FontSize(contentSize);

                table.Cell().Row(3).Column(3).ColumnSpan(2).Element(CellStyle).Text($"Total Payable:").FontSize(contentSize);
                table.Cell().Row(3).Column(5).Element(CellStyle).Text($"Rs {Model.Total.ToString("N0")}").FontSize(contentSize);



            });
        }


        void ComposeAccessoriesContent(IContainer content, List<QuotationViewModel> accModel)
        {
            content.Column(col =>
            {
                col.Item().PaddingBottom(20).Row(row =>
                {

                    row.RelativeItem().AlignMiddle().Column(Line => {

                        if (!string.IsNullOrEmpty(accModel[0].CustomerName))
                        {
                            Line.Item().Text(accModel[0].CustomerName).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(accModel[0].AddressLine1))
                        {
                            Line.Item().Text(accModel[0].AddressLine1).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(accModel[0].AddressLine2))
                        {
                            Line.Item().Text(accModel[0].AddressLine2).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(accModel[0].AddressLine3))
                        {
                            Line.Item().Text(accModel[0].AddressLine3).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(accModel[0].Email))
                        {
                            Line.Item().Text(accModel[0].Email).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(accModel[0].PhoneNumber))
                        {
                            Line.Item().Text(accModel[0].PhoneNumber).FontSize(10).Bold();
                        }
                    });

                });

                col.Item().Element(content => ComposeAccessoriesTable(content, accModel));


                // 🟦 Signature and Stamp Section
                col.Item().PaddingTop(10).Row(row =>
                {
                    // Left side: Signature text + image
                    row.RelativeItem().Column(sig =>
                    {
                        sig.Item().Text("Yours truly,").FontSize(12);
                        sig.Item().Text("for Solutions Corporation").FontSize(12);

                        sig.Item().Height(80).Width(80).Image("wwwroot/" + accModel[0].SignatureImagePath);


                        sig.Item().Text(accModel[0].UserName).FontSize(11).Bold();
                        sig.Item().PaddingBottom(10).Text(accModel[0].Designation).FontSize(8);
                    });

                    row.RelativeItem(3).PaddingLeft(200).Height(80).Width(80).Image("wwwroot/" + accModel[0].StampImagePath);
                });
                //Custom Box
                col.Item().Border(1).Row(row =>
                {
                    // Custom Box
                    row.RelativeItem().Column(sig =>
                    {
                        sig.Item().Padding(5).AlignCenter().Text("Note: Validity is 30 Days, subject to final confirmation.  Prices quoted are subject to US$ rate fluctuation. If rate of US$ increases at the day of issuance of Purchase Order or till the period of supply of goods, the Purchase Order/Invoice shall be issued taken into account the prevailing rate of exchange of US$.").Bold().FontSize(8);
                    });

                });

            });
        }

        void ComposeContent(IContainer content, QuotationViewModel Model)
        {

            content.Column(col =>
            {
                col.Item().PaddingBottom(20).Row(row =>
                {

                    row.RelativeItem().AlignMiddle().Column(Line => {

                        if (!string.IsNullOrEmpty(Model.CustomerName))
                        {
                            Line.Item().Text(Model.CustomerName).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(Model.AddressLine1))
                        {
                            Line.Item().Text(Model.AddressLine1).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(Model.AddressLine2))
                        {
                            Line.Item().Text(Model.AddressLine2).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(Model.AddressLine3))
                        {
                            Line.Item().Text(Model.AddressLine3).FontSize(10).Bold();
                        }

                        if (!string.IsNullOrEmpty(Model.Email))
                        {
                            Line.Item().Text(Model.Email).FontSize(10).Bold().Underline();
                        }

                        if (!string.IsNullOrEmpty(Model.PhoneNumber))
                        {
                            Line.Item().Text(Model.PhoneNumber).FontSize(10).Bold();
                        }
                    });

                });

                col.Item().Element(content=>ComposeTable(content,Model));
               

                // 🟦 Signature and Stamp Section
                col.Item().PaddingTop(10).Row(row =>
                {
                    // Left side: Signature text + image
                    row.RelativeItem().Column(sig =>
                    {
                        sig.Item().Text("Yours truly,").FontSize(12);
                        sig.Item().Text("for Solutions Corporation").FontSize(12);

                        sig.Item().Height(80).Width(80).Image("wwwroot/" + Model.SignatureImagePath);


                        sig.Item().Text(Model.UserName).FontSize(11).Bold(); 
                        sig.Item().PaddingBottom(10).Text(Model.Designation).FontSize(8); 
                    });

                    row.RelativeItem(3).PaddingLeft(200).Height(80).Width(80).Image("wwwroot/" + Model.StampImagePath);
                });

                col.Item().Border(1).Row(row =>
                {
                    // Custom Box
                    row.RelativeItem().Column(sig =>
                    {
                        sig.Item().Padding(5).AlignCenter().Text("Note: Validity is 30 Days, subject to final confirmation.  Prices quoted are subject to US$ rate fluctuation. If rate of US$ increases at the day of issuance of Purchase Order or till the period of supply of goods, the Purchase Order/Invoice shall be issued taken into account the prevailing rate of exchange of US$.").Bold().FontSize(8);
                    });

                });

            });
        }
    }
}
