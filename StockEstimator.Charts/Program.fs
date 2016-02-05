open System
open System.Windows.Forms
open FSharp.Charting
open StockEstimator.Logic

[<EntryPoint>]
[<STAThread>]
let main argv = 
 
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault false
 
    let stockData = StockData()

    let estimateBasedOnLastYearsCount = 2

    let data = stockData.GetStockDataForDateRange "msft" (DateTime.Now.AddYears(-estimateBasedOnLastYearsCount)) DateTime.Now

    let lst = [ for x in data -> (x.Key, x.Value)] @ [ for x in 1..365 -> (DateTime.Now.AddDays(float x), decimal (stockData.GetEstimatedPriceForDate ("msft", (DateTime.Now.AddDays(float x)), (DateTime.Now.AddYears(-estimateBasedOnLastYearsCount)))))]
 
    Application.Run (Chart.Line(lst).ShowChart());
 
    0 