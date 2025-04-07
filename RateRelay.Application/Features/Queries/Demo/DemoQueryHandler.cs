using MediatR;

namespace RateRelay.Application.Features.Queries.Demo;

public class DemoQueryHandler : IRequestHandler<DemoQuery, DemoQueryResponse>
{
    public Task<DemoQueryResponse> Handle(DemoQuery request, CancellationToken cancellationToken)
    {
        var response = new DemoQueryResponse
        {
            Message = $"Hello {request.Name}, you are {request.Age} years old!"
        };

        return Task.FromResult(response);
    }
}