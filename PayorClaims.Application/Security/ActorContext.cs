namespace PayorClaims.Application.Security;

/// <summary>
/// Request-scoped actor identity from JWT (or anonymous).
/// Stored in HttpContext.Items under ActorContextKeys.HttpContextItemKey.
/// </summary>
public sealed class ActorContext
{
    public const string HttpContextItemKey = "ActorContext";

    public const string SystemActorType = "System";
    public const string AdminActorType = "Admin";
    public const string AdjusterActorType = "Adjuster";
    public const string ProviderActorType = "Provider";
    public const string MemberActorType = "Member";

    public string ActorType { get; set; } = SystemActorType;
    public Guid? ActorId { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string? Npi { get; set; }
    public Guid? MemberId { get; set; }

    public bool IsAuthenticated => ActorId.HasValue && Roles.Length > 0;
    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsAdjuster => Roles.Contains("Adjuster");
    public bool IsProvider => Roles.Contains("Provider");
    public bool IsMember => Roles.Contains("Member");
}
