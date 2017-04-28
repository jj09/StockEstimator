module StockEstimator.Logic.StockData

#r "./packages/Suave/lib/net40/Suave.dll"
#r "./packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "./packages/MathNet.Numerics/lib/net40/MathNet.Numerics.dll"

open System
open System.Collections.Generic
open FSharp.Data

// this module use yahoo finance API: http://www.jarloo.com/yahoo_finance/
// for alternative API check: https://www.quandl.com/blog/api-for-stock-data

type Stocks = CsvProvider<"http://ichart.finance.yahoo.com/table.csv?s=msft">

let stocksCache = new Dictionary<string, Stocks>()

let getBaseStockUrl ticker =
    "http://ichart.finance.yahoo.com/table.csv?s=" + ticker

let getUrlForDateTimeRange ticker (startDate:DateTime) (endDate:DateTime) = 
    let root = getBaseStockUrl ticker
    sprintf "%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" 
        root (startDate.Month - 1) startDate.Day startDate.Year 
                    (endDate.Month - 1) endDate.Day endDate.Year

let getStockDataForUrl url =
    if not (stocksCache.ContainsKey(url)) then 
        stocksCache.Add(url, Stocks.Load(url))
    stocksCache.[url]    

let GetStockData ticker =
    let stockData = getBaseStockUrl ticker |> getStockDataForUrl 
    dict (stockData.Rows |> Seq.map (fun x -> x.Date, x.Close))

let GetStockDataForDateRange ticker (startDate:DateTime) (endDate:DateTime) =
    let stockData = getUrlForDateTimeRange ticker startDate endDate |> getStockDataForUrl
    dict (stockData.Rows |> Seq.map (fun x -> x.Date, x.Close))

let GetEstimatedPriceForDate (ticker, forDate: DateTime, fromDate: DateTime) =
    let stockData = getUrlForDateTimeRange ticker fromDate (DateTime.Now) |> getStockDataForUrl
    let stockDataRows = stockData.Rows
    let firstDate = (stockDataRows |> Seq.last).Date
        
    let xData, yData = 
        stockDataRows 
        |> Seq.map (fun x -> (x.Date - firstDate).TotalDays, float x.Close)
        |> Seq.toArray 
        |> Array.unzip

    let forDateInDays = (forDate - firstDate).TotalDays

    let intercept, slope = MathNet.Numerics.Fit.Line(xData, yData)
        
    let result = forDateInDays*slope + intercept
    result

let GetEstimatedPriceForDateWithRandom (ticker, forDate: DateTime, fromDate: DateTime) =
    let price = GetEstimatedPriceForDate (ticker, forDate, fromDate)
    price - (price*0.05) + (price*0.1*Random().NextDouble())

let GetEstimatedPriceForDateRange (ticker, startDate: DateTime, endDate: DateTime, fromDate: DateTime) =
    let daysCount = int (endDate - startDate).TotalDays
    seq {
        for i in 0..daysCount do
            let dateTime = startDate.AddDays(float i)
            let price = GetEstimatedPriceForDate (ticker, dateTime, fromDate)
            yield (dateTime, price)
    }

let GetEstimatedPriceForDateRangeWithRandom (ticker, startDate: DateTime, endDate: DateTime, fromDate: DateTime) =
    let daysCount = int (endDate - startDate).TotalDays
    seq {
        for i in 0..daysCount do
            let dateTime = startDate.AddDays(float i)
            let price = GetEstimatedPriceForDateWithRandom (ticker, dateTime, fromDate)
            yield (dateTime, price)
    }