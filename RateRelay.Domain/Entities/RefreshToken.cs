using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("refresh_tokens")]
[Index(nameof(Token), IsUnique = true)]
public class RefreshTokenEntity : BaseEntity
{
    public required string Token { get; set; }
    public DateTime ExpirationDate { get; set; }
    public long AccountId { get; set; }

    public virtual AccountEntity Account { get; set; }
}