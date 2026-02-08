using GraphQL.Types;

namespace PayorClaims.Schema.Scalars;

public class DecimalGraphType : ScalarGraphType
{
    public DecimalGraphType()
    {
        Name = "Decimal";
        Description = "Decimal number (money, amounts). Serialized as number.";
    }

    public override object? ParseValue(object? value)
    {
        if (value == null) return null;
        if (value is decimal d) return d;
        if (value is int i) return (decimal)i;
        if (value is long l) return (decimal)l;
        if (value is double db) return (decimal)db;
        if (value is string s && decimal.TryParse(s, out var parsed)) return parsed;
        throw new FormatException($"Cannot parse '{value}' as Decimal.");
    }

    public override object? Serialize(object? value)
    {
        if (value == null) return null;
        if (value is decimal d) return d;
        if (value is int or long or double) return Convert.ToDecimal(value);
        return value.ToString();
    }
}
