using NullMarketManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager.Export
{
    interface IExportManager
    {
        public ExportResult CalculateExportResults();
    }
}
