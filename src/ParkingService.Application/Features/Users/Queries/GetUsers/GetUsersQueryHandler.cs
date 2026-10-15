using System.Linq.Expressions;
using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Users.DTOs;
using ParkingService.Domain.Entities;

namespace ParkingService.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResult<UserDto>>
{
    private readonly IRepository<User> _userRepository;

    public GetUsersQueryHandler(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        Expression<Func<User, bool>> predicate = u =>
            (!request.Status.HasValue || u.Status == request.Status.Value) &&
            (string.IsNullOrWhiteSpace(request.SearchTerm) ||
             u.FullName.Contains(request.SearchTerm) ||
             (u.Email != null && u.Email.Contains(request.SearchTerm)) ||
             (u.Phone != null && u.Phone.Contains(request.SearchTerm)) ||
             (u.PlateNumber != null && u.PlateNumber.Contains(request.SearchTerm)));

        var pagedUsers = await _userRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            predicate,
            q => q.OrderByDescending(u => u.CreatedAt),
            cancellationToken);

        return new PaginatedResult<UserDto>
        {
            Items = pagedUsers.Items.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                DateOfBirth = u.DateOfBirth,
                Gender = u.Gender,
                Phone = u.Phone,
                Email = u.Email,
                PlateNumber = u.PlateNumber,
                Status = u.Status,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList(),
            TotalCount = pagedUsers.TotalCount,
            PageNumber = pagedUsers.PageNumber,
            PageSize = pagedUsers.PageSize
        };
    }
}
