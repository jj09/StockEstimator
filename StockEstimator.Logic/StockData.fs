namespace StockEstimator.Logic

open FSharp.Data
open MathNet
open System

type Stocks = CsvProvider<"Date,Open,High,Low,Close,Volume,Adj Close">

type StockData() = 

    let getBaseStockUrl ticker =
        "http://ichart.finance.yahoo.com/table.csv?s=" + ticker
    
    let getUrlForDateTimeRange ticker (startDate:DateTime) (endDate:DateTime) = 
        let root = getBaseStockUrl ticker
        sprintf "%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" 
            root (startDate.Month - 1) startDate.Day startDate.Year 
                        (endDate.Month - 1) endDate.Day endDate.Year

    member this.getStockData ticker =
        let stockData = Stocks.Load(getBaseStockUrl ticker)
        dict (stockData.Rows |> Seq.map (fun x -> x.Date, x.Close))

    member this.getStockDataForDateRange ticker (startDate:DateTime) (endDate:DateTime) =
        let url = getUrlForDateTimeRange ticker startDate endDate
        let stockData = Stocks.Load(url)
        dict (stockData.Rows |> Seq.map (fun x -> x.Date, x.Close))

    member this.getEstimatedPriceForDate (ticker, forDate: DateTime, fromDate: DateTime) =
        //// uncomment below when forDate and fromDate are optional (?forDate, ?fromDate)
        //let forDate = defaultArg forDate (DateTime.Now.AddDays(1.))
        //let fromDate = defaultArg fromDate (DateTime.Now.AddYears(-1))

        let stockData = Stocks.Load(getUrlForDateTimeRange ticker fromDate (DateTime.Now)).Rows

        let firstDate = DateTime.Parse((stockData |> Seq.last).Date)
        
        // refactor to one-liner
        let adjData = stockData |> Seq.map (fun x -> ((DateTime.Parse(x.Date) - firstDate).TotalDays, float x.Close)) 
        let xData = adjData |> Seq.map (fun x -> fst x) |> Seq.toArray
        let yData = adjData |> Seq.map (fun x -> snd x) |> Seq.toArray

        let forDateInDays = (forDate - firstDate).TotalDays

        let intercept, slope = MathNet.Numerics.Fit.Line(xData, yData)
        
        let result = forDateInDays*slope + intercept
        result
