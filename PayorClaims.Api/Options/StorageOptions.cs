namespace PayorClaims.Api.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";
    public string LocalPath { get; set; } = "App_Data/uploads";
}
