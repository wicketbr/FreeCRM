using QuestPDF.Fluent;

namespace CRM;

public partial interface IDataAccess
{
    Task<DataObjects.Invoice> GenerateInvoiceImages(DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null);
    Task<DataObjects.Invoice> GenerateInvoicePDF(DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null);
}

public partial class DataAccess
{
    private async Task<InvoiceModel> ConvertInvoiceToInvoiceModel(DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null)
    {
        DataObjects.User user = new DataObjects.User();
        if (invoice.UserId.HasValue) {
            user = await GetUser(invoice.UserId.Value);
        }

        var tenant = GetTenant(invoice.TenantId, CurrentUser);

        var logo = await GetTenantLogo(invoice.TenantId);

        List<InvoiceOrderItem> items = new List<InvoiceOrderItem>();
        if (invoice.InvoiceItems.Any()) {
            foreach (var item in invoice.InvoiceItems) {
                items.Add(new InvoiceOrderItem {
                    Name = StringValue(item.Description),
                    Price = item.Price,
                    Quantity = item.Quantity,
                });
            }
        }

        var output = new InvoiceModel {
            Author = invoice.AddedBy,
            Comments = StringValue(invoice.Notes),
            CustomerAddress = new InvoiceAddress {
                CompanyName = StringValue(user.DisplayName),
                Email = StringValue(user.Email),
                Phone = StringValue(user.Phone),
            },
            DueDate = invoice.InvoiceDueDate.HasValue ? (DateTime)invoice.InvoiceDueDate : DateTime.Now,
            InvoiceNumber = StringValue(invoice.InvoiceNumber),
            IssueDate = invoice.InvoiceCreated.HasValue ? (DateTime)invoice.InvoiceCreated : DateTime.Now,
            SellerAddress = new InvoiceAddress {
                CompanyName = tenant != null ? tenant.Name : String.Empty,
                Email = DefaultReplyToAddressForTenant(invoice.TenantId),
            },
            Logo = logo != null ? logo.Value : null,
            LogoExtension = logo != null ? StringValue(logo.Extension) : String.Empty,
            Items = items,
        };

        return output;
    }

    private InvoiceDocument GenerateInvoiceDocument(InvoiceModel model)
    {
        var output = new InvoiceDocument(model);

        var metadata = output.GetMetadata();
        metadata.Author = model.Author;
        metadata.Creator = "freeCRM";
        metadata.Producer = "freeCRM";
        metadata.Subject = "Invoice";
        metadata.Title = "Invoice" + (!String.IsNullOrWhiteSpace(model.InvoiceNumber) ? " " + model.InvoiceNumber : "");

        return output;
    }

    public async Task<DataObjects.Invoice> GenerateInvoiceImages(DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null)
    {
        var invoiceModel = await ConvertInvoiceToInvoiceModel(invoice, CurrentUser);
        var images = GenerateInvoiceImages(invoiceModel);

        var output = invoice;
        output.Images = images;

        return output;
    }

    /// <summary>
    /// The invoice is returned as a nullable list of byte[] objects which are the byte values for the various jpeg images.
    /// </summary>
    /// <param name="model">An InvoiceModel object.</param>
    /// <returns>A nullable list of byte array objects containing jpeg images.</returns>
    private List<byte[]>? GenerateInvoiceImages(InvoiceModel model)
    {
        List<byte[]>? output = null;

        var document = GenerateInvoiceDocument(model);

        var images = document.GenerateImages(new QuestPDF.Infrastructure.ImageGenerationSettings {
            ImageCompressionQuality = QuestPDF.Infrastructure.ImageCompressionQuality.Best,
            ImageFormat = QuestPDF.Infrastructure.ImageFormat.Jpeg,
        });

        if(images != null && images.Any()) {
            output = images.ToList();
        }

        return output;
    }

    public async Task<DataObjects.Invoice> GenerateInvoicePDF(DataObjects.Invoice invoice, DataObjects.User? CurrentUser = null)
    {
        var invoiceModel = await ConvertInvoiceToInvoiceModel(invoice, CurrentUser);
        var pdf = GenerateInvoicePDF(invoiceModel);

        var output = invoice;
        output.PDF = pdf;

        return output;
    }

    private byte[]? GenerateInvoicePDF(InvoiceModel model)
    {
        byte[]? output = null;

        var document = GenerateInvoiceDocument(model);

        var pdfStream = new MemoryStream();
        document.GeneratePdf(pdfStream);
        output = pdfStream.ToArray();

        return output;
    }
}

public class InvoiceAddress
{
    public string CompanyName { get; set; } = "";
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}

public class InvoiceAddressComponent : QuestPDF.Infrastructure.IComponent
{
    private string Title { get; }
    private InvoiceAddress Address { get; }

    public InvoiceAddressComponent(string title, InvoiceAddress address)
    {
        Title = title;
        Address = address;
    }

    public void Compose(QuestPDF.Infrastructure.IContainer container)
    {
        container.Column(column => {
            column.Spacing(2);

            column.Item().BorderBottom(1).PaddingBottom(5).Text(Title).SemiBold();

            if (!String.IsNullOrWhiteSpace(Address.CompanyName)) {
                column.Item().Text(Address.CompanyName);
            }

            if (!String.IsNullOrWhiteSpace(Address.Street)) {
                column.Item().Text(Address.Street);
            }

            if (!String.IsNullOrWhiteSpace(Address.City) && !String.IsNullOrWhiteSpace(Address.State) && !String.IsNullOrWhiteSpace(Address.PostalCode)) {
                column.Item().Text($"{Address.City}, {Address.State}  {Address.PostalCode}");
            }

            if (!String.IsNullOrWhiteSpace(Address.Email)) {
                column.Item().Text(Address.Email);
            }

            if (!String.IsNullOrWhiteSpace(Address.Phone)) {
                column.Item().Text(Address.Phone);
            }
        });
    }
}

