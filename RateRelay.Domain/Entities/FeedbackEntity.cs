using System.ComponentModel.DataAnnotations.Schema;

namespace RateRelay.Domain.Entities;

[Table("feedbacks")]
public class FeedbackEntity : BaseEntity
{
    public long AccountId { get; set; }

    [ForeignKey("AccountId")]
    public virtual AccountEntity Account { get; set; } = null!;
    
    
}