using AutoMapper;
using MediatR;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;
using RateRelay.Application.Exceptions;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.Business.Commands.InitiateBusinessVerification;

public class InitiateBusinessVerificationCommandHandler(
    CurrentUserContext currentUserContext,
    IBusinessVerificationService businessVerificationService,
    IMapper mapper
) : IRequestHandler<InitiateBusinessVerificationCommand, BusinessVerificationOutputDto>
{
    public async Task<BusinessVerificationOutputDto> Handle(InitiateBusinessVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var verificationResult = await businessVerificationService.InitiateVerificationAsync(
            request.PlaceId,
            currentUserContext.AccountId
        );

        if (!verificationResult.IsSuccess)
        {
            throw new Exception(verificationResult.ErrorMessage);
        }

        if (verificationResult.Verification is null)
        {
            throw new NotFoundException("Verification not found.");
        }

        var businessVerificationOutputDto = mapper.Map<BusinessVerificationOutputDto>(verificationResult.Verification);
        return businessVerificationOutputDto;
    }
}