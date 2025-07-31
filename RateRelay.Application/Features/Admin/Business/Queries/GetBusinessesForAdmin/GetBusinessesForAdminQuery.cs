using MediatR;
using RateRelay.Application.DTOs.Admin.Business;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Admin.Business.Queries.GetBusinessesForAdmin;

public class GetBusinessesForAdminQuery : PagedRequest<AdminBusinessFilterDto>, IRequest<PagedApiResponse<AdminBusinessListDto>>;