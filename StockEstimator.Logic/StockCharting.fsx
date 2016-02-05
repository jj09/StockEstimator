#load @"..\packages\FSharp.Charting.0.90.13\FSharp.Charting.fsx"
#load @"..\packages\MathNet.Numerics.FSharp.3.10.0\MathNet.Numerics.fsx"
#r @"..\packages\FSharp.Data.2.3.0-beta2\lib\net40\FSharp.Data.dll"
#load "StockData.fs"


open System
open System.Drawing
open FSharp.Charting

open StockEstimator.Logic

let stockData = StockData()

let estimateBasedOnLastYearsCount = 2

let d = stockData.GetStockDataForDateRange "msft" (DateTime.Now.AddYears(-estimateBasedOnLastYearsCount)) DateTime.Now

let lst = [ for x in d -> (x.Key, x.Value)] @ [ for x in 1..365 -> (DateTime.Now.AddDays(float x), decimal (stockData.GetEstimatedPriceForDate ("msft", (DateTime.Now.AddDays(float x)), (DateTime.Now.AddYears(-estimateBasedOnLastYearsCount)))))]

Chart.Line lst