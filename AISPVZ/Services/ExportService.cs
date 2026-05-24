using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using AISPVZ.Models;

namespace AISPVZ.Services;

public class ExportService
{
    public async Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            Encoding = Encoding.UTF8
        });
        await csv.WriteRecordsAsync(data);
    }

    public void ExportToExcel<T>(IEnumerable<T> data, string filePath, string sheetName = "Data")
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties();
        var headers = properties.Select(p => p.Name).ToArray();

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var item in data)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(item);
                ws.Cell(row, col + 1).Value = value?.ToString() ?? "";
            }
            row++;
        }

        ws.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
    }

    public async Task ExportOrdersToCsvAsync(Microsoft.EntityFrameworkCore.DbContext db, string filePath)
    {
        var orders = await db.Set<Order>()
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .ToListAsync();

        var exportData = orders.Select(o => new
        {
            o.Id,
            o.Barcode,
            ClientName = o.Client.FullName,
            ClientPhone = o.Client.Phone,
            CellCode = o.Cell?.CellCode ?? "",
            o.Marketplace,
            o.CurrentStatus,
            o.ArrivedAt,
            o.PlannedIssueDate,
            o.IssuedAt,
            ItemsCount = o.Items.Count,
            TotalPrice = o.Items.Sum(i => i.Price * i.Quantity)
        }).ToList();

        await ExportToCsvAsync(exportData, filePath);
    }
}

