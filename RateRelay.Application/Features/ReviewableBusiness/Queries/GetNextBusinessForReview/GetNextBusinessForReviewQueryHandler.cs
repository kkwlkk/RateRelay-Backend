using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.ReviewableBusiness.Queries;
using RateRelay.Application.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.ReviewableBusiness.Queries.GetNextBusinessForReview;

public class GetNextBusinessForReviewQueryHandler(
    CurrentUserContext currentUserContext,
    IBusinessQueueService businessQueueService,
    IGoogleMapsService googleMapsService,
    IMapper mapper
) : IRequestHandler<GetNextBusinessForReviewQuery, GetNextBusinessForReviewOutputDto>
{
    public async Task<GetNextBusinessForReviewOutputDto> Handle(GetNextBusinessForReviewQuery request,
        CancellationToken cancellationToken)
    {
        if (request.SkipBusinessAssignment)
        {
            await businessQueueService.SkipBusinessAssignmentAsync(
                currentUserContext.AccountId, cancellationToken: cancellationToken);
        }

        var businessToReview = await businessQueueService.GetNextAvailableBusinessForUserAsync(
            currentUserContext.AccountId, cancellationToken: cancellationToken);

        if (businessToReview is null)
        {
            throw new NotFoundException("No business available for review. Please try again later.");
        }

        var businessLockTtl = await businessQueueService.GetAssignedBusinessLockTtlByUserAsync(
            currentUserContext.AccountId, cancellationToken: cancellationToken);
    
        if (businessLockTtl is null)
        {
            throw new NotFoundException("No business available for review. Please try again later.");
        }

        var businessToReviewDto = mapper.Map<GetNextBusinessForReviewOutputDto>(businessToReview);
        businessToReviewDto.InitialReviewTimeInSeconds = (int)TimeSpan.FromMinutes(
            Domain.Constants.BusinessQueueConstants.BusinessLockTimeoutInMinutes).TotalSeconds;
        businessToReviewDto.RemainingReviewTimeInSeconds = (int)businessLockTtl.Value.TotalSeconds;
        businessToReviewDto.MapUrl = googleMapsService.GenerateMapUrlFromCid(businessToReview.Cid);

        return businessToReviewDto;
    }
}