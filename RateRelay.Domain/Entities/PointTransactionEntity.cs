using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Enums;

namespace RateRelay.Domain.Entities;

[Table("point_transactions")]
[Index(nameof(AccountId))]
[Index(nameof(TransactionType))]
public class PointTransactionEntity : BaseEntity
{
    public long AccountId { get; set; }

    [ForeignKey("AccountId")]
    public virtual AccountEntity Account { get; set; }

    public int Amount { get; set; }

    [MaxLength(50)]
    public PointTransactionType TransactionType { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    public DateTime TransactionDateUtc { get; set; } = DateTime.UtcNow;
}