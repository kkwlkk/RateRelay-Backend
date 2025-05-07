using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.ReviewableBusiness.Commands;
using RateRelay.Application.Exceptions;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.ReviewableBusiness.Commands.SubmitBusinessReview;

public class SubmitBusinessReviewCommandHandler(
    CurrentUserContext currentUserContext,
    IBusinessQueueService businessQueueService,
    IReviewService reviewService,
    IMapper mapper
) : IRequestHandler<SubmitBusinessReviewCommand, SubmitBusinessReviewOutputDto>
{
    public async Task<SubmitBusinessReviewOutputDto> Handle(SubmitBusinessReviewCommand request,
        CancellationToken cancellationToken)
    {
        var assignedBusiness = await businessQueueService.GetUserAssignedBusinessAsync(
            currentUserContext.AccountId, cancellationToken: cancellationToken);

        if (assignedBusiness is null)
        {
            throw new AppException(
                "You are not assigned to any business for review. Please check your account or contact support.");
        }

        var review = await reviewService.GetUserReviewByBusinessIdAsync(
            assignedBusiness.Id, currentUserContext.AccountId, cancellationToken: cancellationToken);

        if (review is not null)
        {
            throw new AppException("You have already submitted a review for this business.");
        }

        var isReviewAdded = await reviewService.AddUserReviewAsync(
            assignedBusiness.Id,
            currentUserContext.AccountId,
            request.Rating,
            request.Comment,
            request.PostedGoogleReview,
            cancellationToken: cancellationToken
        );

        if (!isReviewAdded)
        {
            throw new AppException("Failed to submit your review. Please try again later.");
        }

        var addedReview = await reviewService.GetUserReviewByBusinessIdAsync(
            assignedBusiness.Id, currentUserContext.AccountId, cancellationToken: cancellationToken);

        var reviewOutputDto = mapper.Map<SubmitBusinessReviewOutputDto>(addedReview);

        return reviewOutputDto;
    }
}