using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Admin.Users;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.Admin.Users.Queries.GetUsersForAdmin;

public class GetUsersForAdminQueryHandler(IUnitOfWorkFactory unitOfWorkFactory)
    : IRequestHandler<GetUsersForAdminQuery, PagedApiResponse<AdminUserListDto>>
{
    public async Task<PagedApiResponse<AdminUserListDto>> Handle(GetUsersForAdminQuery request,
        CancellationToken cancellationToken)
    {
        var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var usersQueryable = accountRepository.GetBaseQueryable();

        if (request.IsVerified.HasValue)
        {
            usersQueryable = usersQueryable.Where(x => (x.OnboardingStep == AccountOnboardingStep.Completed)
                                                       == request.IsVerified.Value);
        }

        usersQueryable = usersQueryable.ApplySearch(request, x =>
            x.Email.Contains(request.Search!) ||
            x.Id.ToString().Contains(request.Search!) ||
            x.GoogleUsername.Contains(request.Search!) ||
            x.GoogleUsername.Contains(request.Search!));

        var totalCount = await usersQueryable.CountAsync(cancellationToken);

        var users = await usersQueryable
            .ApplySorting(request)
            .ApplyPaging(request)
            .ToListAsync(cancellationToken);

        var userDtos = users.Select(user => new AdminUserListDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.GoogleUsername,
            GoogleUsername = user.GoogleUsername,
            IsVerified = user.HasCompletedOnboarding,
            DateCreatedUtc = user.DateCreatedUtc
        }).ToList();

        return request.ToPagedResponse(userDtos, totalCount);
    }
}