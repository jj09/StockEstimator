//module StockEstimator.Web

open System
open Suave
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
  let jsonSerializerSettings = new JsonSerializerSettings()
  jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

  JsonConvert.SerializeObject(v, jsonSerializerSettings)
  |> OK
  >=> Writers.setMimeType "application/json; charset=utf-8"

let getPrice =
    StockData.GetEstimatedPriceForDateWithRandom("msft", DateTime.Now.AddDays(float 5), DateTime.Now.AddYears(-1)) |> JSON

let getPriceForDateRange ticker till since =
    let endDateTime = DateTime.Parse till
    let fromDate = DateTime.Parse since
    StockData.GetEstimatedPriceForDateRangeWithRandom(ticker, DateTime.Now, endDateTime, fromDate) |> JSON

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
    request (fun r ->
        match r.queryParam "date" with
        | Choice1Of2 date -> Http.RequestString("https://ussouthcentral.services.azureml.net/workspaces/03f600fc9ca34cc9a692c2aeea610df0/services/cdbf8d1e5f6d4aa396518a6bfe4bef52/execute?api-version=2.0&format=swagger", 
            headers = [ContentType HttpContentTypes.Json; Authorization "Bearer <TOKEN>" ],
            body = TextRequest (sprintf """ {"Inputs": { "input1": [{'Date': "%s"}]}, "GlobalParameters":  {}} """ date)) |> JSON
        | Choice2Of2 msg -> BAD_REQUEST msg)

let setCORSHeaders =
    setHeader  "Access-Control-Allow-Origin" "*"
    >=> setHeader "Access-Control-Allow-Headers" "content-type"

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

startWebServer defaultConfig webPart
