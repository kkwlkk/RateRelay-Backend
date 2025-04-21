using MediatR;
using RateRelay.Application.DTOs.Account.Queries;

namespace RateRelay.Application.Features.Account.Queries.GetAccount;

public class GetAccountQuery : IRequest<AccountQueryOutputDto>;