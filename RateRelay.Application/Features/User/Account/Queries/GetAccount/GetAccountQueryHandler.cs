using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs;
using RateRelay.Application.DTOs.User.Account.Queries;
using RateRelay.Application.Features.Account.Queries.GetAccount;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Account.Queries.GetAccount;

public class GetAccountQueryHandler(
    CurrentUserContext userContext,
    IUserService userService,
    IMapper mapper
) : IRequestHandler<GetAccountQuery, AccountQueryOutputDto>
{
    public async Task<AccountQueryOutputDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await userService.GetByIdAsync(userContext.AccountId, cancellationToken);

        if (account is null)
        {
            throw new KeyNotFoundException("Account not found");
        }

        var accountDto = mapper.Map<AccountQueryOutputDto>(account);
        var roleDto = mapper.Map<RoleEntityOutputDto>(account.Role);
        accountDto.Role = roleDto;

        return accountDto;
    }
}