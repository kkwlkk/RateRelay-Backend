using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;
using RateRelay.Domain.Common;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;

namespace RateRelay.Application.Features.User.Business.Queries.GetBusinessReviews;

public class GetBusinessReviewsQueryHandler(
    ICurrentUserDataResolver currentUserDataResolver,
    IUnitOfWorkFactory unitOfWorkFactory,
    IGoogleMapsService googleMapsService,
    IMapper mapper
) : IRequestHandler<GetBusinessReviewsQuery, PagedApiResponse<GetBusinessReviewsQueryOutputDto>>
{
    public async Task<PagedApiResponse<GetBusinessReviewsQueryOutputDto>> Handle(GetBusinessReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var accountId = currentUserDataResolver.GetAccountId();
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();

        var businessRepository = unitOfWork.GetRepository<Domain.Entities.BusinessEntity>();
        var reviewsRepository = unitOfWork.GetRepository<Domain.Entities.BusinessReviewEntity>();

        var business = await businessRepository.GetBaseQueryable()
            .Where(x => x.Id == request.BusinessId && x.OwnerAccountId == accountId)
            .FirstOrDefaultAsync(cancellationToken);

        if (business is null)
            throw new NotFoundException(
                "The requested business could not be found or you do not have permission to access it.");

        var baseQuery = reviewsRepository.GetBaseQueryable()
            .Where(x => x.BusinessId == request.BusinessId);

        if (request.Status.HasValue)
            baseQuery = baseQuery.Where(x => x.Status == request.Status.Value);

        baseQuery = baseQuery
            .OrderBy(x => x.Id)
            .ApplySearch(request, x => x.Comment.Contains(request.Search!) || x.Id.ToString().Contains(request.Search!))
            .ApplySorting(request);
        
        if (request.ReviewId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Id == request.ReviewId.Value);
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var reviewEntities = await baseQuery
            .ApplyPaging(request)
            .Include(x => x.Reviewer)
            .ToListAsync(cancellationToken);

        var reviews = reviewEntities.Select(x =>
        {
            var dto = mapper.Map<GetBusinessReviewsQueryOutputDto>(x);
            if (x.PostedGoogleReview)
                dto.GoogleMapsReviewUrl = googleMapsService.GenerateMapReviewUrl(business.PlaceId, x.Reviewer.GoogleId);
            return dto;
        }).ToList();

        return request.ToPagedResponse(reviews, totalCount);
    }
}