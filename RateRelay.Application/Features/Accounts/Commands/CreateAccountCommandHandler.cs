using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Accounts.Commands;
using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;

namespace RateRelay.Application.Features.Accounts.Commands;

public class CreateAccountCommandHandler(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper)
    : IRequestHandler<CreateAccountCommand, ApiResponse<CreateAccountCommandResponseDto>>
{
    public async Task<ApiResponse<CreateAccountCommandResponseDto>> Handle(CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetExtendedRepository<IAccountRepository>();

        var existingAccount = await accountRepository.AccountExistsByUsernameAsync(request.Username);

        if (existingAccount)
            return ApiResponse<CreateAccountCommandResponseDto>.ErrorResponse("Account already exists",
                "ACCOUNT_ALREADY_EXISTS");

        var account = new AccountEntity
        {
            Username = request.Username
        };

        await accountRepository.InsertAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = mapper.Map<CreateAccountCommandResponseDto>(account);

        return ApiResponse<CreateAccountCommandResponseDto>.SuccessResponse(response);
    }
}