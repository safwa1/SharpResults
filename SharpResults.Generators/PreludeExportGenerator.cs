using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SharpResults.Generators;

[Generator]
public sealed class PreludeExportGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("PreludeExportAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

        // This pipeline will find all classes that are candidates for export.
        // A class is a candidate if it has the [PreludeExport] attribute OR
        // if it contains a method that has the [PreludeExport] attribute.
        var candidateClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                // 1. Predicate: Look for any class or method declaration.
                static (s, _) => s is ClassDeclarationSyntax or MethodDeclarationSyntax,
                // 2. Transform: If the node is a candidate, return the class and a flag
                //    indicating if the class itself has the attribute.
                static (ctx, _) => GetCandidateClassInfo(ctx)
            )
            .Where(static c => c is not null) // Filter out non-candidates
            .Select((c, _) => c!.Value) // Unwrap the tuple
            .Collect(); // Collect all results into an ImmutableArray

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(candidateClasses),
            static (spc, t) => GeneratePreludeFiles(spc, t.Left, t.Right)
        );
    }

    /// <summary>
    /// Transforms a syntax node into a candidate class info tuple.
    /// Returns (ClassSyntax, HasClassAttribute) if the node is a candidate, otherwise null.
    /// </summary>
    private static (ClassDeclarationSyntax Class, bool HasClassAttribute)? GetCandidateClassInfo(GeneratorSyntaxContext context)
    {
        // Case 1: The node is a class. Check if it has the attribute.
        if (context.Node is ClassDeclarationSyntax classSyntax)
        {
            if (HasPreludeExportAttribute(context.SemanticModel, classSyntax.AttributeLists))
            {
                return (classSyntax, true);
            }
            return null;
        }

        // Case 2: The node is a method. Check if it has the attribute.
        if (context.Node is MethodDeclarationSyntax methodSyntax)
        {
            if (HasPreludeExportAttribute(context.SemanticModel, methodSyntax.AttributeLists))
            {
                // The method has the attribute, so its parent class is a candidate.
                // We must also check if the parent class itself has the attribute.
                if (methodSyntax.Parent is ClassDeclarationSyntax parentClassSyntax)
                {
                    bool classHasAttribute = HasPreludeExportAttribute(context.SemanticModel, parentClassSyntax.AttributeLists);
                    return (parentClassSyntax, classHasAttribute);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Helper to check if a list of attributes contains our PreludeExportAttribute.
    /// </summary>
    private static bool HasPreludeExportAttribute(SemanticModel semanticModel, SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attrList in attributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(attr);
                if (symbolInfo.Symbol is IMethodSymbol attributeConstructorSymbol)
                {
                    if (attributeConstructorSymbol.ContainingType.ToDisplayString() == "SharpResults.Core.Attributes.PreludeExportAttribute")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Generates the prelude code for all collected candidate classes.
    /// </summary>
        /// <summary>
    /// Generates the prelude code for all collected candidate classes.
    /// </summary>
    private static void GeneratePreludeFiles(SourceProductionContext context, Compilation compilation,
        ImmutableArray<(ClassDeclarationSyntax Class, bool HasClassAttribute)> candidateInfos)
    {
        // De-duplicate classes and consolidate the 'HasClassAttribute' flag.
        // A class might appear multiple times if it has multiple exported methods.
        var candidatesToProcess = new Dictionary<ClassDeclarationSyntax, bool>();
        foreach (var info in candidateInfos)
        {
            if (candidatesToProcess.TryGetValue(info.Class, out var existingFlag))
            {
                // If we've seen it before, OR the flags. If it was true before, it stays true.
                candidatesToProcess[info.Class] = existingFlag || info.HasClassAttribute;
            }
            else
            {
                candidatesToProcess[info.Class] = info.HasClassAttribute;
            }
        }
        
        foreach (var kvp in candidatesToProcess)
        {
            var cls = kvp.Key;
            var hasClassAttribute = kvp.Value;

            var model = compilation.GetSemanticModel(cls.SyntaxTree);
            if (model.GetDeclaredSymbol(cls) is not INamedTypeSymbol classSymbol)
                continue;

            var builder = new StringBuilder();
            var namespaces = new HashSet<string>
            {
                "SharpResults.Core",
                "SharpResults.Core.Delegates",
                "SharpResults.Core.Types",
                "SharpResults.Types",
            };

            var methods = classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.IsStatic && m.DeclaredAccessibility == Accessibility.Public)
                .ToImmutableArray();

            if (methods.Length == 0)
                continue;

            foreach (var method in methods)
            {
                var name = method.Name;

                var exportAttr = method.GetAttributes()
                    .FirstOrDefault(a =>
                        a.AttributeClass?.ToDisplayString() == "SharpResults.Core.Attributes.PreludeExportAttribute");

                // Only process methods with the attribute or all methods if the class has the attribute
                if (exportAttr == null && !hasClassAttribute)
                    continue;

                var alias = exportAttr?.ConstructorArguments.FirstOrDefault().Value as string;
                var displayName = alias ?? name;

                // Type parameters
                var typeParams = method.TypeParameters.Length > 0
                    ? "<" + string.Join(", ", method.TypeParameters.Select(p => p.Name)) + ">"
                    : "";

                // Parameters
                var paramList = string.Join(", ",
                    method.Parameters.Select(p =>
                    {
                        namespaces.Add(p.Type.ContainingNamespace?.ToDisplayString() ?? "");
                        return $"{p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {p.Name}";
                    }));
                var paramNames = string.Join(", ", method.Parameters.Select(p => p.Name));

                // Return type
                namespaces.Add(method.ReturnType.ContainingNamespace?.ToDisplayString() ?? "");

                // Generic constraints
                var constraints = new StringBuilder();
                foreach (var tp in method.TypeParameters)
                {
                    var constraintClauses = new List<string>();

                    if (tp.HasReferenceTypeConstraint)
                        constraintClauses.Add("class");
                    if (tp.HasValueTypeConstraint)
                        constraintClauses.Add("struct");
                    if (tp.HasUnmanagedTypeConstraint)
                        constraintClauses.Add("unmanaged");
                    if (tp.HasNotNullConstraint)
                        constraintClauses.Add("notnull");
                    if (tp.HasConstructorConstraint)
                        constraintClauses.Add("new()");

                    foreach (var ct in tp.ConstraintTypes)
                    {
                        constraintClauses.Add(ct.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                        namespaces.Add(ct.ContainingNamespace?.ToDisplayString() ?? "");
                    }

                    if (constraintClauses.Count > 0)
                        constraints.AppendLine($"    where {tp.Name} : {string.Join(", ", constraintClauses)}");
                }

                // Check for AggressiveInlining
                bool hasAggressiveInlining = method.GetAttributes()
                    .Any(a => a.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.MethodImplAttribute" &&
                              a.ConstructorArguments.Length == 1 &&
                              a.ConstructorArguments[0].Value?.ToString() == "AggressiveInlining");

                // Check ForceAggressiveInlining from our attribute
                bool forceInlining = false;
                if (exportAttr != null && exportAttr.ConstructorArguments.Length > 1 &&
                    exportAttr.ConstructorArguments[1].Value is bool b)
                    forceInlining = b;

                bool addInlining = hasAggressiveInlining || forceInlining;

                var methodHeader = new StringBuilder();
                if (addInlining)
                {
                    methodHeader.AppendLine("[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
                }

                methodHeader.Append($"public static {method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {displayName}{typeParams}({paramList})");

                builder.AppendLine(methodHeader.ToString());
                builder.AppendLine(constraints.ToString() + "    => " + $"{classSymbol.ToDisplayString()}.{name}{typeParams}({paramNames});");
            }

            if (builder.Length == 0)
                continue;

            var fileName = $"{classSymbol.Name}.Prelude.g.cs";

            // Build using statements
            var usings = string.Join("\n", namespaces
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .OrderBy(n => n)
                .Select(n => $"using {n};"));

            var source = $$"""
// <auto-generated />
{{usings}}

namespace SharpResults;

public static partial class Prelude
{
{{builder}}
}
""";

            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private const string AttributeSource = """
// <auto-generated />
namespace SharpResults.Core.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method)]
    public sealed class PreludeExportAttribute : System.Attribute
    {
        public string? Alias { get; }
        public bool ForceAggressiveInlining { get; }

        public PreludeExportAttribute(string? alias = null, bool forceAggressiveInlining = false)
        {
            Alias = alias;
            ForceAggressiveInlining = forceAggressiveInlining;
        }
    }
}
""";
}