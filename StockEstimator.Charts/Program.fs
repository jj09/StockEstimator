module MainApp

open System
open System.Windows.Forms
open FSharp.Charting
open StockEstimator.Charts

[<EntryPoint>]
[<STAThread>]
let main argv = 
 
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault false
 
    let estimateBasedOnLastYearsCount = 2
    let estimateFutureDaysCount = 365
        
    let chartsDrawing = ChartsDrawing()
    let estimateFrom = DateTime.Now.AddYears(-estimateBasedOnLastYearsCount)
    let estimateTill = DateTime.Now.AddDays(float estimateFutureDaysCount)
    let chart = chartsDrawing.DrawEstimate "msft" estimateFrom estimateTill

    Application.Run(chart.ShowChart());
    0