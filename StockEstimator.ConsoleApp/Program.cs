using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockEstimator.Logic;

namespace StockEstimator.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // fetching stock data
            var msftRows = StockData.GetStockData("MSFT");
            //foreach (var row in msftRows)
            //{
            //    Console.WriteLine($"{row.Key}: {string.Format("{0:.00}", float.Parse(row.Value))}");
            //}

            // fetching stock data for date/time range
            var msftRowsForLastYear = StockData.GetStockDataForDateRange("MSFT", DateTime.Now.AddYears(-1), DateTime.Now);
            //foreach (var row in msftRowsForLastYear)
            //{
            //    Console.WriteLine($"{row.Key}: {string.Format("{0:.00}", float.Parse(row.Value))}");
            //}

            // estimating future price
            var futureDate = DateTime.Now.AddDays(5);
            var estimatedPrice = StockData.GetEstimatedPriceForDate("MSFT", futureDate, DateTime.Now.AddMonths(-4));
            Console.WriteLine($"Estimated price for {futureDate.ToShortDateString()} is {string.Format("{0:.00}", estimatedPrice)}");
        }
    }
}
