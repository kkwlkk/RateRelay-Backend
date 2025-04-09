using MediatR;
using RateRelay.Application.DTOs.Queries.Accounts;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Accounts.Queries;

public class AccountsQuery : IRequest<ApiResponse<AccountsQueryResponseDto>>
{
    public long AccountId { get; set; }
}