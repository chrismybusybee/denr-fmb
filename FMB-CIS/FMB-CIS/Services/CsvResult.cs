using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FMB_CIS.Services
{
    public class CsvResult : ActionResult
    {
        readonly Func<CsvWriter, Task> writeCsv;
        readonly string fileName;

        public CsvResult(
            Func<CsvWriter, Task> writeCsv,
            string fileName)
        {
            this.writeCsv = writeCsv;
            this.fileName = fileName;
        }

        public override async Task ExecuteResultAsync(
            ActionContext context)
        {
            var httpContext = context.HttpContext;
            httpContext.Response.ContentType = "text/csv";
            httpContext.Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\"";

            await using var writer = new StreamWriter(httpContext.Response.Body);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await writeCsv(csv);
        }
    }
}
