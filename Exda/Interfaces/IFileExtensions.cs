using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exda.ReportFile
{
    public interface IFileExtensions
    {
        void GenerateCsvReport(string beginDate, string endDate);
        void GenerateExcelReport(string beginDate, string endDate);
    }
}
