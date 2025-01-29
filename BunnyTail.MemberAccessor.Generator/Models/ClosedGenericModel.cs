namespace BunnyTail.MemberAccessor.Generator.Models;

using BunnyTail.MemberAccessor.Generator.Helpers;

internal sealed record ClosedGenericModel(
    string Namespace,
    string ClassName,
    EquatableArray<string> TypeArguments);
