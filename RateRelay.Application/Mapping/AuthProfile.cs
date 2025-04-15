using AutoMapper;
using RateRelay.Application.DTOs.Auth.Commands;
using RateRelay.Application.Features.Auth.Commands.Google;
using RateRelay.Application.Features.Auth.Commands.RefreshToken;

namespace RateRelay.Application.Mapping;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<GoogleAuthCommand, GoogleAuthInputDto>()
            .ReverseMap();
        CreateMap<AuthOutputDto, GoogleAuthCommand>()
            .ReverseMap();
        CreateMap<RefreshTokenCommand, RefreshTokenInputDto>()
            .ReverseMap();
    }
}