using Microsoft.AspNetCore.Mvc;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Cards.Commands.AssignUserToCard;
using ParkingService.Application.Features.Cards.Commands.CreateCard;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Application.Features.Cards.Queries.GetCardById;
using ParkingService.Application.Features.Cards.Queries.GetCards;

namespace ParkingService.Api.Controllers;

public record AssignUserRequest(Guid? UserId);

/// <summary>
/// Controller for managing RFID/Mifare access cards.
/// </summary>
public class CardsController : BaseApiController
{
    /// <summary>
    /// Thêm thẻ mới vào hệ thống (cho phép userId là null).
    /// </summary>
    /// <param name="command">Thông tin thẻ cần tạo</param>
    /// <returns>Thông tin thẻ vừa được tạo</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CardDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCardCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Lấy danh sách thẻ với phân trang và bộ lọc theo UserId hoặc trạng thái.
    /// </summary>
    /// <param name="query">Tham số phân trang và lọc</param>
    /// <returns>Danh sách thẻ phân trang</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<CardDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetCardsQuery query)
    {
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một thẻ theo Id (bao gồm số xe của người dùng để kiểm thử).
    /// </summary>
    /// <param name="id">Id của thẻ (Guid)</param>
    /// <returns>Thông tin chi tiết thẻ</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new GetCardByIdQuery(id));
        return OkResponse(result);
    }

    /// <summary>
    /// Liên kết thẻ với người dùng (1 User - 1 Thẻ) hoặc hủy liên kết (truyền userId = null).
    /// </summary>
    /// <param name="id">Id của thẻ</param>
    /// <param name="request">Chứa UserId cần liên kết</param>
    /// <returns>Thông tin thẻ sau khi liên kết</returns>
    [HttpPut("{id:guid}/assign-user")]
    [ProducesResponseType(typeof(ApiResponse<CardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignUser([FromRoute] Guid id, [FromBody] AssignUserRequest request)
    {
        var result = await Mediator.Send(new AssignUserToCardCommand(id, request.UserId));
        return OkResponse(result);
    }
}
