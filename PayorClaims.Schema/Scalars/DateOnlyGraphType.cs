using GraphQL.Types;

namespace PayorClaims.Schema.Scalars;

public class DateOnlyGraphType : ScalarGraphType
{
    public DateOnlyGraphType()
    {
        Name = "Date";
        Description = "Date (yyyy-MM-dd).";
    }

    public override object? ParseValue(object? value)
    {
        if (value == null) return null;
        if (value is DateOnly d) return d;
        if (value is DateTime dt) return DateOnly.FromDateTime(dt);
        if (value is string s && DateOnly.TryParse(s, out var parsed)) return parsed;
        throw new FormatException($"Cannot parse '{value}' as Date.");
    }

    public override object? Serialize(object? value)
    {
        if (value == null) return null;
        if (value is DateOnly d) return d.ToString("yyyy-MM-dd");
        if (value is DateTime dt) return DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd");
        return value.ToString();
    }
}
