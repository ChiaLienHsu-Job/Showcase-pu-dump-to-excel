using System.Data;
using ClosedXML.Excel;

namespace Showcase.PU.Core
{
    public static class ExcelExporter
    {
        /// <summary>
        /// 將 Data / Errors 兩個 DataTable 匯出為 xlsx，並加上 Summary/Meta。
        /// </summary>
        public static void Export(string savePath, DataTable data, DataTable errors, string deviceId, string portName)
        {
            using var wb = new XLWorkbook();

            wb.Worksheets.Add(data, "Data");
            wb.Worksheets.Add(errors, "Errors");

            var sum = wb.Worksheets.Add("Summary");
            sum.Cell("A1").Value = "DeviceID"; sum.Cell("B1").Value = deviceId;
            sum.Cell("A2").Value = "Port"; sum.Cell("B2").Value = portName;
            sum.Cell("A3").Value = "Records"; sum.Cell("B3").Value = data.Rows.Count;
            sum.Cell("A4").Value = "Errors"; sum.Cell("B4").Value = errors.Rows.Count;

            var meta = wb.Worksheets.Add("Meta");
            meta.Cell("A1").Value = "Tool"; meta.Cell("B1").Value = "showcase-pu-dump-to-excel";
            meta.Cell("A2").Value = "ExportTime"; meta.Cell("B2").Value = DateTime.Now;

            var wsData = wb.Worksheets.Worksheet("Data");
            wsData.SheetView.FreezeRows(1);
            wsData.Columns().AdjustToContents();

            wb.SaveAs(savePath);
        }
    }
}
