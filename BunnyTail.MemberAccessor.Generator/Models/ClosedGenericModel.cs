namespace BunnyTail.MemberAccessor.Generator.Models;

using SourceGenerateHelper;

internal sealed record ClosedGenericModel(
    string Namespace,
    string ClassName,
    EquatableArray<string> TypeArguments);
