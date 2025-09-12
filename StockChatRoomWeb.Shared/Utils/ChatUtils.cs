using StockChatRoomWeb.Shared.Constants;

namespace StockChatRoomWeb.Shared.Utils;

public static class ChatUtils
{
    // Utility to get group name for a specific chat room
    public static string GetRoomGroupName(string? roomId) => roomId == null ? ChatConstants.ChatRoomName : $"{ChatConstants.RoomGroupPrefix}{roomId}";
}