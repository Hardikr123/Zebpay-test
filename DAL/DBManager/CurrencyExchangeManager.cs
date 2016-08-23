using DAL.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DAL.DBManager
{
    public class CurrencyExchangeManager
    {
        private CurrencyContext _context;
        public CurrencyExchangeManager()
        {

            _context = new CurrencyContext();
        }


        
        public async Task UpdateExchange()
        {
            //get currency list  needed for publish
            string Clist = ConfigurationManager.AppSettings["CurrencyList"];
            string[] ListArray = Clist.Split(',');
            var Currencies = _context.CurrencyRates.ToList();
            for (int i = 0; i < ListArray.Length; i++)
            {
                var client = new RestClient("https://www.google.com/finance/converter?a=1&from=" + ListArray[i] + "&to=INR&meta=" + Guid.NewGuid().ToString());
                var request = new RestRequest(Method.GET);
                var queryResult = client.Execute(request);
                //[^<]+
                var result = Regex.Matches(queryResult.Content, "<span class=\"?bld\"?>([^<]+)</span>")[0].Groups[1].Value;
                result = result.Substring(0, result.IndexOf(' '));
                var CrrencyEntity = Currencies.Find(a => a.SourceCurrency == ListArray[i]);
                if (CrrencyEntity != null)
                {
                    CrrencyEntity.UpdatedDate = DateTime.Now;
                    CrrencyEntity.ExchangeRate = Convert.ToDecimal(result);
                }
                else
                {
                    CurrencyExchangeRates NewCrrency = new CurrencyExchangeRates();
                    NewCrrency.CreatedDate = DateTime.Now;
                    NewCrrency.UpdatedDate = DateTime.Now;
                    NewCrrency.ExchangeRate = Convert.ToDecimal(result);
                    NewCrrency.DestinationCurrency = "INR";
                    NewCrrency.SourceCurrency = ListArray[i];
                    _context.CurrencyRates.Add(NewCrrency);

                }


            }
            await Task.Run(() => _context.SaveChanges()); 

        }
    }
}
