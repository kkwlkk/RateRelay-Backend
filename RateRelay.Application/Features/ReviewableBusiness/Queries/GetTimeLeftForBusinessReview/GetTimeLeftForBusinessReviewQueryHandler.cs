using MediatR;
using RateRelay.Application.DTOs.ReviewableBusiness.Queries;
using RateRelay.Application.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.ReviewableBusiness.Queries.GetTimeLeftForBusinessReview;

public class GetTimeLeftForBusinessReviewQueryHandler(
    CurrentUserContext currentUserContext,
    IBusinessQueueService businessQueueService
) : IRequestHandler<GetTimeLeftForBusinessReviewQuery, GetTimeLeftForBusinessReviewOutputDto>
{
    public async Task<GetTimeLeftForBusinessReviewOutputDto> Handle(GetTimeLeftForBusinessReviewQuery request,
        CancellationToken cancellationToken)
    {
        var businessLockTtl = await businessQueueService.GetAssignedBusinessLockTtlByUserAsync(
            currentUserContext.AccountId, cancellationToken: cancellationToken);

        if (businessLockTtl is null)
        {
            throw new AppException(
                "You are not assigned to any business for review. Please check your account or contact support.");
        }

        var businessLockTtlDto = new GetTimeLeftForBusinessReviewOutputDto
        {
            RemainingReviewTimeInSeconds = (int)businessLockTtl.Value.TotalSeconds
        };

        return businessLockTtlDto;
    }
}