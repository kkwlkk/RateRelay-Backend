using MediatR;
using RateRelay.Application.DTOs.User.Business.UserBusiness.Queries;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.User.Business.Queries.GetAllUserBusinesses;

public class GetAllUserBusinessesQuery : PagedRequest, IRequest<PagedApiResponse<GetBusinessQueryOutputDto>>;