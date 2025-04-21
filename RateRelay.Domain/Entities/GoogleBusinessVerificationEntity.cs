using System.ComponentModel.DataAnnotations.Schema;

namespace RateRelay.Domain.Entities;

[Table("business_verifications")]
public class BusinessVerificationEntity : BaseEntity
{
    public long BusinessId { get; set; }
    
    [ForeignKey("BusinessId")]
    public BusinessEntity? Business { get; set; }
    
    public DateTime VerificationStartedUtc { get; set; }

    public DateTime? VerificationCompletedUtc { get; set; }

    public DayOfWeek VerificationDay { get; set; }
    
    [Column("verification_opening_time")]
    public TimeSpan VerificationOpeningTime { get; set; }
    
    [Column("verification_closing_time")]
    public TimeSpan VerificationClosingTime { get; set; }
    
    public int VerificationAttempts { get; set; }
    
    [NotMapped]
    public bool IsVerificationExpired => (DateTime.UtcNow - VerificationStartedUtc).TotalDays > 7;
}