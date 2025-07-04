using AutoMapper;
using RateRelay.Application.DTOs.Auth.Commands;
using RateRelay.Application.DTOs.User.Auth.Commands;
using RateRelay.Application.Features.Auth.Commands.Google;
using RateRelay.Application.Features.Auth.Commands.RefreshToken;
using RateRelay.Application.Features.User.Auth.Commands.Google;

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