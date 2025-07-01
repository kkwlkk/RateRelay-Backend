using MediatR;
using RateRelay.Application.DTOs.User.Account.Queries;

namespace RateRelay.Application.Features.User.Account.Queries.GetAccountStatistics;

public class GetAccountStatisticsQuery : IRequest<AccountStatisticsQueryOutputDto>;