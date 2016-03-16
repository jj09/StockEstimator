# StockEstimator

F# library for estimating future stock prices

## About

This library use linear regression for estimating future stock prices based on the historical data.

## Projects

* **StockEstimator.Logic** - F# project that calculates the future stock prices
* **StockEstimator.Tests** - F# unit test project written with xUnit, FsUnit, FsCheck and Unquote
* **StockEstimator.WebApi** - F# RESTful API for get getting stock prices (using StockEstimator.Logic)  written in F# with <a href="https://suave.io/">Suave Framework</a>
* **StockEstimator.Web** - <a href="https://github.com/aspnet">ASP.NET Core</a> web application built with <a href="http://aurelia.io">Aurelia Framework</a> that uses StockEstimator.WebApi for getting future stock prices
* **StockEstimator.Charts** - F# WebForms app that display chart with future stock price estimates
* **StockEstimator.ConsoleApp** - C# console app that get future stock prices using StockEstimator.Logic

## TODO

* Refactor logic to perform one request per estimate range
* Prettify UI
* Add more advanced algorithms for estimating stock prices
* Deploy to Azure (as <a href="https://msdn.microsoft.com/en-us/virtualization/windowscontainers/quick_start/azure_setup">Windows Container</a>)
* Deploy to Heroku
* add real-time prices for current day (i.e. refresh result every 10(?) seconds to get latest price) - will be visible when estimating based on last 1-2 days)
* add caching results on server-side
* add caching results on client-side
