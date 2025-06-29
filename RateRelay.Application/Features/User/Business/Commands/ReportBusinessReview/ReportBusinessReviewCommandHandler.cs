using MediatR;
using RateRelay.Application.DTOs.Business.BusinessReviews.Commands;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Business.Commands.ReportBusinessReview;

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
        var review = await reviewService.GetBusinessReviewAsync(request.ReviewId, cancellationToken);

        if (review is null)
        {
            throw new NotFoundException(
                $"Business review with ID {request.ReviewId} not found.", "BusinessReviewNotFound");
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

        var ticket = await ticketService.CreateTicketAsync(
            TicketType.BusinessReview,
            $"Report Business Review #{request.ReviewId}",
            request.Content,
            currentUserContext.AccountId,
            cancellationToken: cancellationToken
        );

        return new ReportBusinessReviewOutputDto();
    }
}