namespace PayorClaims.Schema.Types.Actors;

public class StaffActorDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string Role { get; set; } = null!;
}
