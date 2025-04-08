using MediatR;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Queries.Demo;

public class DemoQuery : IRequest<ApiResponse<DemoQueryResponse>>
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class DemoQueryResponse
{
    public int Age { get; set; }
}