namespace StockDataEstimator.Tests

open System
open StockEstimator.Logic

module StockDataTests =
    open Xunit
    open FsUnit.Xunit
    open FsCheck.Xunit
    open System.Linq
    open Swensen.Unquote

    [<Fact>]
    let ``returns stock data in correct format`` () =
        // Arrange
        let stockData = StockData()        

        // Act
        let data = stockData.GetStockData "msft"
        let firstKey = Enumerable.ToList(data.Keys).[0]
        let firstValue = data.Values.First()

        // Assert
        Assert.Equal(DateTime.Now.GetType(), firstKey.GetType())
        Assert.Equal((decimal 1).GetType(), firstValue.GetType())
        //firstKey |> should be ofExactType<DateTime>   // throws MissingMethodException
        //firstValue |> should be ofExactType<decimal>    // throws MissingMethodException       


    [<Theory>]
    [<InlineData("2016-02-01", "2016-02-01", 1)>]
    [<InlineData("2016-02-01", "2016-02-02", 2)>]
    [<InlineData("2016-01-25", "2016-01-29", 5)>]
    [<InlineData("2016-01-25", "2016-01-31", 5)>]
    [<InlineData("2016-01-23", "2016-01-31", 5)>]
    [<InlineData("2016-01-22", "2016-01-31", 6)>]
    [<InlineData("2016-01-22", "2016-02-01", 7)>]
    [<InlineData("2016-01-01", "2016-01-31", 19)>]  // New Year, MLK
    [<InlineData("2016-01-16", "2016-01-17", 0)>]   // wekend
    let ``GetStockDataForDateRange returns correct number of results`` from till expectedRowCount =
        // Arrange
        let stockData = StockData()        

        // Act
        let data = stockData.GetStockDataForDateRange "msft" (DateTime.Parse from) (DateTime.Parse till)

        // Assert
        Assert.Equal(expectedRowCount, data.Count)
        data.Count |> should equal expectedRowCount // fsunit: https://fsprojects.github.io/FsUnit/#What-is-FsUnit
        // test <@ expectedRowCount = data.Count @> // throws MissingMethodException

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(10)>]
    [<InlineData(28)>]
    [<InlineData(100)>]
    [<InlineData(365)>]
    let ``GetEstimatedPriceForDateWithRandom returns price greater or less by 5% from GetEstimatedPriceForDate`` (addDays: int) =
        // Arrange
        let stockData = StockData()
        let targetDay = DateTime.Now.AddDays(float addDays)
        let lookBackTill = DateTime.Now.AddYears(-1)

        // Act
        let estimatedPrice = stockData.GetEstimatedPriceForDate("msft", targetDay, lookBackTill)
        let estimatedPriceWithRandom = stockData.GetEstimatedPriceForDateWithRandom("msft", targetDay, lookBackTill)

        // Assert
        estimatedPriceWithRandom |> should (equalWithin (estimatedPrice*0.05)) estimatedPrice

    // property-based style
    type Days =
        static member Int() =
            FsCheck.Arb.Default.Int32()
            |> FsCheck.Arb.filter (fun i -> i > 0)

    type DayProperty () =
        inherit PropertyAttribute(Arbitrary = [| typeof<Days> |])

    //[<Property(Arbitrary = [| typeof<Days> |])>]
    [<DayProperty>]
    let ``GetEstimatedPriceForDateWithRandom returns price greater or less by 5% from GetEstimatedPriceForDate (property style)`` (addDays: int) =
        // Arrange
        let stockData = StockData()
        let targetDay = DateTime.Now.AddDays(float addDays)
        let lookBackTill = DateTime.Now.AddYears(-1)

        // Act
        let estimatedPrice = stockData.GetEstimatedPriceForDate("msft", targetDay, lookBackTill)
        let estimatedPriceWithRandom = stockData.GetEstimatedPriceForDateWithRandom("msft", targetDay, lookBackTill)

        // Assert
        estimatedPriceWithRandom |> should (equalWithin (estimatedPrice*0.05)) estimatedPrice