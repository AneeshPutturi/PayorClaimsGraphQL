namespace PayorClaims.Api.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
}
