using MediatR;
using RateRelay.Application.DTOs.Accounts.Commands;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Features.Accounts.Commands;

public class CreateAccountCommand : IRequest<ApiResponse<CreateAccountCommandResponseDto>>
{
    public string Username { get; set; }
}