using System.Text.RegularExpressions;

namespace StockChatRoomWeb.Core.Services;

public static class StockCommandParser
{
    private const string StockCommandPattern = @"^/stock=([a-zA-Z0-9._]+)$";
    private const string StockCommandPrefix = "/stock=";

    public static bool IsStockCommand(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        return message.Trim().StartsWith(StockCommandPrefix, StringComparison.OrdinalIgnoreCase);
    }

    public static string? ExtractStockSymbol(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        var trimmedMessage = message.Trim();
        var match = Regex.Match(trimmedMessage, StockCommandPattern, RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : null;
    }

    public static bool IsValidStockSymbol(string? symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return false;

        // Basic validation: alphanumeric characters, dots, and underscores
        return Regex.IsMatch(symbol, @"^[a-zA-Z0-9._]+$") && symbol.Length <= 20;
    }

    public static string FormatStockResponse(string symbol, decimal? price, bool isSuccess, string? errorMessage = null)
    {
        if (!isSuccess || !price.HasValue)
        {
            return errorMessage ?? $"Sorry, I couldn't find stock information for {symbol.ToUpperInvariant()}.";
        }

        return $"La cotización para {symbol.ToUpperInvariant()} es ${price.Value:F2} por acción.";
    }
}