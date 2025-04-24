using MediatR;
using RateRelay.Application.DTOs.ReviewableBusiness.Commands;

namespace RateRelay.Application.Features.ReviewableBusiness.Commands.SubmitBusinessReview;

public class SubmitBusinessReviewCommand : IRequest<SubmitBusinessReviewOutputDto>;