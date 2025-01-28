namespace BunnyTail.MemberAccessor.Generator.Models;

using BunnyTail.MemberAccessor.Generator.Helpers;

internal sealed record TypeModel(
    string Namespace,
    string ClassName,
    EquatableArray<PropertyModel> Properties);
