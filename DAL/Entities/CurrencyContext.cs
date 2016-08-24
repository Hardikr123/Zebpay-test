using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class CurrencyContext : DbContext
    {
        public CurrencyContext()
            : base("name=ConnectionString")
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrencyExchangeRates>().Property(C => C.ExchangeRate).HasPrecision(18,4);
                base.OnModelCreating(modelBuilder);
        }
        public DbSet<CurrencyExchangeRates> CurrencyRates { get; set; }
    }
}
