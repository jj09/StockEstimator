namespace StockDataEstimator.Tests

open System
open StockEstimator.Logic

// fix for MissingMethod Exception: https://github.com/SwensenSoftware/unquote/issues/119

module StockDataTests =
    open Xunit
    open FsUnit.Xunit
    open FsCheck
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
        Assert.Equal(DateTime.Now.GetType(), firstKey.GetType())    // xunit
        Assert.Equal((decimal 1).GetType(), firstValue.GetType())   // xunit
        firstKey |> should be ofExactType<DateTime>     // fsunit
        firstValue |> should be ofExactType<decimal>    // fsunit


    [<Theory>]
    [<InlineData("2016-02-01", "2016-02-01", 1)>]
    [<InlineData("2016-02-01", "2016-02-02", 2)>]
    [<InlineData("2016-01-25", "2016-01-29", 5)>]
    [<InlineData("2016-01-25", "2016-01-31", 5)>]
    [<InlineData("2016-01-23", "2016-01-31", 5)>]
    [<InlineData("2016-01-22", "2016-01-31", 6)>]
    [<InlineData("2016-01-22", "2016-02-01", 7)>]
    [<InlineData("2016-01-01", "2016-01-31", 19)>]  // Stock marked closed on New Year's Day and MLK Day
    [<InlineData("2016-01-16", "2016-01-17", 0)>]   // weekend
    let ``GetStockDataForDateRange returns correct number of results`` from till expectedRowCount =
        // Arrange
        let stockData = StockData()        

        // Act
        let data = stockData.GetStockDataForDateRange "msft" (DateTime.Parse from) (DateTime.Parse till)

        // Assert
        Assert.Equal(expectedRowCount, data.Count)  // xunit
        data.Count |> should equal expectedRowCount // fsunit: https://fsprojects.github.io/FsUnit/#What-is-FsUnit
        test <@ expectedRowCount = data.Count @>    // unquote

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
        static member Int32() =
            Arb.Default.Int32()
            |> Arb.filter (fun i -> i > 0 && i < 365)

    type DayPropertyAttribute() =
        inherit PropertyAttribute(Arbitrary = [| typeof<Days> |])

    //[<Property(Arbitrary = [| typeof<Days> |])>]  // alternative for DayPropertyAttribute definition
    [<DayProperty>]
    let ``GetEstimatedPriceForDateWithRandom returns price greater or less by 5% from GetEstimatedPriceForDate (property style)`` (addDays: Int32) =
        // Arrange
        let stockData = StockData()
        let targetDay = DateTime.Now.AddDays(float addDays)
        let lookBackTill = DateTime.Now.AddYears(-1)

        // Act
        let estimatedPrice = stockData.GetEstimatedPriceForDate("msft", targetDay, lookBackTill)
        let estimatedPriceWithRandom = stockData.GetEstimatedPriceForDateWithRandom("msft", targetDay, lookBackTill)

        // Assert        
        estimatedPriceWithRandom |> should (equalWithin (estimatedPrice*0.05)) estimatedPrice