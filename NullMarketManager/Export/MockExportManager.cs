using NullMarketManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager.Export
{
    class MockExportManager : IExportManager
    {
        public ExportResult CalculateExportResults()
        {
            List<OrderResult> items = new List<OrderResult>()
            {
                new OrderResult()
                {
                    Profit = 10,
                    ExportQuantity = 1000,
                    Name = "Test Name ",
                    Volume = 69.69f
                },
                new OrderResult()
                {
                    Profit = 420,
                    ExportQuantity = 12050,
                    Name = "Test Name 2",
                    Volume = 420.69f
                },
                new OrderResult()
                {
                    Profit = 19,
                    ExportQuantity = 123456,
                    Name = "Test Name 3",
                    Volume = 432534f
                }
            };

            return new ExportResult(items, true);
        }
    }
}
