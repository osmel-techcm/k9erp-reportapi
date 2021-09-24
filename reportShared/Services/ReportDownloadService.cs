using OfficeOpenXml;
using reportCore.Entities;
using reportCore.Interfaces;
using reportCore.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace reportShared.Services
{
    public class ReportDownloadService : IReportDownloadService
    {
        public async Task<responseData> GenerateExcelReport(int reportId, dynamic dataReport, List<ColumnsReport> columnsReport)
        {
            var responseData = new responseData();
            var reportName = string.Empty;

            switch (reportId)
            {
                case 1:
                    reportName = "Logs per Personnels & Clients";
                    break;
                case 2:
                    reportName = "Fobs per Personnels & Clients";
                    break;
                case 3:
                    reportName = "Personnels & Clients per Doors";
                    break;
                case 4:
                    reportName = "Door Status";
                    break;
            }

            var ef = new ExcelPackage();
            var ws = ef.Workbook.Worksheets.Add(reportName);

            var startCellTitle = ws.Cells[1, 1];
            var endCellTitle = ws.Cells[1, columnsReport.Count()];
            startCellTitle.Style.Font.Size = 12;

            ws.Cells[startCellTitle.Address + ":" + endCellTitle].Merge = true;
            ws.Cells[startCellTitle.Address + ":" + endCellTitle].Value = reportName;

            int rowIndex = 3;
            int colIndex = 1;

            foreach (var colu in columnsReport)
            {
                var col = ws.Column(colIndex);
                var colName = ws.Cells[rowIndex, colIndex];
                colName.Value = colu.name;
                colName.Style.Font.Bold = true;
                colName.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                colName.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                colIndex++;
            }

            rowIndex = 4;

            foreach (var row in dataReport)
            {
                colIndex = 1;
                foreach (var col in columnsReport)
                {
                    ws.Cells[rowIndex, colIndex].Value = row[col.field].ToString().Trim();
                    colIndex++;
                }
                rowIndex++;
            }

            ws.Cells.AutoFitColumns();

            responseData.data = ef.GetAsByteArray();

            return responseData;
        }
    }
}
