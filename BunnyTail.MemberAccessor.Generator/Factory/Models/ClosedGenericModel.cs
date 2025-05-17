namespace BunnyTail.MemberAccessor.Generator.Factory.Models;

using SourceGenerateHelper;

internal sealed record ClosedGenericModel(
    string Namespace,
    string ClassName,
    EquatableArray<string> TypeArguments);
