using System.Linq.Expressions;
using RateRelay.Domain.Entities;
using RateRelay.Domain.Enums;
using RateRelay.Domain.Exceptions;
using RateRelay.Domain.Extensions;
using RateRelay.Domain.Extensions.Account;
using RateRelay.Domain.Interfaces;
using RateRelay.Domain.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Common;
using RateRelay.Domain.Constants;

namespace RateRelay.Infrastructure.Services;

public class TicketService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IUserService userService
) : ITicketService
{
    public async Task<TicketEntity> CreateTicketAsync(TicketType type, string title, string description,
        long reporterId, string? internalNotes = null, long? assignedToId = null,
        TicketSubjects? subjects = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(title) || title.Length > typeof(TicketEntity).GetMaxLength(nameof(TicketEntity.Title)))
            throw new AppException("Title cannot be empty or exceed maximum length.", nameof(title));

        if (string.IsNullOrEmpty(description) ||
            description.Length > typeof(TicketEntity).GetMaxLength(nameof(TicketEntity.Description)))
            throw new AppException("Description cannot be empty or exceed maximum length.", nameof(description));

        if (!string.IsNullOrEmpty(internalNotes))
        {
            var user = await userService.GetByIdAsync(reporterId, cancellationToken);
            if (!user.HasPermission(Permission.AddInternalComments))
                throw new ForbiddenException("User does not have permission to add internal notes.");
        }

        var ticket = new TicketEntity
        {
            Type = type,
            Title = title,
            Description = description,
            ReporterId = reporterId,
            InternalNotes = internalNotes ?? string.Empty,
            AssignedToId = assignedToId
        };

        subjects?.ApplyTo(ticket);

        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();

        await ticketRepository.InsertAsync(ticket, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return ticket;
    }

    public async Task<bool> UpdateTicketStatusAsync(long ticketId, TicketStatus newStatus, long changedById,
        string? changeReason = null,
        CancellationToken cancellationToken = default)
    {
        if (newStatus == TicketStatus.Obsolete)
            throw new AppException(
                "Cannot update ticket status to Obsolete. Use the appropriate method to mark a ticket as obsolete.",
                "TicketCannotSetObsolete");

        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();
        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);

        if (ticket == null)
            return false;

        var user = await userService.GetByIdAsync(changedById, cancellationToken);

        var isOwner = ticket.ReporterId == changedById;
        var isAssigned = ticket.AssignedToId == changedById;
        var hasElevatedPermission = user.HasPermission(Permission.ChangeTicketStatus);
        var canHandleAssigned = user.HasPermission(Permission.HandleAssignedTickets) && isAssigned;

        if (!isOwner && !canHandleAssigned && !hasElevatedPermission)
            throw new ForbiddenException("User does not have permission to change this ticket's status.");

        var ticketStatusHistoryRepository = uow.GetRepository<TicketStatusHistoryEntity>();

        var statusHistory = new TicketStatusHistoryEntity
        {
            TicketId = ticketId,
            FromStatus = ticket.Status,
            ToStatus = newStatus,
            ChangedById = changedById,
            ChangedReason = changeReason
        };

        ticketRepository.Update(ticket);
        ticket.Status = newStatus;
        ticket.LastActivityUtc = DateTime.UtcNow;

        ApplyStatusSpecificChanges(ticket, newStatus);

        await ticketStatusHistoryRepository.InsertAsync(statusHistory, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> AssignTicketAsync(long ticketId, long? newAssignedToId, long changedById,
        string? internalNotes = null,
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetByIdAsync(changedById, cancellationToken);

        if (!user.HasPermission(Permission.AssignTickets))
            throw new ForbiddenException("User does not have permission to assign tickets.");

        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();
        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);

        if (ticket == null)
            return false;

        if (newAssignedToId.HasValue)
        {
            var assignedUser = await userService.GetByIdAsync(newAssignedToId.Value, cancellationToken);
            if (!assignedUser.HasPermission(Permission.HandleAssignedTickets))
                throw new AppException(
                    "User cannot be assigned tickets as they do not have permission to handle them.");
        }

        var previousAssignee = ticket.AssignedToId;
        ticket.AssignedToId = newAssignedToId;
        ticket.LastActivityUtc = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(internalNotes))
        {
            if (!string.IsNullOrEmpty(ticket.InternalNotes))
                ticket.InternalNotes += System.Environment.NewLine;
            ticket.InternalNotes +=
                $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC] Assignment changed by User {changedById}: {internalNotes}";
        }

        ticketRepository.Update(ticket);

        if (previousAssignee != newAssignedToId)
        {
            var commentRepository = uow.GetRepository<TicketCommentEntity>();
            var assignmentComment = new TicketCommentEntity
            {
                TicketId = ticketId,
                AuthorId = changedById,
                Content = newAssignedToId.HasValue
                    ? $"Ticket assigned to User {newAssignedToId.Value}"
                    : "Ticket unassigned",
                IsInternal = true,
                IsSystemGenerated = true
            };
            await commentRepository.InsertAsync(assignmentComment, cancellationToken);
        }

        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TicketCommentEntity> GetTicketCommentByIdAsync(long commentId, long userId,
        bool includeAuthor = false,
        CancellationToken cancellationToken = default)
    {
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var commentRepository = uow.GetRepository<TicketCommentEntity>();

        var query = commentRepository.GetBaseQueryable()
            .Where(c => c.Id == commentId);

        if (includeAuthor)
        {
            query = query.Include(c => c.Author);
        }

        var comment = await query.FirstOrDefaultAsync(cancellationToken);

        if (comment == null)
            throw new NotFoundException($"Comment with ID {commentId} not found.");

        var user = await userService.GetByIdAsync(userId, cancellationToken);
        if (!user.HasPermission(Permission.ViewInternalTicketData) && comment.IsInternal)
        {
            throw new ForbiddenException("User does not have permission to view this comment.");
        }

        return comment;
    }

    public async Task<List<TicketCommentEntity>> GetTicketCommentsAsync(long ticketId, long userId,
        bool includeInternal = false,
        CancellationToken cancellationToken = default)
    {
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var commentRepository = uow.GetRepository<TicketCommentEntity>();

        IQueryable<TicketCommentEntity> query = commentRepository.GetBaseQueryable()
            .Where(c => c.TicketId == ticketId)
            .Include(c => c.Author)
            .OrderByDescending(c => c.DateCreatedUtc);

        if (!includeInternal)
        {
            query = query.Where(c => !c.IsInternal);
        }

        var comments = await query.ToListAsync(cancellationToken);

        if (comments is null || comments.Count == 0)
            return [];

        var user = await userService.GetByIdAsync(userId, cancellationToken);
        if (!user.HasPermission(Permission.ViewInternalTicketData))
        {
            comments = comments.Where(c => !c.IsInternal).ToList();
        }

        return comments;
    }

    public async Task<TicketCommentEntity> AddCommentAsync(long ticketId, long authorId, string content,
        bool isInternal = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(content) ||
            content.Length > typeof(TicketCommentEntity).GetMaxLength(nameof(TicketCommentEntity.Content)))
            throw new AppException("Content cannot be empty or exceed maximum length.", nameof(content));

        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();
        var commentRepository = uow.GetRepository<TicketCommentEntity>();

        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null)
            throw new AppException("Ticket not found.", nameof(ticketId));

        var user = await userService.GetByIdAsync(authorId, cancellationToken);

        var hasAccess = ticket.ReporterId == authorId ||
                        ticket.AssignedToId == authorId ||
                        user.HasPermission(Permission.ViewAllTickets);

        if (!hasAccess)
            throw new ForbiddenException("User does not have access to this ticket.");

        if (isInternal && !user.HasPermission(Permission.AddInternalComments))
            throw new ForbiddenException("User does not have permission to add internal comments.");

        var comment = new TicketCommentEntity
        {
            TicketId = ticketId,
            AuthorId = authorId,
            Content = content,
            IsInternal = isInternal,
            IsSystemGenerated = false
        };

        await commentRepository.InsertAsync(comment, cancellationToken);

        ticket.LastActivityUtc = DateTime.UtcNow;
        ticketRepository.Update(ticket);

        await uow.SaveChangesAsync(cancellationToken);

        return comment;
    }

    public async Task<TicketEntity?> GetTicketByIdAsync(long ticketId, long userId, bool includeComments = false,
        bool includeHistory = false,
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetByIdAsync(userId, cancellationToken);

        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();

        var ticket = await ticketRepository.GetBaseQueryable()
            .Where(t => t.Id == ticketId)
            .Select(t => new { t.ReporterId, t.AssignedToId })
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket == null)
            return null;

        var isOwner = ticket.ReporterId == userId;
        var isAssigned = ticket.AssignedToId == userId;
        var hasViewAllPermission = user.HasPermission(Permission.ViewAllTickets);

        if (!isOwner && !isAssigned && !hasViewAllPermission)
            throw new ForbiddenException("User does not have access to this ticket.");

        IQueryable<TicketEntity> query = ticketRepository.GetBaseQueryable()
            .Include(t => t.Reporter)
            .Include(t => t.AssignedTo);

        if (includeComments)
        {
            query = query.Include(t => t.Comments)
                .ThenInclude(c => c.Author);
        }

        if (includeHistory)
        {
            if (!user.HasPermission(Permission.ViewTicketHistory))
                throw new ForbiddenException("User does not have permission to view ticket history.");

            query = query.Include(t => t.StatusHistory)
                .ThenInclude(h => h.ChangedBy);
        }

        var result = await query.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (result != null && includeComments && !user.HasPermission(Permission.ViewInternalTicketData))
        {
            result.Comments = result.Comments?.Where(c => !c.IsInternal).ToList();
        }

        return result;
    }

    public async Task<bool> CanUserAccessTicketAsync(long ticketId, long userId, bool isAdmin = false,
        CancellationToken cancellationToken = default)
    {
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();

        var ticket = await ticketRepository.GetBaseQueryable()
            .Where(t => t.Id == ticketId)
            .Select(t => new { t.ReporterId, t.AssignedToId })
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket == null)
            return false;

        if (isAdmin)
            return true;

        var isOwner = ticket.ReporterId == userId;
        var isAssigned = ticket.AssignedToId == userId;

        if (isOwner || isAssigned)
            return true;

        var user = await userService.GetByIdAsync(userId, cancellationToken);
        return user.HasPermission(Permission.ViewAllTickets);
    }

    public async Task<bool> MarkTicketObsoleteAsync(long ticketId, long changedById, string reason,
        CancellationToken cancellationToken = default)
    {
        var user = await userService.GetByIdAsync(changedById, cancellationToken);

        if (!user.HasPermission(Permission.MarkTicketsObsolete))
            throw new ForbiddenException("User does not have permission to mark tickets as obsolete.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new AppException("Reason is required when marking a ticket as obsolete.", nameof(reason));

        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();
        var ticketStatusHistoryRepository = uow.GetRepository<TicketStatusHistoryEntity>();

        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket == null)
            return false;

        if (ticket.IsObsolete)
            throw new AppException("Ticket is already marked as obsolete.", "TicketAlreadyObsolete");

        var statusHistory = new TicketStatusHistoryEntity
        {
            TicketId = ticketId,
            FromStatus = ticket.Status,
            ToStatus = TicketStatus.Obsolete,
            ChangedById = changedById,
            ChangedReason = reason
        };

        ticket.Status = TicketStatus.Obsolete;
        ticket.LastActivityUtc = DateTime.UtcNow;
        ticket.DateClosedUtc ??= DateTime.UtcNow;

        ticketRepository.Update(ticket);
        await ticketStatusHistoryRepository.InsertAsync(statusHistory, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<TicketEntity>> GetTicketsByUserAsync(long userId, bool includeAssigned = true,
        bool includeReported = true, TicketStatus? statusFilter = null, TicketType? typeFilter = null,
        CancellationToken cancellationToken = default)
    {
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();

        var query = ticketRepository.GetBaseQueryable()
            .Include(t => t.Reporter)
            .Include(t => t.AssignedTo)
            .Where(t => t.Status != TicketStatus.Obsolete);

        var conditions = new List<Expression<Func<TicketEntity, bool>>>();

        if (includeReported)
            conditions.Add(t => t.ReporterId == userId);

        if (includeAssigned)
            conditions.Add(t => t.AssignedToId == userId);

        if (conditions.Any())
        {
            var combinedCondition = conditions.Aggregate((expr1, expr2) =>
                Expression.Lambda<Func<TicketEntity, bool>>(
                    Expression.OrElse(expr1.Body, expr2.Body), expr1.Parameters));
            query = query.Where(combinedCondition);
        }

        if (statusFilter.HasValue)
            query = query.Where(t => t.Status == statusFilter.Value);

        if (typeFilter.HasValue)
            query = query.Where(t => t.Type == typeFilter.Value);

        return await query.OrderByDescending(t => t.LastActivityUtc).ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<TicketEntity>> GetPagedUserTicketsAsync(int pageNumber, int pageSize,
        long userId, TicketStatus? statusFilter = null,
        TicketType? typeFilter = null, CancellationToken cancellationToken = default)
    {
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();

        var query = ticketRepository.GetBaseQueryable()
            .Include(t => t.Reporter)
            .Include(t => t.AssignedTo)
            .Where(t => t.ReporterId == userId || t.AssignedToId == userId)
            .Where(t => t.Status != TicketStatus.Obsolete);

        if (statusFilter.HasValue)
            query = query.Where(t => t.Status == statusFilter.Value);

        if (typeFilter.HasValue)
            query = query.Where(t => t.Type == typeFilter.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var tickets = await query
            .OrderByDescending(t => t.LastActivityUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TicketEntity>
        {
            Items = tickets,
            TotalCount = totalCount,
            CurrentPage = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> IsUserOnTicketCooldownAsync(long userId, TicketType type,
        CancellationToken cancellationToken = default)
    {
        await using var uow = await unitOfWorkFactory.CreateAsync();
        var ticketRepository = uow.GetRepository<TicketEntity>();

        const int cooldownPeriod = TicketConstants.CooldownPeriodSeconds;
        var lastTicket = await ticketRepository.GetBaseQueryable()
            .Where(t => t.ReporterId == userId && t.Type == type &&
                        t.DateCreatedUtc > DateTime.UtcNow.AddSeconds(-cooldownPeriod))
            .OrderByDescending(t => t.DateCreatedUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return lastTicket != null;
    }

    private static void ApplyStatusSpecificChanges(TicketEntity ticket, TicketStatus newStatus)
    {
        switch (newStatus)
        {
            case TicketStatus.Resolved:
                ticket.DateResolvedUtc = DateTime.UtcNow;
                break;
            case TicketStatus.Closed:
                ticket.DateClosedUtc = DateTime.UtcNow;
                ticket.DateResolvedUtc ??= DateTime.UtcNow;
                break;
            case TicketStatus.Reopened:
                ticket.DateResolvedUtc = null;
                ticket.DateClosedUtc = null;
                break;
            case TicketStatus.InProgress:
                ticket.DateStartedUtc ??= DateTime.UtcNow;
                break;
        }
    }
}