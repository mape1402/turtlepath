using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TurtlePath.Analyzers
{
    /// <summary>
    /// Detects unsafe CId comparisons and assignments between entities with different configured CId value types.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CIdEntityUsageAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Diagnostic id for unsafe CId comparison diagnostics.
        /// </summary>
        public const string ComparisonDiagnosticId = "TP0001";

        /// <summary>
        /// Diagnostic id for unsafe CId assignment diagnostics.
        /// </summary>
        public const string AssignmentDiagnosticId = "TP0002";

        private static readonly DiagnosticDescriptor ComparisonRule = new(
            ComparisonDiagnosticId,
            "Avoid comparing CId values from entities with different configured value types",
            "Comparing '{0}.Id' ({1}) with '{2}.Id' ({3}) can fail at runtime because both values are CId but mask different value types",
            "TurtlePath.Identifier",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor AssignmentRule = new(
            AssignmentDiagnosticId,
            "Avoid assigning CId values from entities with different configured value types",
            "Assigning '{2}.Id' ({3}) to '{0}.Id' ({1}) can fail at runtime because both values are CId but mask different value types",
            "TurtlePath.Identifier",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(ComparisonRule, AssignmentRule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startContext =>
            {
                var cIdConfiguration = CIdConfigurationIndex.Build(startContext.Compilation, startContext.CancellationToken);

                startContext.RegisterSyntaxNodeAction(
                    nodeContext => AnalyzeBinaryExpression(nodeContext, cIdConfiguration),
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression,
                    SyntaxKind.GreaterThanExpression,
                    SyntaxKind.GreaterThanOrEqualExpression,
                    SyntaxKind.LessThanExpression,
                    SyntaxKind.LessThanOrEqualExpression);

                startContext.RegisterSyntaxNodeAction(
                    nodeContext => AnalyzeAssignmentExpression(nodeContext, cIdConfiguration),
                    SyntaxKind.SimpleAssignmentExpression);
            });
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context, CIdConfigurationIndex cIdConfiguration)
        {
            var binary = (BinaryExpressionSyntax)context.Node;

            if (!TryGetMismatchedIds(context.SemanticModel, cIdConfiguration, binary.Left, binary.Right, out var left, out var right))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                ComparisonRule,
                binary.OperatorToken.GetLocation(),
                left.EntityName,
                left.ValueTypeName,
                right.EntityName,
                right.ValueTypeName));
        }

        private static void AnalyzeAssignmentExpression(SyntaxNodeAnalysisContext context, CIdConfigurationIndex cIdConfiguration)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;

            if (!TryGetMismatchedIds(context.SemanticModel, cIdConfiguration, assignment.Left, assignment.Right, out var left, out var right))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                AssignmentRule,
                assignment.OperatorToken.GetLocation(),
                left.EntityName,
                left.ValueTypeName,
                right.EntityName,
                right.ValueTypeName));
        }

        private static bool TryGetMismatchedIds(
            SemanticModel semanticModel,
            CIdConfigurationIndex cIdConfiguration,
            ExpressionSyntax leftExpression,
            ExpressionSyntax rightExpression,
            out EntityIdReference left,
            out EntityIdReference right)
        {
            left = default;
            right = default;

            if (!TryGetEntityIdReference(semanticModel, cIdConfiguration, leftExpression, out left))
                return false;

            if (!TryGetEntityIdReference(semanticModel, cIdConfiguration, rightExpression, out right))
                return false;

            return left.ValueType != null &&
                right.ValueType != null &&
                !SymbolEqualityComparer.Default.Equals(left.ValueType, right.ValueType);
        }

        private static bool TryGetEntityIdReference(
            SemanticModel semanticModel,
            CIdConfigurationIndex cIdConfiguration,
            ExpressionSyntax expression,
            out EntityIdReference reference)
        {
            reference = default;

            var symbol = semanticModel.GetSymbolInfo(expression).Symbol as IPropertySymbol;

            if (symbol == null || symbol.Name != "Id")
                return false;

            var cIdType = semanticModel.Compilation.GetTypeByMetadataName("TurtlePath.Domain.Identifier.CId");

            if (cIdType == null || !SymbolEqualityComparer.Default.Equals(symbol.Type, cIdType))
                return false;

            var entityType = GetReceiverType(semanticModel, expression) ?? symbol.ContainingType;
            var valueType = cIdConfiguration.GetValueType(entityType);

            if (valueType == null)
                return false;

            reference = new EntityIdReference(
                entityType,
                valueType,
                entityType.Name,
                valueType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

            return true;
        }

        private static ITypeSymbol GetReceiverType(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            if (expression is not MemberAccessExpressionSyntax memberAccess)
                return null;

            return semanticModel.GetTypeInfo(memberAccess.Expression).Type;
        }

        private readonly struct EntityIdReference
        {
            public EntityIdReference(ITypeSymbol entity, ITypeSymbol valueType, string entityName, string valueTypeName)
            {
                Entity = entity;
                ValueType = valueType;
                EntityName = entityName;
                ValueTypeName = valueTypeName;
            }

            public ITypeSymbol Entity { get; }

            public ITypeSymbol ValueType { get; }

            public string EntityName { get; }

            public string ValueTypeName { get; }
        }

        private sealed class CIdConfigurationIndex
        {
            private readonly ITypeSymbol defaultValueType;
            private readonly Dictionary<ITypeSymbol, ITypeSymbol> entityValueTypes;

            private CIdConfigurationIndex(
                ITypeSymbol defaultValueType,
                Dictionary<ITypeSymbol, ITypeSymbol> entityValueTypes)
            {
                this.defaultValueType = defaultValueType;
                this.entityValueTypes = entityValueTypes;
            }

            public static CIdConfigurationIndex Build(Compilation compilation, System.Threading.CancellationToken cancellationToken)
            {
                ITypeSymbol defaultValueType = null;
                var entityValueTypes = new Dictionary<ITypeSymbol, ITypeSymbol>(SymbolEqualityComparer.Default);

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var semanticModel = compilation.GetSemanticModel(syntaxTree);
                    var root = syntaxTree.GetRoot(cancellationToken);

                    foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var method = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;

                        if (method == null)
                            continue;

                        if (method.Name == "UseCId" && method.TypeArguments.Length == 2)
                        {
                            defaultValueType = method.TypeArguments[0];
                            continue;
                        }

                        if (method.Name == "UseCIdFor" && method.TypeArguments.Length == 3)
                            entityValueTypes[method.TypeArguments[0]] = method.TypeArguments[1];
                    }
                }

                return new CIdConfigurationIndex(defaultValueType, entityValueTypes);
            }

            public ITypeSymbol GetValueType(ITypeSymbol entityType)
            {
                if (entityValueTypes.TryGetValue(entityType, out var configuredValueType))
                    return configuredValueType;

                return defaultValueType;
            }
        }
    }
}
