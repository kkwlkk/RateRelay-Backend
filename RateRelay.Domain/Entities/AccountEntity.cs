using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RateRelay.Domain.Entities;

[Table("accounts")]
[Index(nameof(Username), IsUnique = true)]
public class AccountEntity : BaseModelEntity
{
    [MaxLength(64)]
    public required string Username { get; set; }
}