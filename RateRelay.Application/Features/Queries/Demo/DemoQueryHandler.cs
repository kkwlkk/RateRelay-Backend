using MediatR;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Queries.Demo;

public class DemoQueryHandler : IRequestHandler<DemoQuery, ApiResponse<DemoQueryResponse>>
{
    public async Task<ApiResponse<DemoQueryResponse>> Handle(DemoQuery request, CancellationToken cancellationToken)
    {
        if (!IsValid(request))
            return ApiResponse<DemoQueryResponse>.ErrorResponse("Invalid request", "INVALID_REQUEST");

        var response = new DemoQueryResponse
        {
            Age = request.Age
        };

        return ApiResponse<DemoQueryResponse>.SuccessResponse(response);
    }

    private bool IsValid(DemoQuery request)
    {
        return !string.IsNullOrEmpty(request.Name) && request.Age > 0;
    }
}