using AutoMapper;
using RateRelay.Application.DTOs.Tickets;
using RateRelay.Application.DTOs.Tickets.Commands;
using RateRelay.Application.DTOs.Tickets.Queries;
using RateRelay.Application.Features.Tickets.Commands.AddTicketComment;
using RateRelay.Application.Features.Tickets.Commands.UpdateTicketStatus;
using RateRelay.Application.Features.Tickets.Queries.GetUserTickets;
using RateRelay.Domain.Entities;

namespace RateRelay.Application.Mapping;

public class TicketsProfile : Profile
{
    public TicketsProfile()
    {
        CreateMap<TicketEntity, CreateTicketOutputDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<TicketEntity, GetTicketDetailsOutputDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.DateCreatedUtc))
            .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter.Username))
            .ForMember(dest => dest.AssignedToName,
                opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.Username : string.Empty))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Where(c => !c.IsInternal)))
            .ForMember(dest => dest.StatusHistory, opt => opt.MapFrom(src => src.StatusHistory));

        CreateMap<TicketCommentEntity, TicketCommentDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.DateCreatedUtc))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.DateModifiedUtc))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.Username));

        CreateMap<TicketStatusHistoryEntity, TicketStatusHistoryDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.DateCreatedUtc))
            .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedBy.Username));

        CreateMap<UpdateTicketStatusInputDto, UpdateTicketStatusCommand>()
            .ForMember(dest => dest.TicketId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment));

        CreateMap<AddTicketCommandInputDto, AddTicketCommentCommand>()
            .ForMember(dest => dest.TicketId, opt => opt.Ignore())
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.IsInternal, opt => opt.MapFrom(src => src.IsInternal));

        CreateMap<TicketCommentEntity, AddTicketCommentOutputDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.DateCreatedUtc));
        
        CreateMap<TicketEntity, GetUserTicketsOutputDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.DateCreatedUtc))
            .ForMember(dest => dest.LastActivityUtc, opt => opt.MapFrom(src => src.DateModifiedUtc));
        
        CreateMap<GetUserTicketsInputDto, GetUserTicketsQuery>();
    }
}