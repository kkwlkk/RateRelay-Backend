using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateRelay.Domain.Enums;

public enum Permission : ulong
{
    [Display(Name = "None")]
    [Description("No permissions")]
    None = 0,
}