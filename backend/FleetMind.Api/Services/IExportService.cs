using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetMind.Api.Services;

public interface IExportService
{
    Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data);
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName);
    Task<byte[]> ExportMultiSheetToExcelAsync(Dictionary<string, IEnumerable<object>> sheets);
}
