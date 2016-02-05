namespace StockEstimator.Logic

open System
open FSharp.Data

// this module use yahoo finance API: http://www.jarloo.com/yahoo_finance/
// for alternative API check: https://www.quandl.com/blog/api-for-stock-data

type Stocks = CsvProvider<"http://ichart.finance.yahoo.com/table.csv?s=msft">

type StockData() = 

    let getBaseStockUrl ticker =
        "http://ichart.finance.yahoo.com/table.csv?s=" + ticker
    
    let getUrlForDateTimeRange ticker (startDate:DateTime) (endDate:DateTime) = 
        let root = getBaseStockUrl ticker
        sprintf "%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" 
            root (startDate.Month - 1) startDate.Day startDate.Year 
                        (endDate.Month - 1) endDate.Day endDate.Year

    member this.GetStockData ticker =
        let stockData = Stocks.Load(getBaseStockUrl ticker)
        dict (stockData.Rows |> Seq.map (fun x -> x.Date, x.Close))

    member this.GetStockDataForDateRange ticker (startDate:DateTime) (endDate:DateTime) =
        let url = getUrlForDateTimeRange ticker startDate endDate
        let stockData = Stocks.Load(url)
        dict (stockData.Rows |> Seq.map (fun x -> x.Date, x.Close))

    member this.GetEstimatedPriceForDate (ticker, forDate: DateTime, fromDate: DateTime) =
        //// uncomment below when forDate and fromDate are optional (?forDate, ?fromDate)
        //let forDate = defaultArg forDate (DateTime.Now.AddDays(1.))
        //let fromDate = defaultArg fromDate (DateTime.Now.AddYears(-1))

        let stockData = Stocks.Load(getUrlForDateTimeRange ticker fromDate (DateTime.Now)).Rows

        let firstDate = (stockData |> Seq.last).Date
        
        // TODO: refactor to one-liner
        let adjData = stockData |> Seq.map (fun x -> (x.Date - firstDate).TotalDays, float x.Close)
        let xData = adjData |> Seq.map fst |> Seq.toArray
        let yData = adjData |> Seq.map snd |> Seq.toArray

        let forDateInDays = (forDate - firstDate).TotalDays

        let intercept, slope = MathNet.Numerics.Fit.Line(xData, yData)
        
        let result = forDateInDays*slope + intercept
        result
