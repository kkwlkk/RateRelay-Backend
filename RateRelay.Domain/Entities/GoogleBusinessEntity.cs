using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("businesses")]
[Index(nameof(PlaceId), IsUnique = true)]
public class BusinessEntity : BaseEntity
{
    [MaxLength(255)]
    public required string PlaceId { get; set; }

    [MaxLength(255)]
    public required string Cid { get; set; }

    [MaxLength(255)]
    public required string BusinessName { get; set; }

    public long OwnerAccountId { get; set; }

    [ForeignKey("OwnerAccountId")]
    public AccountEntity? OwnerAccount { get; set; }

    public bool IsVerified { get; set; }

    public byte Priority { get; set; }

    [InverseProperty("Business")]
    public BusinessVerificationEntity? Verification { get; set; }
    
    [InverseProperty("Business")]
    public virtual ICollection<BusinessBoostEntity> Boosts { get; set; } = new List<BusinessBoostEntity>();
}