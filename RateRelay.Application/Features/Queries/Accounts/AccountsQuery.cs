using MediatR;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Queries.Accounts;

public class AccountsQuery : IRequest<ApiResponse<AccountsQueryResponse>>
{
    public long AccountId { get; set; }
}

public class AccountsQueryResponse
{
    public long AccountId { get; set; }
    public string Username { get; set; }
    public DateTime DateCreatedUtc { get; set; }
}