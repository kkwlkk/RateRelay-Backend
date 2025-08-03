using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RateRelay.Application.DTOs.User.Account.Queries;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Account.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommandHandler (
    CurrentUserContext userContext,
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper
) : IRequestHandler<UpdateAccountSettingsCommand, IActionResult>
{
    public async Task<IActionResult> Handle(UpdateAccountSettingsCommand request, CancellationToken cancellationToken)
    {
        await using var unitOfWork = await unitOfWorkFactory.CreateAsync();
        var accountRepository = unitOfWork.GetRepository<AccountEntity>();
        var account = await accountRepository.GetByIdAsync(userContext.AccountId, cancellationToken);

        if (account is null)
        {
            throw new NotFoundException("Account not found");
        }

        accountRepository.Update(account);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var accountDto = mapper.Map<AccountQueryOutputDto>(account);
        return new OkObjectResult(accountDto);
    }
}