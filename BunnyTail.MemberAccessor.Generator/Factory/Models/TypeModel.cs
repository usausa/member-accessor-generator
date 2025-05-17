namespace BunnyTail.MemberAccessor.Generator.Factory.Models;

using SourceGenerateHelper;

internal sealed record TypeModel(
    string Namespace,
    string ClassName,
    int TypeArgumentCount,
    EquatableArray<PropertyModel> Properties);
