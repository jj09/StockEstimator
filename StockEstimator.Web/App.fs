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

let JSON v =
  let jsonSerializerSettings = new JsonSerializerSettings()
  jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

  JsonConvert.SerializeObject(v, jsonSerializerSettings)
  |> OK
  >=> Writers.setMimeType "application/json; charset=utf-8"

let getPrice =
    StockData.GetEstimatedPriceForDateWithRandom("msft", DateTime.Now.AddDays(float 5), DateTime.Now.AddYears(-1)) |> JSON

let getPriceForDateRnage =
    StockData.GetEstimatedPriceForDateRangeWithRandom("msft", DateTime.Now, DateTime.Now.AddDays(float 5), DateTime.Now.AddYears(-1)) |> JSON

let webPart = 
    choose [
        GET >=> choose [
            path "/" >=> Files.file "index.html"

            path "/GetPrice" >=> getPrice
            path "/GetPriceForDateRange" >=> getPriceForDateRnage

            //static files
            pathRegex "(.*)\.(css|png|gif)" >=> Files.browseHome
        ]

    ]

startWebServer defaultConfig webPart
