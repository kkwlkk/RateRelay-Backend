using MediatR;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Queries.Accounts;

public class AccountsQueryHandler(IUnitOfWorkFactory unitOfWorkFactory)
    : IRequestHandler<AccountsQuery, ApiResponse<AccountsQueryResponse>>
{
    public async Task<ApiResponse<AccountsQueryResponse>> Handle(AccountsQuery request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();

        var account = await accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        
        if (account is null) 
            return ApiResponse<AccountsQueryResponse>.ErrorResponse("Account not found", "ACCOUNT_NOT_FOUND");

        var response = new AccountsQueryResponse
        {
            AccountId = account.Id,
            Username = account.Username,
            DateCreatedUtc = account.DateCreatedUtc,
        };
        
        return ApiResponse<AccountsQueryResponse>.SuccessResponse(response);
    }
}