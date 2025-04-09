using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Queries.Accounts;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Accounts.Queries;

public class AccountsQueryHandler(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper)
    : IRequestHandler<AccountsQuery, ApiResponse<AccountsQueryResponseDto>>
{
    public async Task<ApiResponse<AccountsQueryResponseDto>> Handle(AccountsQuery request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var account = await accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        
        if (account is null) 
            return ApiResponse<AccountsQueryResponseDto>.ErrorResponse("Account not found", "ACCOUNT_NOT_FOUND");

        var response = mapper.Map<AccountsQueryResponseDto>(account);

        return ApiResponse<AccountsQueryResponseDto>.SuccessResponse(response);
    }
}