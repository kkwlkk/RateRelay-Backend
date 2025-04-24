using MediatR;
using RateRelay.Application.DTOs.ReviewableBusiness.Queries;

namespace RateRelay.Application.Features.ReviewableBusiness.Queries.GetNextBusinessForReview;

public class GetNextBusinessForReviewQuery : IRequest<GetNextBusinessForReviewOutputDto>;