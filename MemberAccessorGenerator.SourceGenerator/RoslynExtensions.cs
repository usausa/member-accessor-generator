namespace MemberAccessorGenerator.SourceGenerator;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class RoslynExtensions
{
    public static string GetClassName(this ClassDeclarationSyntax syntax)
    {
        var identifier = syntax.Identifier.ToString();
        return syntax.TypeParameterList is not null
            ? $"{identifier}<{String.Join(", ", syntax.TypeParameterList.Parameters.Select(p => p.Identifier.ToString()))}>"
            : identifier;
    }
}
