using MediatR;

namespace RateRelay.Application.Features.Queries.Demo;

public class DemoQuery : IRequest<DemoQueryResponse>
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class DemoQueryResponse
{
    public string Message { get; set; }
}