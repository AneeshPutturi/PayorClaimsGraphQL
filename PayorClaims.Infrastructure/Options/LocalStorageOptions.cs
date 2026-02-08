namespace PayorClaims.Infrastructure.Options;

public class LocalStorageOptions
{
    public const string SectionName = "Storage";
    public string LocalPath { get; set; } = "App_Data/uploads";
}
