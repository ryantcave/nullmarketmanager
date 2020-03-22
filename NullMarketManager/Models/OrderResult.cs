using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager.Models
{
    class OrderResult
    {
        public string Name { get; set; }
        public float Volume { get; set; }
        public double Profit { get; set; }
        public int ExportQuantity { get; set; }
    }
}
