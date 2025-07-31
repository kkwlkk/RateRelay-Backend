using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateRelay.Domain.Enums;

public enum Permission : ulong
{
    [Display(Name = "None")]
    [Description("No permissions")]
    None = 0,

    #region Ticket Permissions

    [Display(Name = "View All Tickets")]
    [Description("View tickets from all users")]
    ViewAllTickets = 1,

    [Display(Name = "Edit All Tickets")]
    [Description("Edit any ticket regardless of ownership")]
    EditAllTickets = 2,

    [Display(Name = "Assign Tickets")]
    [Description("Assign tickets to other users")]
    AssignTickets = 4,

    [Display(Name = "Change Ticket Status")]
    [Description("Change ticket status beyond basic user actions")]
    ChangeTicketStatus = 8,

    [Display(Name = "Add Internal Comments")]
    [Description("Add internal-only comments to tickets")]
    AddInternalComments = 16,

    [Display(Name = "View Internal Ticket Data")]
    [Description("View internal comments and notes")]
    ViewInternalTicketData = 32,

    [Display(Name = "Handle Assigned Tickets")]
    [Description("Work on tickets assigned to you as an agent")]
    HandleAssignedTickets = 64,

    [Display(Name = "Mark Tickets Obsolete")]
    [Description("Mark tickets as obsolete (administrative)")]
    MarkTicketsObsolete = 128,

    [Display(Name = "View Ticket History")]
    [Description("View ticket status change history")]
    ViewTicketHistory = 256,

    [Display(Name = "Delete Tickets")]
    [Description("Delete tickets (administrative)")]
    DeleteTickets = 512,

    #endregion
    
    
    [Display(Name = "View All Businesses")]
    ViewAllBusinesses = 1024,

    [Display(Name = "Manage Business")]
    ManageBusinessPriority = 4096,

    [Display(Name = "Create Business")]
    CreateBusiness = 2048,

    [Display(Name = "Delete Business")]
    DeleteBusiness = 16384,
    
    [Display(Name = "View All Users")]
    ViewAllUsers = 8192,
}