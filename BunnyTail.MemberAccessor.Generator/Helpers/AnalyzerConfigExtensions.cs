namespace BunnyTail.MemberAccessor.Generator.Helpers;

using System.ComponentModel;

using Microsoft.CodeAnalysis.Diagnostics;

internal static class AnalyzerConfigExtensions
{
    public static T GetValue<T>(this AnalyzerConfigOptions options, string key)
    {
        if (options.TryGetValue($"build_property.{key}", out var value))
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)value;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
            {
                return (T)converter.ConvertFrom(value)!;
            }
        }

        return default!;
    }
}
