using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using RateRelay.Application.DTOs.Account.ReviewHistory.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Extensions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.Account.Queries.ReviewHistory;

public class GetAccountReviewHistoryQueryHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper,
    IGoogleMapsService googleMapsService
) : IRequestHandler<GetAccountReviewHistoryQuery, PagedApiResponse<AccountReviewHistoryQueryOutputDto>>
{
    public async Task<PagedApiResponse<AccountReviewHistoryQueryOutputDto>> Handle(GetAccountReviewHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var accountId = currentUserDataResolver.GetAccountId();
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessReviewsRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var businessReviews = businessReviewsRepository.GetBaseQueryable()
            .Where(x => x.ReviewerId == accountId)
            .OrderByDescending(x => x.DateCreatedUtc);

        var response = await businessReviews
            .ProjectTo<AccountReviewHistoryQueryOutputDto>(mapper.ConfigurationProvider)
            .ToPagedApiResponseAsync(request.Page, request.PageSize, cancellationToken: cancellationToken);

        response.Data.ForEach(review => { review.MapUrl = googleMapsService.GenerateMapUrlFromCid(review.Cid); });

        return response;
    }
}