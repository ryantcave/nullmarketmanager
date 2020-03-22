using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager.Models
{
    class ExportResult
    {
        public List<OrderResult> orderResults;
        public bool didExecute;

        public ExportResult(List<OrderResult> orderResults, bool didExecute)
        {
            this.orderResults = orderResults;
            this.didExecute = didExecute;
        }
    }
}
