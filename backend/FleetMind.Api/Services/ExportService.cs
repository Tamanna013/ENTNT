using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace FleetMind.Api.Services;

public class ExportService : IExportService
{
    public async Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();

        // Write header
        var headers = properties.Select(p => EscapeCsvValue(p.Name));
        sb.AppendLine(string.Join(",", headers));

        // Write data
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var val = p.GetValue(item);
                return EscapeCsvValue(val?.ToString() ?? string.Empty);
            });
            sb.AppendLine(string.Join(",", values));
        }

        // UTF-8 with BOM for Excel compatibility
        var utf8WithBom = new UTF8Encoding(true);
        var bytes = utf8WithBom.GetBytes(sb.ToString());
        var bom = utf8WithBom.GetPreamble();
        
        var result = new byte[bom.Length + bytes.Length];
        System.Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        System.Buffer.BlockCopy(bytes, 0, result, bom.Length, bytes.Length);

        return await Task.FromResult(result);
    }

    public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName)
    {
        using var workbook = new XLWorkbook();
        WriteSheet(workbook, sheetName, data.Cast<object>(), typeof(T));

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return await Task.FromResult(ms.ToArray());
    }

    public async Task<byte[]> ExportMultiSheetToExcelAsync(Dictionary<string, IEnumerable<object>> sheets)
    {
        using var workbook = new XLWorkbook();
        foreach (var kvp in sheets)
        {
            var sheetName = kvp.Key;
            var data = kvp.Value;
            var type = data.GetType().GetGenericArguments().FirstOrDefault() ?? data.FirstOrDefault()?.GetType() ?? typeof(object);
            WriteSheet(workbook, sheetName, data, type);
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return await Task.FromResult(ms.ToArray());
    }

    private void WriteSheet(XLWorkbook workbook, string sheetName, IEnumerable<object> data, System.Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Header
        for (var i = 0; i < properties.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = properties[i].Name; // Cosmetic limitation noted: using exact property name
            cell.Style.Font.Bold = true;
        }

        // Data
        var rowIndex = 2;
        foreach (var item in data)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                var val = properties[i].GetValue(item);
                worksheet.Cell(rowIndex, i + 1).Value = val?.ToString() ?? string.Empty;
            }
            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
