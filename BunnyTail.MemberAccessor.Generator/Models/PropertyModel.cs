namespace BunnyTail.MemberAccessor.Generator.Models;

internal sealed record PropertyModel(
    string Name,
    string Type,
    bool CanRead,
    bool CanWrite);
