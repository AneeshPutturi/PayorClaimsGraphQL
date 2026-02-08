using GraphQL;
using GraphQL.Types;

namespace PayorClaims.Schema;

public static class ResolveContextExtensions
{
    public static T? GetArgument<T>(this IResolveFieldContext context, string name)
    {
        if (context.Arguments?.TryGetValue(name, out var val) != true)
            return default;
        if (val.Value == null)
            return default;
        if (val.Value is T t) return t;
        try
        {
            return (T)Convert.ChangeType(val.Value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    public static T GetArgument<T>(this IResolveFieldContext context, string name, T defaultValue)
    {
        if (context.Arguments?.TryGetValue(name, out var val) != true || val.Value == null)
            return defaultValue;
        if (val.Value is T t) return t;
        try
        {
            return (T)Convert.ChangeType(val.Value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}
