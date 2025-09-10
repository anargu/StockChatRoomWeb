using Microsoft.Extensions.Logging;

namespace StooqBot;

public class StockDataParser
{
    private readonly ILogger<StockDataParser>? _logger;

    public StockDataParser(ILogger<StockDataParser>? logger = null)
    {
        _logger = logger;
    }

    public decimal ParsePriceFromCsv(string csvData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(csvData))
            {
                return 0m;
            }

            // CSV format: Symbol,Date,Time,Open,High,Low,Close,Volume
            // Example: AAPL.US,2024-01-09,22:00:02,185.56,185.56,185.56,185.56,0
            var lines = csvData.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            // Check if we have at least one data line
            if (lines.Length == 0)
            {
                return 0m;
            }

            // Determine which line contains the data
            string dataLine;
            if (lines.Length > 1)
            {
                // Multiple lines - assume first line is header, take second line
                dataLine = lines[1];
            }
            else
            {
                // Single line - assume it's the data line
                dataLine = lines[0];
            }

            var columns = dataLine.Split(',');
            
            if (columns.Length >= 7)
            {
                // Use Close price (column 6, 0-indexed)
                if (decimal.TryParse(columns[6], out var closePrice))
                {
                    // Return the close price only if it's positive, otherwise return 0
                    return closePrice > 0 ? closePrice : 0m;
                }
            }
            
            // Fallback: try to find any positive decimal value in the data
            // Only use fallback when CSV structure is wrong (less than 7 columns)
            var words = csvData.Split(new char[] { ' ', ',', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (decimal.TryParse(word, out var price) && price > 0)
                {
                    return price;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error parsing CSV data: {CsvData}", csvData);
        }

        return 0m; // Return 0 if parsing fails
    }

    public bool IsValidStockData(string data)
    {
        return !string.IsNullOrWhiteSpace(data) && 
               !data.Contains("N/D") && 
               data != "Data not found";
    }
}