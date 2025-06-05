using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Account.ReviewHistory.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
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

        var query = businessReviewsRepository.GetBaseQueryable()
            .Where(x => x.ReviewerId == accountId)
            .ApplySearch(request, x =>
                x.Business.BusinessName.Contains(request.Search!) ||
                x.Comment.Contains(request.Search!) ||
                x.Id.ToString().Contains(request.Search!));

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .ProjectTo<AccountReviewHistoryQueryOutputDto>(mapper.ConfigurationProvider)
            .ApplySorting(request)
            .ApplyPaging(request)
            .ToListAsync(cancellationToken);

        reviews.ForEach(review => { review.MapUrl = googleMapsService.GenerateMapUrlFromCid(review.Cid); });

        var response = request.ToPagedResponse(reviews, totalCount);

        return response;
    }
}