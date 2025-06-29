using MediatR;
using RateRelay.Application.DTOs.Business.UserBusiness.Queries;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Business.Queries.GetAllUserBusinesses;

public class GetAllUserBusinessesQuery : PagedRequest, IRequest<PagedApiResponse<GetBusinessQueryOutputDto>>;