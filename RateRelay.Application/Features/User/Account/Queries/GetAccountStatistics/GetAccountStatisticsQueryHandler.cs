using MediatR;
using RateRelay.Application.DTOs.User.Account.Queries;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Account.Queries.GetAccountStatistics;

public class GetAccountStatisticsQueryHandler(
    CurrentUserContext userContext,
    IUnitOfWorkFactory unitOfWorkFactory
) : IRequestHandler<GetAccountStatisticsQuery, AccountStatisticsQueryOutputDto>
{
    public async Task<AccountStatisticsQueryOutputDto> Handle(GetAccountStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = userContext.AccountId;
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var businessRepository = uow.GetRepository<BusinessEntity>();
        var ticketRepository = uow.GetRepository<TicketEntity>();
        var businessReviewsRepository = uow.GetRepository<BusinessReviewEntity>();

        var totalBusinesses = await businessRepository.CountAsync(b => b.OwnerAccountId == userId, cancellationToken);
        var totalTickets = await ticketRepository.CountAsync(t => t.ReporterId == userId, cancellationToken);
        var totalAwaitingBusinessReviews = await businessReviewsRepository.CountAsync(
            br => br.Status == BusinessReviewStatus.Pending && br.Business.OwnerAccountId == userId, cancellationToken);
        var totalCompletedBusinessReviews = await businessReviewsRepository.CountAsync(
            br => br.Status == BusinessReviewStatus.Accepted && br.Business.OwnerAccountId == userId,
            cancellationToken);

        return new AccountStatisticsQueryOutputDto
        {
            TotalBusinesses = totalBusinesses,
            TotalTickets = totalTickets,
            TotalAwaitingBusinessReviews = totalAwaitingBusinessReviews,
            TotalCompletedBusinessReviews = totalCompletedBusinessReviews
        };
    }
}