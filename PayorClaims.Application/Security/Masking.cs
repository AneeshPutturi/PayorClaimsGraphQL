namespace PayorClaims.Application.Security;

public static class Masking
{
    public static string MaskSsn(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "***-**-****";
        if (value.Length < 4) return "***-**-****";
        var last4 = value.Length >= 4 ? value[^4..] : "****";
        return $"***-**-{last4}";
    }

    public static string MaskPhone(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "***-***-****";
        if (value.Length < 4) return "***-***-****";
        var last4 = value.Length >= 4 ? value[^4..] : "****";
        return $"***-***-{last4}";
    }

    public static string MaskEmail(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "***@***.***";
        var at = value.IndexOf('@');
        if (at <= 0) return "***@***.***";
        var local = value[..at];
        var domain = value[at..];
        var maskedLocal = local.Length > 0 ? local[0] + "***" : "***";
        return maskedLocal + domain;
    }

    /// <summary>Returns masked DOB string for display when not allowed; otherwise pass through.</summary>
    public static string MaskDob() => "****-**-**";
}
