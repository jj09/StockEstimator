namespace StockDataEstimator.Tests

open System
open StockEstimator.Logic

module StockDataTests =
    open Xunit
    open System.Linq

    [<Fact>]
    let ``returns stock data in correct format`` () =
        // Arrange
        let stockData = StockData()        

        // Act
        let data = stockData.GetStockData "msft"
        let firstElement = Enumerable.ToList(data.Keys).[0]
        let firstValue = data.Values.First()

        // Assert
        Assert.Equal(DateTime.Now.GetType(), firstElement.GetType())
        Assert.Equal((decimal 1).GetType(), firstValue.GetType())