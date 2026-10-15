using Microsoft.AspNetCore.Mvc;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Users.Commands.CreateUser;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Application.Features.Users.Queries.GetUserById;
using ParkingService.Application.Features.Users.Queries.GetUsers;

namespace ParkingService.Api.Controllers;

/// <summary>
/// Controller for managing Users.
/// </summary>
public class UsersController : BaseApiController
{
    /// <summary>
    /// Thêm người dùng mới vào hệ thống.
    /// </summary>
    /// <param name="command">Thông tin người dùng cần tạo</param>
    /// <returns>Thông tin người dùng vừa được tạo</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Lấy danh sách người dùng với phân trang và bộ lọc.
    /// </summary>
    /// <param name="query">Tham số phân trang và lọc</param>
    /// <returns>Danh sách người dùng phân trang</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetUsersQuery query)
    {
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một người dùng theo Id.
    /// </summary>
    /// <param name="id">Id của người dùng (Guid)</param>
    /// <returns>Thông tin chi tiết người dùng</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id));
        return OkResponse(result);
    }
}
