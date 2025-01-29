namespace BunnyTail.MemberAccessor.Generator;

using System.Collections.Immutable;
using System.Linq;
using System.Text;

using BunnyTail.MemberAccessor.Generator.Helpers;
using BunnyTail.MemberAccessor.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class TemplateGenerator : IIncrementalGenerator
{
    private const string GenerateAccessorAttributeName = "BunnyTail.MemberAccessor.GenerateAccessorAttribute";
    private const string TypedAccessorAttributeName = "BunnyTail.MemberAccessor.TypedAccessorAttribute";

    private const string AccessorFactorySuffix = "_AccessorFactory";

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateAccessorAttributeName,
                static (syntax, _) => IsTypeSyntax(syntax),
                static (context, _) => GetTypeModel(context))
            .Collect();

        var closedGenericProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                TypedAccessorAttributeName,
                static (_, _) => true,
                static (context, _) => GetClosedGenericModel(context))
            .Collect();

        context.RegisterImplementationSourceOutput(
            typeProvider.Combine(closedGenericProvider),
            static (context, provider) => Execute(context, provider.Left, provider.Right));
    }

    // ------------------------------------------------------------
    // Parser
    // ------------------------------------------------------------

    private static bool IsTypeSyntax(SyntaxNode syntax) =>
        syntax is ClassDeclarationSyntax;

    private static Result<TypeModel> GetTypeModel(GeneratorAttributeSyntaxContext context)
    {
        var symbol = (INamedTypeSymbol)context.TargetSymbol;

        var ns = String.IsNullOrEmpty(symbol.ContainingNamespace.Name)
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();

        var properties = symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(static x => new PropertyModel(x.Name, x.Type.ToDisplayString()))
            .ToArray();

        return Results.Success(new TypeModel(
            ns,
            symbol.GetClassName(),
            symbol.TypeArguments.Length,
            new EquatableArray<PropertyModel>(properties)));
    }

    private static EquatableArray<Result<ClosedGenericModel>> GetClosedGenericModel(GeneratorAttributeSyntaxContext context)
    {
        var list = new List<Result<ClosedGenericModel>>();
        if (context.TargetSymbol is ISourceAssemblySymbol assemblySymbol)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var attributeData in assemblySymbol.GetAttributes().Where(Predicate))
            {
                list.Add(GetClosedGenericModel(context.TargetNode, null, attributeData));
            }
        }
        else if (context.TargetSymbol is INamedTypeSymbol classSymbol)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var attributeData in classSymbol.GetAttributes().Where(Predicate))
            {
                list.Add(GetClosedGenericModel(context.TargetNode, classSymbol.OriginalDefinition, attributeData));
            }
        }

        return new EquatableArray<Result<ClosedGenericModel>>(list.ToArray());

        bool Predicate(AttributeData attributeData) =>
            attributeData.AttributeClass?.ToDisplayString() == TypedAccessorAttributeName;
    }

    private static Result<ClosedGenericModel> GetClosedGenericModel(SyntaxNode syntax, INamedTypeSymbol? openGenericSymbol, AttributeData attributeData)
    {
        if (attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol symbol)
        {
            return Results.Error<ClosedGenericModel>(null);
        }

        if (!symbol.IsGenericType)
        {
            return Results.Error<ClosedGenericModel>(new DiagnosticInfo(Diagnostics.InvalidTypeArgument, syntax.GetLocation(), symbol.Name));
        }

        if ((openGenericSymbol is not null) &&
            !SymbolEqualityComparer.Default.Equals(openGenericSymbol, symbol.OriginalDefinition))
        {
            return Results.Error<ClosedGenericModel>(new DiagnosticInfo(Diagnostics.InvalidAttributeLocation, syntax.GetLocation(), symbol.Name));
        }

        var ns = String.IsNullOrEmpty(symbol.ContainingNamespace.Name)
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();

        var typeArguments = symbol.TypeArguments.Select(static x => x.ToDisplayString()).ToArray();

        return Results.Success(new ClosedGenericModel(
            ns,
            symbol.GetClassName(),
            new EquatableArray<string>(typeArguments)));
    }

    // ------------------------------------------------------------
    // Generator
    // ------------------------------------------------------------

    private static void Execute(SourceProductionContext context, ImmutableArray<Result<TypeModel>> types, ImmutableArray<EquatableArray<Result<ClosedGenericModel>>> closedGenerics)
    {
        foreach (var info in types.SelectPart(static x => x.Error))
        {
            context.ReportDiagnostic(info);
        }
        foreach (var info in closedGenerics.SelectMany(static x => x.ToArray().SelectPart(static x => x.Error)))
        {
            context.ReportDiagnostic(info);
        }

        var targetTypes = types.SelectPart(static x => x.Value).ToList();
        var closedTypes = closedGenerics.SelectMany(static x => x.ToArray().SelectPart(static x => x.Value)).ToList();

        var builder = new SourceBuilder();
        foreach (var type in targetTypes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            builder.Clear();
            BuildAccessorSource(builder, type);

            var filename = MakeFilename(type.Namespace, type.ClassName);
            var source = builder.ToString();
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }

        builder.Clear();
        BuildRegistrySource(builder, targetTypes, closedTypes);
        context.AddSource(
            "AccessorInitializer.g.cs",
            SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    private static void BuildAccessorSource(SourceBuilder builder, TypeModel type)
    {
        builder.AutoGenerated();
        builder.EnableNullable();
        builder.NewLine();

        var className = $"global::{type.Namespace}.{type.ClassName}";

        // namespace
        if (!String.IsNullOrEmpty(type.Namespace))
        {
            builder.Namespace(type.Namespace);
            builder.NewLine();
        }

        // class
        builder.Indent()
            .Append("internal sealed class ")
            .Append(MakeFactoryName(type))
            .Append(" : BunnyTail.MemberAccessor.IAccessorFactory<")
            .Append(className)
            .Append('>')
            .NewLine();
        builder.BeginScope();

        // property

        // getter
        foreach (var property in type.Properties.ToArray())
        {
            builder
                .Indent()
                .Append("private static readonly Func<object, object?> ");
            AppendObjectGetter(builder, property.Name);
            builder
                .Append(" = static x => ((")
                .Append(className)
                .Append(")x).")
                .Append(property.Name)
                .Append("!;")
                .NewLine();
        }

        builder.NewLine();

        // setter
        foreach (var property in type.Properties.ToArray())
        {
            builder
                .Indent()
                .Append("private static readonly Action<object, object?> ");
            AppendObjectSetter(builder, property.Name);
            builder
                .Append(" = static (x, v) => ((")
                .Append(className)
                .Append(")x).")
                .Append(property.Name)
                .Append(" = (")
                .Append(property.Type)
                .Append(")v!;")
                .NewLine();
        }

        builder.NewLine();

        // getter
        foreach (var property in type.Properties.ToArray())
        {
            builder
                .Indent()
                .Append("private static readonly Func<")
                .Append(className)
                .Append(", ")
                .Append(property.Type)
                .Append("> ");
            AppendTypedGetter(builder, property.Name);
            builder
                .Append(" = static x => x.")
                .Append(property.Name)
                .Append(';')
                .NewLine();
        }

        builder.NewLine();

        // setter
        foreach (var property in type.Properties.ToArray())
        {
            builder
                .Indent()
                .Append("private static readonly Action<")
                .Append(className)
                .Append(", ")
                .Append(property.Type)
                .Append("> ");
            AppendTypedSetter(builder, property.Name);
            builder
                .Append(" = static (x, v) => x.")
                .Append(property.Name)
                .Append(" = v;")
                .NewLine();
        }

        builder.NewLine();

        // method

        // getter
        builder
            .Indent()
            .Append("public Func<object, object?>? CreateGetter(string name)")
            .NewLine();
        builder.BeginScope();

        foreach (var property in type.Properties.ToArray())
        {
            builder
                .Indent()
                .Append("if (name == \"")
                .Append(property.Name)
                .Append("\") return ");
            AppendObjectGetter(builder, property.Name);
            builder
                .Append(';')
                .NewLine();
        }

        builder
            .Indent()
            .Append("return null;")
            .NewLine();
        builder.EndScope();

        builder.NewLine();

        // setter
        builder
            .Indent()
            .Append("public Action<object, object?>? CreateSetter(string name)")
            .NewLine();
        builder.BeginScope();

        foreach (var property in type.Properties.ToArray())
        {
            builder
                .Indent()
                .Append("if (name == \"")
                .Append(property.Name)
                .Append("\") return ");
            AppendObjectSetter(builder, property.Name);
            builder
                .Append(';')
                .NewLine();
        }

        builder
            .Indent()
            .Append("return null;")
            .NewLine();
        builder.EndScope();

        builder.NewLine();

        // getter
        builder
            .Indent()
            .Append("public Func<")
            .Append(className)
            .Append(", TProperty>? CreateGetter<TProperty>(string name)")
            .NewLine();
        builder.BeginScope();

        foreach (var property in type.Properties.ToArray())
        {
            builder
                .Indent()
                .Append("if (name == \"")
                .Append(property.Name)
                .Append("\") return (Func<")
                .Append(className)
                .Append(", TProperty>)(object)");
            AppendTypedGetter(builder, property.Name);
            builder
                .Append(';')
                .NewLine();
        }

        builder
            .Indent()
        .Append("return null;")
        .NewLine();
        builder.EndScope();

        builder.NewLine();

        // setter
        builder
            .Indent()
            .Append("public Action<")
            .Append(className)
            .Append(", TProperty>? CreateSetter<TProperty>(string name)")
            .NewLine();
        builder.BeginScope();

        foreach (var property in type.Properties.ToArray())
        {
            builder
                .Indent()
                .Append("if (name == \"")
                .Append(property.Name)
                .Append("\") return (Action<")
                .Append(className)
                .Append(", TProperty>)(object)");
            AppendTypedSetter(builder, property.Name);
            builder
                .Append(';')
                .NewLine();
        }

        builder
            .Indent()
            .Append("return null;")
            .NewLine();
        builder.EndScope();

        builder.EndScope();
    }

    private static void BuildRegistrySource(SourceBuilder builder, List<TypeModel> types, List<ClosedGenericModel> closedTypes)
    {
        builder.AutoGenerated();
        builder.EnableNullable();
        builder.NewLine();

        // class
        builder
            .Indent()
            .Append("internal static class AccessorFactoryInitializer")
            .NewLine();
        builder.BeginScope();

        // method
        builder
            .Indent()
            .Append("[System.Runtime.CompilerServices.ModuleInitializer]")
            .NewLine();
        builder
            .Indent()
            .Append("public static void Initialize()")
            .NewLine();
        builder.BeginScope();

        foreach (var type in types)
        {
            builder
                .Indent()
                .Append("BunnyTail.MemberAccessor.AccessorRegistry.RegisterFactory(typeof(")
                .Append(MakeRegistryTargetName(type))
                .Append("), typeof(")
                .Append(MakeRegistryFactoryName(type))
                .Append("));")
                .NewLine();
            if (type.TypeArgumentCount > 0)
            {
                var namePart = type.ClassName.AsSpan(0, type.ClassName.IndexOf('<') + 1);
                foreach (var closedType in closedTypes)
                {
                    if ((type.Namespace == closedType.Namespace) &&
                        closedType.ClassName.AsSpan().StartsWith(namePart))
                    {
                        builder
                            .Indent()
                            .Append("BunnyTail.MemberAccessor.AccessorRegistry.RegisterFactory(typeof(")
                            .Append(MakeRegistryTargetName(closedType))
                            .Append("), typeof(")
                            .Append(MakeRegistryFactoryName(closedType))
                            .Append("));")
                            .NewLine();
                    }
                }
            }
        }

        builder.EndScope();

        builder.EndScope();
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static string MakeFactoryName(TypeModel model)
    {
        var index = model.ClassName.IndexOf('<');
        return index < 0
            ? $"{model.ClassName}{AccessorFactorySuffix}"
            : $"{model.ClassName.Substring(0, index)}{AccessorFactorySuffix}{model.ClassName.Substring(index)}";
    }

    private static string MakeRegistryTargetName(TypeModel model)
    {
        var ns = String.IsNullOrEmpty(model.Namespace)
            ? string.Empty
            : $"global::{model.Namespace}.";
        var index = model.ClassName.IndexOf('<');
        return index < 0
            ? $"{ns}{model.ClassName}"
            : $"{ns}{model.ClassName.Substring(0, index)}<{new string(',', model.TypeArgumentCount - 1)}>";
    }

    private static string MakeRegistryFactoryName(TypeModel model)
    {
        var ns = String.IsNullOrEmpty(model.Namespace)
            ? string.Empty
            : $"global::{model.Namespace}.";
        var index = model.ClassName.IndexOf('<');
        return index < 0
            ? $"{ns}{model.ClassName}{AccessorFactorySuffix}"
            : $"{ns}{model.ClassName.Substring(0, index)}{AccessorFactorySuffix}<{new string(',', model.TypeArgumentCount - 1)}>";
    }

    private static string MakeRegistryTargetName(ClosedGenericModel model)
    {
        var ns = String.IsNullOrEmpty(model.Namespace)
            ? string.Empty
            : $"global::{model.Namespace}.";
        return $"{ns}{model.ClassName}";
    }

    private static string MakeRegistryFactoryName(ClosedGenericModel model)
    {
        var ns = String.IsNullOrEmpty(model.Namespace)
            ? string.Empty
            : $"global::{model.Namespace}.";
        var index = model.ClassName.IndexOf('<');
        return $"{ns}{model.ClassName.Substring(0, index)}{AccessorFactorySuffix}{model.ClassName.Substring(index)}";
    }

    private static void AppendObjectGetter(SourceBuilder builder, string name) =>
        builder.Append("Object").Append(name).Append("Getter");

    private static void AppendObjectSetter(SourceBuilder builder, string name) =>
        builder.Append("Object").Append(name).Append("Setter");

    private static void AppendTypedGetter(SourceBuilder builder, string name) =>
        builder.Append("Typed").Append(name).Append("Getter");

    private static void AppendTypedSetter(SourceBuilder builder, string name) =>
        builder.Append("Typed").Append(name).Append("Setter");

    private static string MakeFilename(string ns, string className)
    {
        var buffer = new StringBuilder();

        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className.Replace('<', '[').Replace('>', ']'));
        buffer.Append("_Accessor.g.cs");

        return buffer.ToString();
    }
}
