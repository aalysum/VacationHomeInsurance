using System.ComponentModel.DataAnnotations;


namespace VacationHome.ApiGateway.Configuration;


public sealed class GatewayOptions
{
    public const string SectionName = "Gateway";


    [Required]
    [Range(8, 200)]
    public int MaximumCorrelationIdLength { get; init; } = 100;


    public bool RequireHttps { get; init; } = true;


    public bool IncludeExceptionDetails { get; init; }
}