public class InvoiceDocument : QuestPDF.Infrastructure.IDocument
{
    private QuestPDF.Infrastructure.DocumentMetadata _metadata = QuestPDF.Infrastructure.DocumentMetadata.Default;
    private QuestPDF.Infrastructure.DocumentSettings _settings = QuestPDF.Infrastructure.DocumentSettings.Default;

    public InvoiceModel Model { get; }

    public InvoiceDocument(InvoiceModel model)
    {
        Model = model;
    }

    public QuestPDF.Infrastructure.DocumentMetadata GetMetadata() => _metadata;
    public QuestPDF.Infrastructure.DocumentSettings GetSettings() => _settings;

    public void Compose(QuestPDF.Infrastructure.IDocumentContainer container)
    {
        container
            .Page(page => {
                page.Margin(50);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().SkipOnce().AlignCenter().Text(x => {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
    }

    void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
    {
        var titleStyle = QuestPDF.Infrastructure.TextStyle.Default.FontSize(20).SemiBold();// .FontColor(Colors.Blue.Medium);

        container.Row(row => {
            row.RelativeItem().Column(column => {
                column.Item().Text($"Invoice {Model.InvoiceNumber}").Style(titleStyle);

                column.Item().Text(text => {
                    text.Span("Created: ").SemiBold();
                    text.Span($"{Model.IssueDate:d}");
                });

                column.Item().Text(text => {
                    text.Span("Due: ").SemiBold();
                    text.Span($"{Model.DueDate:d}");
                });

                if (!String.IsNullOrWhiteSpace(Model.PONumber)) {
                    column.Item().Text(text => {
                        text.Span("PO Number: ").SemiBold();
                        text.Span($"{Model.PONumber}");
                    });
                }
            });

            if (Model.Logo != null) {
                string extension = !String.IsNullOrWhiteSpace(Model.LogoExtension) ? Model.LogoExtension : "";

                switch (extension.ToLower()) {
                    case "svg":
                    case ".svg":
                        string svg = System.Text.Encoding.Default.GetString(Model.Logo);
                        row.ConstantItem(200).Height(50).AlignRight().AlignTop().Svg(svg).FitArea();
                        break;

                    default:
                        var imageStream = new MemoryStream(Model.Logo);
                        row.ConstantItem(200).Height(50).AlignRight().AlignTop().Image(imageStream).FitArea();
                        break;
                }
            }
        });
    }

    void ComposeContent(QuestPDF.Infrastructure.IContainer container)
    {
        container.PaddingVertical(20).Column(column => {
            column.Spacing(5);

            column.Item().Row(row => {
                row.RelativeItem().Component(new InvoiceAddressComponent("From", Model.SellerAddress));
                row.ConstantItem(20);
                row.RelativeItem().Component(new InvoiceAddressComponent("For", Model.CustomerAddress));
            });

            column.Item().Element(ComposeTable);

            var totalPrice = Model.Items.Sum(x => x.Price * x.Quantity);
            column.Item().AlignRight().Text("Total: " + totalPrice.ToString("C")).FontSize(14);

            if (!string.IsNullOrWhiteSpace(Model.Comments)) {
                column.Item().PaddingTop(20).Element(ComposeComments);
            }
        });
    }

    void ComposeTable(QuestPDF.Infrastructure.IContainer container)
    {
        container.Table(table => {
            // step 1
            table.ColumnsDefinition(columns => {
                columns.RelativeColumn(3);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            // step 2
            table.Header(header => {
                header.Cell().Element(CellStyle).Text("Description");
                header.Cell().Element(CellStyle).AlignRight().Text("Unit Price");
                header.Cell().Element(CellStyle).AlignRight().Text("Quantity");
                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                }
            });

            // step 3
            foreach (var item in Model.Items) {
                table.Cell().Element(CellStyle).Text(item.Name);
                table.Cell().Element(CellStyle).AlignRight().Text(item.Price.ToString("C"));
                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text((item.Price * item.Quantity).ToString("C"));

                static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
        });
    }

    void ComposeComments(QuestPDF.Infrastructure.IContainer container)
    {
        container
            .Background(QuestPDF.Helpers.Colors.Grey.Lighten4)
            .Border(0.2f)
            .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
            .Padding(5)
            .Column(column => {
                column.Spacing(5);
                column.Item().Text("Comments").FontSize(14).SemiBold();
                column.Item().Text(Model.Comments).FontSize(10).NormalWeight();
            });
    }
}

public class InvoiceModel
{
    public string? Author { get; set; }

    public string InvoiceNumber { get; set; } = "";
    public string PONumber { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }

    public InvoiceAddress SellerAddress { get; set; } = new InvoiceAddress();
    public InvoiceAddress CustomerAddress { get; set; } = new InvoiceAddress();

    public byte[]? Logo { get; set; }
    public string LogoExtension { get; set; } = "";

    public List<InvoiceOrderItem> Items { get; set; } = new List<InvoiceOrderItem>();
    public string Comments { get; set; } = "";
}

public class InvoiceOrderItem
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

//public static class SvgExtensions
//{
//    public static void Svg(this QuestPDF.Infrastructure.IContainer container, Svg.Skia.SKSvg svg)
//    {
//        container
//            .AlignCenter()
//            .AlignMiddle()
//            .ScaleToFit()
//            .Width(svg.Picture.CullRect.Width)
//            .Height(svg.Picture.CullRect.Height)
//            .Canvas((canvas, space) => canvas.DrawPicture(svg.Picture));
//    }
//}