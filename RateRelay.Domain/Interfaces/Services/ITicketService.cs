using RateRelay.Domain.Common;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Interfaces;

public interface ITicketService
{
    Task<TicketEntity> CreateTicketAsync(
        TicketType type,
        string title,
        string description,
        long reporterId,
        string? internalNotes = null,
        long? assignedToId = null,
        TicketSubjects? subjects = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> UpdateTicketStatusAsync(
        long ticketId,
        TicketStatus newStatus,
        long changedById,
        string? changeReason = null,
        CancellationToken cancellationToken = default
    );
    
    Task<bool> AssignTicketAsync(
        long ticketId,
        long? newAssignedToId,
        long changedById,
        string? internalNotes = null,
        CancellationToken cancellationToken = default
    );
    
    Task<TicketCommentEntity> GetTicketCommentByIdAsync(
        long commentId,
        long userId,
        bool includeAuthor = false,
        CancellationToken cancellationToken = default
    );
    
    Task<List<TicketCommentEntity>> GetTicketCommentsAsync(
        long ticketId,
        long userId,
        bool includeInternal = false,
        CancellationToken cancellationToken = default
    );
    
    Task<TicketCommentEntity> AddCommentAsync(
        long ticketId,
        long authorId,
        string content,
        bool isInternal = false,
        CancellationToken cancellationToken = default
    );
    
    Task<TicketEntity?> GetTicketByIdAsync(
        long ticketId,
        long userId,
        bool includeComments = false,
        bool includeHistory = false,
        CancellationToken cancellationToken = default
    );
    
    Task<bool> CanUserAccessTicketAsync(
        long ticketId,
        long userId,
        bool isAdmin = false,
        CancellationToken cancellationToken = default
    );

    Task<bool> MarkTicketObsoleteAsync(
        long ticketId,
        long changedById,
        string reason,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<TicketEntity>> GetTicketsByUserAsync(
        long userId,
        bool includeAssigned = true,
        bool includeReported = true,
        TicketStatus? statusFilter = null,
        TicketType? typeFilter = null,
        CancellationToken cancellationToken = default
    );
    
    Task<PagedResult<TicketEntity>> GetPagedUserTicketsAsync(
        int pageNumber,
        int pageSize,
        long userId,
        TicketStatus? statusFilter = null,
        TicketType? typeFilter = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsUserOnTicketCooldownAsync(
        long userId,
        TicketType type,
        CancellationToken cancellationToken = default
    );
}