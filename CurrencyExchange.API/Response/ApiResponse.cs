using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


    public class ApiResponse
    {
        public int returncode { get; set; }

        public string SourceCurrency { get; set; }
        public string ConversionRate { get; set; }
        public string Total { get; set; }
        public decimal Amount { get; set; }
        public string err { get; set; }
        public long timestamp{ get; set; }
    }

    public class ErrorDesc
    {
        public string err { get; set; }

        public int returncode { get; set; }

    }



    public class APIInputParameter
    {
        public decimal amount { get; set; }

        public string CurrencyCode { get; set; }


    }

