using MediatR;
using RateRelay.Application.DTOs.Account.ReviewHistory.Queries;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Account.Queries.ReviewHistory;

public class GetAccountReviewHistoryQuery : PagedRequest, IRequest<PagedApiResponse<AccountReviewHistoryQueryOutputDto>>;