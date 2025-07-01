using MediatR;
using RateRelay.Application.DTOs.User.Account.Queries;

namespace RateRelay.Application.Features.Account.Queries.GetAccount;

public class GetAccountQuery : IRequest<AccountQueryOutputDto>;