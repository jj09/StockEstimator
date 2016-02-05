module StockCharts

open System
open System.Drawing
open FSharp.Charting


let DrawLine() =
    Chart.Line [ for x in 1.0 .. 100.0 -> (x, x ** 2.0) ]
