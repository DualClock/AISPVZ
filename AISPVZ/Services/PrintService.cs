using AISPVZ.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AISPVZ.Services;

public class PrintService
{
    private readonly QrCodeService _qrService = new();

    public void PrintIssueReceipt(Order order, Employee employee, decimal totalAmount)
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(40),
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12
        };

        doc.Blocks.Add(new Paragraph(new Run("АКТ ВЫДАЧИ ЗАКАЗА"))
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });

        var infoTable = new Table();
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(300) });

        void AddInfoRow(string label, string value)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontWeight = FontWeights.Bold }) { Padding = new Thickness(4) });
            row.Cells.Add(new TableCell(new Paragraph(new Run(value))) { Padding = new Thickness(4) });
            infoTable.RowGroups[0].Rows.Add(row);
        }

        infoTable.RowGroups.Add(new TableRowGroup());
        AddInfoRow("Штрихкод:", order.Barcode);
        AddInfoRow("Клиент:", order.Client?.FullName ?? "-");
        AddInfoRow("Телефон:", order.Client?.Phone ?? "-");
        AddInfoRow("Маркетплейс:", order.Marketplace.ToString());
        AddInfoRow("Ячейка:", order.Cell?.CellCode ?? "-");
        AddInfoRow("Дата выдачи:", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        AddInfoRow("Оператор:", employee?.FullName ?? "-");
        AddInfoRow("Сумма:", $"{totalAmount:N2} ₽");

        doc.Blocks.Add(infoTable);

        doc.Blocks.Add(new Paragraph(new Run("Состав заказа:")) { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 20, 0, 10) });

        var itemsTable = new Table();
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(30) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(250) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(60) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(80) });

        itemsTable.RowGroups.Add(new TableRowGroup());

        var headerRow = new TableRow();
        headerRow.Cells.Add(MakeCell("№", true));
        headerRow.Cells.Add(MakeCell("Наименование", true));
        headerRow.Cells.Add(MakeCell("Кол-во", true));
        headerRow.Cells.Add(MakeCell("Цена", true));
        itemsTable.RowGroups[0].Rows.Add(headerRow);

        int idx = 1;
        foreach (var item in order.Items)
        {
            var row = new TableRow();
            row.Cells.Add(MakeCell(idx.ToString()));
            row.Cells.Add(MakeCell(item.ProductName));
            row.Cells.Add(MakeCell(item.Quantity.ToString()));
            row.Cells.Add(MakeCell($"{item.Price:N2}"));
            itemsTable.RowGroups[0].Rows.Add(row);
            idx++;
        }

        doc.Blocks.Add(itemsTable);

        var qrData = $"PVZ|{order.Barcode}|{order.Client?.Phone}|{DateTime.Now:yyyyMMddHHmm}";
        var qrBytes = _qrService.GenerateQrCodeBytes(qrData, 10);

        var qrImage = new BitmapImage();
        using (var ms = new MemoryStream(qrBytes))
        {
            qrImage.BeginInit();
            qrImage.StreamSource = ms;
            qrImage.CacheOption = BitmapCacheOption.OnLoad;
            qrImage.EndInit();
            qrImage.Freeze();
        }

        var qrContainer = new BlockUIContainer(new Image
        {
            Source = qrImage,
            Width = 120,
            Height = 120,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 20, 0, 10)
        });
        doc.Blocks.Add(qrContainer);

        doc.Blocks.Add(new Paragraph(new Run("Отсканируйте QR-код для верификации"))
        {
            FontSize = 10,
            Foreground = Brushes.Gray,
            TextAlignment = TextAlignment.Center
        });

        doc.Blocks.Add(new Paragraph(new Run("\n\n_________________ (подпись клиента)") { FontSize = 10 }));
        doc.Blocks.Add(new Paragraph(new Run($"_________________ (подпись оператора: {employee?.FullName})") { FontSize = 10 }));

        PrintDocument(doc, $"Акт выдачи {order.Barcode}");
    }

    public void PrintAcceptReceipt(Order order)
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(40),
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12
        };

        doc.Blocks.Add(new Paragraph(new Run("АКТ ПРИЁМКИ ЗАКАЗА"))
        {
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });

        var infoTable = new Table();
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(180) });
        infoTable.Columns.Add(new TableColumn { Width = new GridLength(300) });
        infoTable.RowGroups.Add(new TableRowGroup());

        void AddInfoRow(string label, string value)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontWeight = FontWeights.Bold }) { Padding = new Thickness(4) });
            row.Cells.Add(new TableCell(new Paragraph(new Run(value))) { Padding = new Thickness(4) });
            infoTable.RowGroups[0].Rows.Add(row);
        }

        AddInfoRow("Штрихкод:", order.Barcode);
        AddInfoRow("Клиент:", order.Client?.FullName ?? "-");
        AddInfoRow("Телефон:", order.Client?.Phone ?? "-");
        AddInfoRow("Маркетплейс:", order.Marketplace.ToString());
        AddInfoRow("Ячейка:", order.Cell?.CellCode ?? "-");
        AddInfoRow("Дата приёмки:", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        AddInfoRow("Плановая выдача:", order.PlannedIssueDate.ToString("dd.MM.yyyy"));

        doc.Blocks.Add(infoTable);

        doc.Blocks.Add(new Paragraph(new Run("Состав заказа:")) { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 20, 0, 10) });

        var itemsTable = new Table();
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(30) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(250) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(60) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(80) });
        itemsTable.RowGroups.Add(new TableRowGroup());

        var headerRow = new TableRow();
        headerRow.Cells.Add(MakeCell("№", true));
        headerRow.Cells.Add(MakeCell("Наименование", true));
        headerRow.Cells.Add(MakeCell("Кол-во", true));
        headerRow.Cells.Add(MakeCell("Цена", true));
        itemsTable.RowGroups[0].Rows.Add(headerRow);

        int idx = 1;
        foreach (var item in order.Items)
        {
            var row = new TableRow();
            row.Cells.Add(MakeCell(idx.ToString()));
            row.Cells.Add(MakeCell(item.ProductName));
            row.Cells.Add(MakeCell(item.Quantity.ToString()));
            row.Cells.Add(MakeCell($"{item.Price:N2}"));
            itemsTable.RowGroups[0].Rows.Add(row);
            idx++;
        }

        doc.Blocks.Add(itemsTable);
        doc.Blocks.Add(new Paragraph(new Run("\n\n_________________ (подпись клиента)") { FontSize = 10 }));

        PrintDocument(doc, $"Акт приёмки {order.Barcode}");
    }

    public void PrintReport<T>(IEnumerable<T> data, string title, string[] headers, Func<T, string[]> rowSelector)
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(30),
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 11
        };

        doc.Blocks.Add(new Paragraph(new Run(title))
        {
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        });

        doc.Blocks.Add(new Paragraph(new Run($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}"))
        {
            FontSize = 10,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 15)
        });

        var table = new Table();
        foreach (var _ in headers)
            table.Columns.Add(new TableColumn());
        table.RowGroups.Add(new TableRowGroup());

        var hRow = new TableRow();
        foreach (var h in headers)
            hRow.Cells.Add(MakeCell(h, true));
        table.RowGroups[0].Rows.Add(hRow);

        foreach (var item in data)
        {
            var row = new TableRow();
            foreach (var cell in rowSelector(item))
                row.Cells.Add(MakeCell(cell));
            table.RowGroups[0].Rows.Add(row);
        }

        doc.Blocks.Add(table);
        PrintDocument(doc, title);
    }

    private static TableCell MakeCell(string text, bool bold = false)
    {
        var para = new Paragraph(new Run(text));
        if (bold) para.FontWeight = FontWeights.Bold;
        return new TableCell(para) { Padding = new Thickness(4), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1) };
    }

    private static void PrintDocument(FlowDocument document, string description)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            printDialog.PrintDocument(paginator, description);
        }
    }
}
