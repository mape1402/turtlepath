using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using TurtlePath.Analyzers;

namespace TurtlePath.Analyzers.Tests;

public class CIdEntityUsageAnalyzerTests
{
    [Fact]
    public async Task Reports_comparison_between_entities_with_different_configured_CId_value_types()
    {
        var diagnostics = await AnalyzeAsync("""
            using System;
            using TurtlePath.Domain.Contracts;
            using TurtlePath.Domain.Identifier;

            public sealed class Customer : BaseEntity { }
            public sealed class Invoice : BaseEntity { }

            public sealed class IdentifierProfile : CIdProfile
            {
                public override void Configure(CIdProfileBuilder builder)
                {
                    builder.UseCIdFor<Invoice, int, int>(config => { });
                }
            }

            public sealed class Service
            {
                public bool Matches(Customer customer, Invoice invoice)
                    => customer.Id == invoice.Id;
            }

            public static class Registration
            {
                public static void Configure(object builder)
                    => TurtlePathServiceCollectionExtensions.UseCId<Guid, string>(builder, config => { });
            }

            public static class TurtlePathServiceCollectionExtensions
            {
                public static void UseCId<TTarget, TDb>(object builder, Action<object> configure) { }
            }
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(CIdEntityUsageAnalyzer.ComparisonDiagnosticId, diagnostic.Id);
    }

    [Fact]
    public async Task Reports_assignment_between_entities_with_different_configured_CId_value_types()
    {
        var diagnostics = await AnalyzeAsync("""
            using System;
            using TurtlePath.Domain.Contracts;
            using TurtlePath.Domain.Identifier;

            public sealed class Customer : BaseEntity { }
            public sealed class Invoice : BaseEntity { }

            public sealed class IdentifierProfile : CIdProfile
            {
                public override void Configure(CIdProfileBuilder builder)
                {
                    builder.UseCIdFor<Invoice, int, int>(config => { });
                }
            }

            public sealed class Service
            {
                public void Assign(Customer customer, Invoice invoice)
                {
                    customer.Id = invoice.Id;
                }
            }

            public static class Registration
            {
                public static void Configure(object builder)
                    => TurtlePathServiceCollectionExtensions.UseCId<Guid, string>(builder, config => { });
            }

            public static class TurtlePathServiceCollectionExtensions
            {
                public static void UseCId<TTarget, TDb>(object builder, Action<object> configure) { }
            }
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(CIdEntityUsageAnalyzer.AssignmentDiagnosticId, diagnostic.Id);
    }

    [Fact]
    public async Task Does_not_report_when_entities_share_configured_CId_value_type()
    {
        var diagnostics = await AnalyzeAsync("""
            using System;
            using TurtlePath.Domain.Contracts;
            using TurtlePath.Domain.Identifier;

            public sealed class Customer : BaseEntity { }
            public sealed class Order : BaseEntity { }

            public sealed class Service
            {
                public bool Matches(Customer customer, Order order)
                    => customer.Id == order.Id;
            }

            public static class Registration
            {
                public static void Configure(object builder)
                    => TurtlePathServiceCollectionExtensions.UseCId<Guid, string>(builder, config => { });
            }

            public static class TurtlePathServiceCollectionExtensions
            {
                public static void UseCId<TTarget, TDb>(object builder, Action<object> configure) { }
            }
            """);

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        var compilation = CSharpCompilation.Create(
            "AnalyzerTests",
            [CSharpSyntaxTree.ParseText(source)],
            GetReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        var analyzer = new CIdEntityUsageAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static IEnumerable<MetadataReference> GetReferences()
    {
        var trustedPlatformAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));

        return trustedPlatformAssemblies.Append(
            MetadataReference.CreateFromFile(typeof(TurtlePath.Domain.Identifier.CId).Assembly.Location));
    }
}
