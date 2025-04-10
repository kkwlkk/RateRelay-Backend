using AutoMapper;
using RateRelay.Application.DTOs.Auth.Commands;
using RateRelay.Application.Features.Auth.Commands.Login;
using RateRelay.Application.Features.Auth.Commands.RefreshToken;

namespace RateRelay.Application.Mapping;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<LoginCommand, LoginInputDto>()
            .ReverseMap();
        CreateMap<LoginOutputDto, LoginCommand>()
            .ReverseMap();
        CreateMap<RefreshTokenCommand, RefreshTokenInputDto>()
            .ReverseMap();
    }
}