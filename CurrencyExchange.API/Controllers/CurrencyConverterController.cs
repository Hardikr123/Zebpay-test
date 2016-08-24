using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using AttributeRouting.Web.Http;

using System.Threading.Tasks;
using DAL.DBManager;
using DAL.Entities;
using System.Configuration;
using System.Dynamic;
using System.Runtime.Caching;
using DAL;

namespace CurrencyExchange.API.Controllers
{
     
    public class CurrencyConverterController : ApiController
    {
        MemoryCache MemCache = MemoryCache.Default;//creating defaulte instance of MemoryCache
        CurrencyExchangeManager _DBManager = new CurrencyExchangeManager();
        string _SupportedCurrList = ConfigurationManager.AppSettings["CurrencyList"]; //stored Currency list in web.config for which exchange rate need to be published

        [HttpPost]
        public HttpResponseMessage rate([FromBody]APIInputParameter obj)
        {

            try
            {
                if (obj == null) { obj = new APIInputParameter() { amount = 1, CurrencyCode = "USD" }; }
                if (obj.CurrencyCode == null || obj.CurrencyCode.Length == 0) { obj.CurrencyCode = "USD"; obj.amount = 1; }
                if (obj.amount == 0) { obj.amount = 1; }
                string[] CurrArray = _SupportedCurrList.Split(',');
                bool flag = true;
                for (int i = 0; i < CurrArray.Length; i++)
                {
                    if (CurrArray[i] == obj.CurrencyCode)
                        flag = true;
                }
                if (flag)
                {

                    var res = MemCache.Get("CachedRate");
                    if (res != null)
                    {
                        var excRate = res as List<CurrencyExchangeRates>;
                        CurrencyExchangeRates exc = new CurrencyExchangeRates();
                        exc = excRate.Find(x => x.SourceCurrency == obj.CurrencyCode);
                        ApiResponse resp = new ApiResponse();
                        resp.returncode = 1;
                        resp.err = "success";
                        resp.SourceCurrency = obj.CurrencyCode;
                        resp.ConversionRate = Math.Round(exc.ExchangeRate, 2).ToString("####.##");
                        resp.Amount = obj.amount;
                        resp.Total = (Math.Round(exc.ExchangeRate, 2) * obj.amount).ToString("####.##");
                        resp.timestamp = exc.UpdatedDate.Ticks;
                        return Request.CreateResponse<ApiResponse>(HttpStatusCode.OK, resp);
                    }
                    else
                    {
                        CurrencyContext _context = new CurrencyContext();
                        
                        var CurrRate = _context.CurrencyRates.ToList().Where(x => x.SourceCurrency == obj.CurrencyCode).FirstOrDefault();

                        if (CurrRate != null)
                        {
                            ApiResponse resp = new ApiResponse();
                            resp.returncode = 1;
                            resp.err = "success";
                            resp.SourceCurrency = obj.CurrencyCode
                                ;
                            resp.ConversionRate = Math.Round(CurrRate.ExchangeRate, 2).ToString("####.##");
                            resp.Amount = obj.amount;
                            resp.Total = (Math.Round(CurrRate.ExchangeRate, 2) * obj.amount).ToString("####.##");
                            resp.timestamp = CurrRate.UpdatedDate.Ticks;

                            MemCache.Add("CachedRate",_context.CurrencyRates.ToList(), DateTimeOffset.UtcNow.AddMinutes(2));
                            return Request.CreateResponse<ApiResponse>(HttpStatusCode.OK, resp);
                        }
                        else
                        {
                            return Request.CreateResponse<ErrorDesc>(HttpStatusCode.OK, new ErrorDesc() { err = "Currency Rate not Updated!", returncode = 0 });
                        }
                    }
                }
                else
                {
                    return Request.CreateResponse<ErrorDesc>(HttpStatusCode.OK, new ErrorDesc() { err = "Currency Code Not Supported!", returncode = 0 });
                }
            }

            catch (Exception ex)
            {
                return Request.CreateResponse<ErrorDesc>(HttpStatusCode.OK, new ErrorDesc() { err = ex.Message, returncode = 0 });
            }

        }

        [HttpGet]
        [ActionName("Exchange")]
        ///this action will be called from azure scheduler 
        public async Task< HttpResponseMessage> UpdateExchanges()
        {

            try
            {
                //Update change rate in table
                await Task.Run(()=> _DBManager.UpdateExchange());
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //return error message along with response statuscode
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new ErrorDesc() { returncode=0,err= ex.Message});
            }
       
        }


    }
}
