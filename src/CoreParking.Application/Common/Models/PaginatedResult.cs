using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoreParking.Application.Common.Models;

/// <summary>
/// Káº¿t quáº£ phÃ¢n trang tiÃªu chuáº©n phá»¥c vá»¥ báº£ng dá»¯ liá»‡u 10 hoáº·c 20 items / trang.
/// </summary>
public record PaginatedResult<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; init; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; init; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; init; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => PageNumber > 1;

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedResult() { }

    public PaginatedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items ?? new List<T>();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}