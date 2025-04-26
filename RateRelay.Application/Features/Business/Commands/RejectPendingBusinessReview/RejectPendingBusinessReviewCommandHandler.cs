using MediatR;
using Microsoft.EntityFrameworkCore;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;
using RateRelay.Application.Exceptions;
using RateRelay.Application.Features.Business.Commands.AcceptPendingBusinessReview;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Business.Commands.RejectPendingBusinessReview;

public class RejectPendingBusinessReviewCommandHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory,
    IReviewService reviewService
) : IRequestHandler<RejectPendingBusinessReviewCommand, RejectPendingBusinessReviewOutputDto>
{
    public async Task<RejectPendingBusinessReviewOutputDto> Handle(RejectPendingBusinessReviewCommand request,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var businessRepository = unitOfWork.GetRepository<BusinessEntity>();
        var businessReviewRepository = unitOfWork.GetRepository<BusinessReviewEntity>();

        var businessReview = await businessReviewRepository.GetBaseQueryable()
            .Where(x => x.Id == request.ReviewId && x.Status == BusinessReviewStatus.Pending)
            // .Include(x => x.Business)
            .FirstOrDefaultAsync(cancellationToken);

        if (businessReview is null)
        {
            throw new NotFoundException($"Business review with ID {request.ReviewId} not found.");
        }

        var business = await businessRepository.GetBaseQueryable()
            .Where(x => x.Id == businessReview.BusinessId)
            .FirstOrDefaultAsync(cancellationToken);

        if (business is null)
        {
            throw new NotFoundException($"Business with ID {businessReview.BusinessId} not found.");
        }

        if (business.OwnerAccountId != currentUserContext.AccountId)
        {
            throw new AppException("You do not have permission to reject this review.");
        }

        if (businessReview.Status != BusinessReviewStatus.Pending)
        {
            throw new AppException("This review is already accepted or rejected.");
        }

        var isRejected = await reviewService.RejectUserReviewAsync(
            businessReview.BusinessId,
            currentUserContext.AccountId,
            cancellationToken
        );

        if (!isRejected)
        {
            throw new AppException("Failed to accept the review.");
        }

        var outputDto = new RejectPendingBusinessReviewOutputDto()
        {
            IsRejected = true,
        };

        return outputDto;
    }
}