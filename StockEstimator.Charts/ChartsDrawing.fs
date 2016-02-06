namespace StockEstimator.Charts

open System
open System.Net
open FSharp.Charting
open StockEstimator.Logic
open System.Drawing

type ChartsDrawing() =

    member this.CreatePriceLine (stockData: List<DateTime * decimal>) stock color =
        Chart.Line (stockData, Name = stock)
        |> Chart.WithSeries.Style(Color = color, BorderWidth = 2)

    member this.DrawEstimate ticker (lookBackTill: DateTime) (estimateTill: DateTime) =
        let stockData = StockData()
        let data = stockData.GetStockDataForDateRange ticker lookBackTill DateTime.Now
        let past = [ for x in data -> (x.Key, x.Value)]
        let daysFromNowTillEstimateEnd = int (estimateTill - DateTime.Now).TotalDays
        let future = [ for x in 1..daysFromNowTillEstimateEnd -> (DateTime.Now.AddDays(float x), decimal (stockData.GetEstimatedPriceForDateWithRandom (ticker, DateTime.Now.AddDays(float x), lookBackTill)))]
        (Chart.Combine [ this.CreatePriceLine past "past" Color.SkyBlue 
                         this.CreatePriceLine future "future" Color.Red ])