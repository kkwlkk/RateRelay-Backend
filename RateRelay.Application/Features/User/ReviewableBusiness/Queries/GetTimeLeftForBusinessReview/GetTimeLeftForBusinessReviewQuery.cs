using MediatR;
using RateRelay.Application.DTOs.ReviewableBusiness.Queries;

namespace RateRelay.Application.Features.ReviewableBusiness.Queries.GetTimeLeftForBusinessReview;

public class GetTimeLeftForBusinessReviewQuery : IRequest<GetTimeLeftForBusinessReviewOutputDto>;