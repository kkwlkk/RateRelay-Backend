using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using RateRelay.Application.DTOs.Business.BusinessVerification.Commands;
using RateRelay.Domain.Constants.ErrorCodes;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Interfaces;
using RateRelay.Infrastructure.Services;

namespace RateRelay.Application.Features.User.Business.Commands.InitiateBusinessVerification;

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
            throw new DomainException(verificationResult.ErrorMessage, verificationResult.ErrorCode,
                verificationResult.Metadata);

        if (verificationResult.Verification is null)
            throw new DomainException("Verification not found", BusinessVerificationErrorCodes.VerificationNotFound,
                StatusCodes.Status404NotFound);

        return mapper.Map<BusinessVerificationOutputDto>(verificationResult.Verification);
    }
}