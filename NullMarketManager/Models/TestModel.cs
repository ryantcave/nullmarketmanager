using System;
using System.Collections.Generic;
using System.Text;

namespace NullMarketManager.Models
{
    class TestModel
    {
        public TestModel()
        {

        }

        public TestModel(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public string key { get; set; }
        public string value { get; set; }
    }
}
