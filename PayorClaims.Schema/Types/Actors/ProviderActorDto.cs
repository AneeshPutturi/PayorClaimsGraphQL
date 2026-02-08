namespace PayorClaims.Schema.Types.Actors;

public class ProviderActorDto
{
    public Guid Id { get; set; }
    public string Npi { get; set; } = null!;
    public string Name { get; set; } = null!;
}
