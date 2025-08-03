using MediatR;
using RateRelay.Application.DTOs.Admin.Business.Queries.GetBusinessesForAdmin;
using RateRelay.Application.DTOs.Admin.Business.Queries.GetBusinessForAdmin;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Admin.Business.Queries.GetBusinessesForAdmin;

public class GetBusinessesForAdminQuery : PagedRequest<AdminBusinessFilterDto>, IRequest<PagedApiResponse<AdminBusinessListDto>>;