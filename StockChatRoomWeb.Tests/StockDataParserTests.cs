using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using StooqBot;

namespace StockChatRoomWeb.Tests;

public class StockDataParserTests
{
    private readonly Mock<ILogger<StockDataParser>> _mockLogger;
    private readonly StockDataParser _parser;

    public StockDataParserTests()
    {
        _mockLogger = new Mock<ILogger<StockDataParser>>();
        _parser = new StockDataParser(_mockLogger.Object);
    }

    [Fact]
    public void ParsePriceFromCsv_ValidCsvWithHeader_ReturnsCorrectPrice()
    {
        // Sample data
        var csvData = @"Symbol,Date,Time,Open,High,Low,Close,Volume
AAPL.US,2024-01-09,22:00:02,185.50,186.00,184.75,185.56,1000000";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(185.56m, result);
    }

    [Fact]
    public void ParsePriceFromCsv_ValidCsvWithoutHeader_ReturnsCorrectPrice()
    {
        var csvData = "MSFT.US,2024-01-09,22:00:02,378.25,379.50,377.80,378.85,850000";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(378.85m, result);
    }

    [Fact]
    public void ParsePriceFromCsv_MultipleLines_ReturnsFirstDataLinePrice()
    {
        var csvData = @"Symbol,Date,Time,Open,High,Low,Close,Volume
GOOGL.US,2024-01-09,22:00:02,140.50,141.25,139.75,140.85,750000
GOOGL.US,2024-01-08,22:00:02,139.25,140.50,138.90,139.75,680000";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(140.85m, result); // Should return first data line close price
    }

    [Fact]
    public void ParsePriceFromCsv_IncompleteColumns_ReturnsFallbackPrice()
    {
        // CSV with only 4 columns (missing close price)
        var csvData = "TSLA.US,2024-01-09,22:00:02,245.50";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(245.50m, result);
    }

    [Fact]
    public void ParsePriceFromCsv_PriceWithDecimals_ParsesCorrectly()
    {
        var csvData = "AMZN.US,2024-01-09,22:00:02,151.123,151.789,150.456,151.678,950000";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(151.678m, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n\n")]
    public void ParsePriceFromCsv_EmptyOrWhitespace_ReturnsZero(string csvData)
    {
        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void ParsePriceFromCsv_InvalidData_ReturnsZero()
    {
        var csvData = "INVALID,DATA,NO,NUMBERS,HERE,ONLY,TEXT,INVALID";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void ParsePriceFromCsv_NullData_ReturnsZero()
    {
        var result = _parser.ParsePriceFromCsv(null!);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void ParsePriceFromCsv_MalformedCsv_UsesFallback()
    {
        // Malformed CSV but contains a valid price
        var csvData = "Some random text with a stock price of 99.45 somewhere in the middle";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(99.45m, result); // Should find the price using fallback
    }

    [Fact]
    public void ParsePriceFromCsv_NegativePrice_ReturnsZero()
    {
        // CSV with negative price (invalid)
        var csvData = "BAD.US,2024-01-09,22:00:02,100.00,100.00,100.00,-50.25,1000";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(0m, result); // Negative prices should be ignored
    }

    [Fact]
    public void ParsePriceFromCsv_ZeroPrice_ReturnsZero()
    {
        var csvData = "ZERO.US,2024-01-09,22:00:02,0.00,0.00,0.00,0.00,0";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(0m, result); // Zero prices should result in zero
    }

    [Fact]
    public void IsValidStockData_ValidData_ReturnsTrue()
    {
        var validData = "AAPL.US,2024-01-09,22:00:02,185.50,186.00,184.75,185.56,1000000";

        var result = _parser.IsValidStockData(validData);

        Assert.True(result);
    }

    [Theory]
    [InlineData("N/D")]
    [InlineData("Data not found")]
    [InlineData("AAPL.US,N/D,N/D,N/D,N/D,N/D,N/D,N/D")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValidStockData_InvalidData_ReturnsFalse(string invalidData)
    {
        var result = _parser.IsValidStockData(invalidData);

        Assert.False(result);
    }

    [Fact]
    public void ParsePriceFromCsv_ExceptionThrown_ReturnsZeroAndLogsError()
    {
        var parser = new StockDataParser(_mockLogger.Object);
        var csvData = "Valid data"; // This will cause an exception in the parsing logic

        var result = parser.ParsePriceFromCsv(csvData);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void ParsePriceFromCsv_RealWorldStooqFormat_ParsesCorrectly()
    {
        // Real format from Stooq API
        var csvData = @"Symbol,Date,Time,Open,High,Low,Close,Volume
AAPL.US,2024-01-09,22:00:02,185.56,185.56,185.56,185.56,0";

        var result = _parser.ParsePriceFromCsv(csvData);

        Assert.Equal(185.56m, result);
    }
}