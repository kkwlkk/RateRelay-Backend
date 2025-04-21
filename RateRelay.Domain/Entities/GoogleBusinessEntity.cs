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
    public required string BusinessName { get; set; }
    
    public long OwnerAccountId { get; set; }
    
    [ForeignKey("OwnerAccountId")]
    public AccountEntity? OwnerAccount { get; set; }
    
    public bool IsVerified { get; set; }

    [InverseProperty("Business")]
    public BusinessVerificationEntity? Verification { get; set; }
}