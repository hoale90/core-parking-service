using System.Linq;
using System.Text.Json.Serialization;

namespace CoreParking.Application.Common.Models;

/// <summary>
/// Cáº¥u trÃºc chuáº©n cho pháº£n há»“i API theo quy chuáº©n cá»§a Desktop App.
/// </summary>
public record ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("error")]
    public ApiError? Error { get; init; }

    public ApiResponse(bool success, T? data, ApiError? error)
    {
        Success = success;
        Data = data;
        Error = error;
    }

    public static ApiResponse<T> Ok(T data) => new(true, data, null);

    public static ApiResponse<T> Fail(string message, string code = "ERROR")
    {
        // Tá»± Ä‘á»™ng hoÃ¡n vá»‹ náº¿u tham sá»‘ Ä‘áº§u tiÃªn lÃ  mÃ£ lá»—i (in hoa/gáº¡ch dÆ°á»›i) vÃ  tham sá»‘ thá»© hai lÃ  thÃ´ng Ä‘iá»‡p chi tiáº¿t
        if (code != "ERROR" && !string.IsNullOrEmpty(message) && message.All(c => char.IsUpper(c) || c == '_') && (code.Contains(' ') || code.Any(c => c > 127)))
        {
            return new(false, default, new ApiError(message, code));
        }

        return new(false, default, new ApiError(code, message));
    }
}

public record ApiError
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    public ApiError(string code, string message)
    {
        Code = code;
        Message = message;
    }
}