using MediatR;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;
using RateRelay.Application.DTOs.User.Business.BusinessReviews.Commands;
using RateRelay.Domain.Common;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Business.Commands.ReportBusinessReview;

public class ReportBusinessReviewCommandHandler(
    CurrentUserContext currentUserContext,
    IUnitOfWorkFactory unitOfWorkFactory,
    IReviewService reviewService,
    ITicketService ticketService
) : IRequestHandler<ReportBusinessReviewCommand, ReportBusinessReviewOutputDto>
{
    public async Task<ReportBusinessReviewOutputDto> Handle(ReportBusinessReviewCommand request,
        CancellationToken cancellationToken)
    {
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var review = await reviewService.GetBusinessReviewAsync(request.ReviewId, true, cancellationToken: cancellationToken);

        if (review is null)
        {
            throw new NotFoundException(
                $"Business review with ID {request.ReviewId} not found.", "BusinessReviewNotFound");
        }

        if (review.ReviewerId == currentUserContext.AccountId)
        {
            throw new AppException(
                $"You cannot report your own review with ID {request.ReviewId}.",
                "BusinessReviewCannotReportOwn");
        }

        if (review.Business.OwnerAccountId != currentUserContext.AccountId)
        {
            throw new AppException(
                "You do not have permission to report this review.",
                "BusinessReviewNoPermissionToReport");
        }

        if (review.BusinessId != request.BusinessId)
        {
            throw new AppException(
                $"Business review with ID {request.ReviewId} does not belong to business with ID {request.BusinessId}.",
                "BusinessReviewDoesNotBelongToBusiness");
        }

        if (review.Status is BusinessReviewStatus.UnderDispute)
        {
            throw new AppException(
                $"Business review with ID {request.ReviewId} is already under dispute.",
                "BusinessReviewAlreadyUnderDispute");
        }

        if (review.Status is not BusinessReviewStatus.Pending and not BusinessReviewStatus.Accepted)
        {
            throw new AppException(
                $"Business review with ID {request.ReviewId} cannot be reported.",
                "BusinessReviewCannotBeReported");
        }

        var ticketSubjects = new TicketSubjects
        {
            BusinessId = request.BusinessId,
            ReviewId = request.ReviewId,
        };

        var ticket = await ticketService.CreateTicketAsync(
            TicketType.BusinessReview,
            request.Title,
            request.Content,
            currentUserContext.AccountId,
            subjects: ticketSubjects,
            cancellationToken: cancellationToken
        );

        await reviewService.UpdateReviewStatusAsync(
            request.ReviewId,
            BusinessReviewStatus.UnderDispute,
            cancellationToken
        );

        return new ReportBusinessReviewOutputDto
        {
            TicketId = ticket.Id
        };
    }
}