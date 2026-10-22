namespace VacationHome.ApiGateway.Configuration;


public sealed class CorsOptions
{
    public const string SectionName = "Cors";


    public const string PolicyName = "GatewayCorsPolicy";


    public string[] AllowedOrigins { get; init; } = [];
}
