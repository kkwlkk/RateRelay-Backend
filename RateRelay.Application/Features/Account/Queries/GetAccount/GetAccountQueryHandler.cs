using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs;
using RateRelay.Application.DTOs.Account.Queries;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Account.Queries.GetAccount;

public class GetAccountQueryHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    IUserService userService,
    IMapper mapper
) : IRequestHandler<GetAccountQuery, AccountQueryOutputDto>
{
    public async Task<AccountQueryOutputDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await userService.GetFullAccountByIdAsync(currentUserDataResolver.GetAccountId());

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