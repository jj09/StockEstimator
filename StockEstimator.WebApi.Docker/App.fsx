#r "./packages/Suave/lib/net40/Suave.dll"
#r "./packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "./packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#load "StockData.fsx"

open System
open System.Net
open Suave
open Suave.Http
open Suave.Web
open Suave.Successful
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open StockEstimator.Logic
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave.Operators
open Suave.Writers
open Suave.Successful
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

let JSON v =
  let jsonSerializerSettings = JsonSerializerSettings()
  jsonSerializerSettings.ContractResolver <- CamelCasePropertyNamesContractResolver()

  JsonConvert.SerializeObject(v, jsonSerializerSettings)
  |> OK
  >=> Writers.setMimeType "application/json; charset=utf-8"

let setCORSHeaders =
    setHeader  "Access-Control-Allow-Origin" "*"
    >=> setHeader "Access-Control-Allow-Headers" "content-type"

let getPrice =
    StockEstimator.Logic.GetEstimatedPriceForDateWithRandom("msft", DateTime.Now.AddDays(float 5), DateTime.Now.AddYears(-1)) |> JSON

let getPriceForDateRange ticker till since =
    let endDateTime = DateTime.Parse till
    let fromDate = DateTime.Parse since
    StockEstimator.Logic.GetEstimatedPriceForDateRangeWithRandom(ticker, DateTime.Now, endDateTime, fromDate) |> JSON

// THIS IS AWESOME :D
let getPriceForDateRangeRequest =
    request (fun r -> 
        match r.queryParam "ticker" with
        | Choice1Of2 ticker -> (request (fun r ->
            match r.queryParam "till" with
            | Choice1Of2 till -> (request (fun r ->
                match r.queryParam "since" with
                | Choice1Of2 since -> getPriceForDateRange ticker till since
                | Choice2Of2 msg -> BAD_REQUEST msg))
            | Choice2Of2 msg -> BAD_REQUEST msg))
        | Choice2Of2 msg -> BAD_REQUEST msg)

let getPriceFromAzure =
    // from https://www.reddit.com/r/programming/comments/4ee3s7/getting_started_with_f/
    let bind name nextStep = request (fun r -> 
        match r.queryParam name with
        | Choice1Of2 value -> nextStep value
        | Choice2Of2 msg -> BAD_REQUEST msg)

    let experiments = dict[
        "msft", ("9a21fffe27f34d6b90ab83a9a28af1f5", "9LqHbhrox5Bq05Eqwb4+0dg+7S9avXEsse03xUKhRBAc4Paz7I6KzB9/k9sXYnD1db61HzubiYuB4jp3IXd6ew=="); 
        "msft(2015-11-04 to 2016-11-03)", ("08c4ee6a411145fca9973c2fd3a8fad6", "GHWmBkKaEXMVi5bifrYSi1h0BqIwD5ccopEm8E4RZqbTidwQAZVf1vQikDoITAR8LFVP9AH3ObxzN+0NpotXOw=="); 
        "msft(2016-10-04 to 2016-11-03)", ("cdbf8d1e5f6d4aa396518a6bfe4bef52", "UbkxG8mVjv9Keyn2iKh+G2rzUGROrbfZuZ9dtQyTGH4Nrq/cZ10BUpm1OL348YsJJf9mLPJQXBi/JatUDiHEGw==")];

    bind "date" <| fun date -> 
    bind "experiment" <| fun experiment -> 
        let service = fst (experiments.Item(experiment))
        let token = snd (experiments.Item(experiment))
        Http.RequestString(sprintf "https://ussouthcentral.services.azureml.net/workspaces/03f600fc9ca34cc9a692c2aeea610df0/services/%s/execute?api-version=2.0&format=swagger" service, 
            headers = [ContentType HttpContentTypes.Json; Authorization (sprintf "Bearer %s" token) ],
            body = TextRequest (sprintf """ {"Inputs": { "input1": [{'Date': "%s"}]}, "GlobalParameters":  {}} """ date)) |> JSON


let webPart = 
    choose [
        GET >=> setCORSHeaders >=> choose [
            path "/" >=> Files.file "index.html"

            path "/GetPrice" >=> getPrice
            path "/GetPriceForDateRange" >=> getPriceForDateRangeRequest

            path "/GetPriceFromAzure" >=> getPriceFromAzure

            //static files
            pathRegex "(.*)\.(css|png|gif)" >=> Files.browseHome
        ]
    ]

let config =
  { defaultConfig with
       bindings = [HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") 80us]
  }

let resp = "Processed by " + System.Environment.MachineName
startWebServer config webPart