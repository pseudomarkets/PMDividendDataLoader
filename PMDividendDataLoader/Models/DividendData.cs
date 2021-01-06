using System;
using System.Collections.Generic;
using System.Text;

namespace PMDividendDataLoader.Models
{
    public class DividendData
    {
        public string symbol { get; set; }
        public double dividendPerShare { get; set; }
        public double yield { get; set; }
        public DateTime paymentDate { get; set; }
        public DateTime exDate { get; set; }
    }
}
