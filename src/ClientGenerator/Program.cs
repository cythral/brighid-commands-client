using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NJsonSchema.CodeGeneration.CSharp;

using NSwag;
using NSwag.CodeGeneration.CSharp;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#pragma warning disable SA1204, SA1009
namespace Brighid.Commands.ClientGenerator
{
    /// <summary>
    /// Program for generating client and factory classes and interfaces for the Brighid Commands Service.
    /// </summary>
    [Generator]
    public class Program : ISourceGenerator
    {
        private readonly string[] usings = new[]
        {
            "System",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.DependencyInjection.Extensions",
            "Brighid.Commands.Client",
        };

        private readonly string[] ignoredCodes = new string[]
        {
            "CS1591",
            "SA1518",
            "SA1633",
            "SA1600",
            "SA1210",
            "SA1601",
        };

        /// <inheritdoc />
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        /// <inheritdoc />
        public void Execute(GeneratorExecutionContext context)
        {
            ExecuteAsync(context).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes the generator asynchronously.
        /// </summary>
        /// <param name="context">The generator context.</param>
        /// <returns>The resulting task.</returns>
        public async Task ExecuteAsync(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TemplateDirectory", out var templateDirectory);
            if (templateDirectory == null)
            {
                throw new Exception("Template Directory should be defined.");
            }

            var swaggerString = GetSwaggerFile(context);
            var document = await OpenApiDocument.FromJsonAsync(swaggerString);
            var settings = new CSharpClientGeneratorSettings
            {
                GenerateClientClasses = true,
                GenerateClientInterfaces = true,
                ClassName = "{controller}Client",
                OperationNameGenerator = new OperationNameGenerator(),
                CSharpGeneratorSettings =
                {
                    TemplateDirectory = templateDirectory,
                    Namespace = "Brighid.Commands.Client",
                    JsonLibrary = CSharpJsonLibrary.SystemTextJson,
                    PropertyNameGenerator = new PropertyNameGenerator(),
                },
            };

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();
            var extensions = GenerateServiceCollectionExtensions(code);

            context.AddSource("GeneratedClientCode", code);
            context.AddSource("GeneratedExtensions", extensions);
        }

        private string GenerateServiceCollectionExtensions(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var treeRoot = tree.GetCompilationUnitRoot();
            var interfaces = treeRoot.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            var interfaceNames = from iface in interfaces
                                 let name = iface.Identifier.ValueText
                                 where !name.EndsWith("Factory")
                                 select iface.Identifier.ValueText;

            var autoGeneratedTrivia = Comment("// <auto-generated />");
            var codes = ignoredCodes.Select(code => ParseExpression(code));
            var ignoreWarningsTrivia = Trivia(PragmaWarningDirectiveTrivia(Token(DisableKeyword), SeparatedList(codes), true));
            var nullableEnableTrivia = Trivia(NullableDirectiveTrivia(Token(EnableKeyword), true));
            var members = GenerateUseMethods(interfaceNames);
            var classDeclaration = ClassDeclaration("ServiceCollectionExtensions")
                .WithModifiers(TokenList(Token(PublicKeyword), Token(StaticKeyword), Token(PartialKeyword)))
                .WithMembers(List(members));

            var namespaceDeclaration = NamespaceDeclaration(ParseName("Microsoft.Extensions.DependencyInjection"))
                .WithMembers(List(new MemberDeclarationSyntax[] { classDeclaration }));

            var usings = List(this.usings.Select(@using => UsingDirective(ParseName(@using))));
            var compilationUnit = CompilationUnit(List<ExternAliasDirectiveSyntax>(), usings, List<AttributeListSyntax>(), List(new MemberDeclarationSyntax[] { namespaceDeclaration }))
                .WithLeadingTrivia(TriviaList(nullableEnableTrivia, autoGeneratedTrivia, ignoreWarningsTrivia));

            return compilationUnit.NormalizeWhitespace().GetText(Encoding.UTF8).ToString();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateUseMethods(IEnumerable<string> interfaceNames)
        {
            foreach (var interfaceName in interfaceNames)
            {
                var implementationName = interfaceName.Skip(1);
                yield return GenerateUseMethod(interfaceName, string.Join(string.Empty, implementationName));
            }
        }

        private static string GetSwaggerFile(GeneratorExecutionContext context)
        {
            var swaggerFileQuery = from file in context.AdditionalFiles where file.Path.Contains("swagger.json") select file;
            var swaggerFile = swaggerFileQuery.First();
            return swaggerFile.GetText()!.ToString();
        }

        private static MemberDeclarationSyntax GenerateUseMethod(string interfaceName, string implementationName)
        {
            var parameters = SeparatedList(new ParameterSyntax[]
            {
                Parameter(List<AttributeListSyntax>(), TokenList(Token(ThisKeyword)), ParseTypeName("IServiceCollection"), Identifier("services"), null),
                Parameter(List<AttributeListSyntax>(), TokenList(), ParseTypeName("Uri?"), Identifier("baseUri"), EqualsValueClause(Token(EqualsToken), ParseExpression("null"))),
            });

            var clientName = implementationName.Replace("Client", string.Empty);
            clientName = clientName == "Commands" ? string.Empty : clientName;

            var methodName = $"UseBrighidCommands{clientName}";
            return MethodDeclaration(ParseTypeName("void"), methodName)
                .WithModifiers(TokenList(Token(PublicKeyword), Token(StaticKeyword)))
                .WithParameterList(ParameterList(parameters))
                .WithBody(Block(GenerateUseMethodBody(interfaceName, implementationName)));
        }

        private static IEnumerable<StatementSyntax> GenerateUseMethodBody(string interfaceName, string implementationName)
        {
            yield return ParseStatement($"baseUri ??= new Uri(\"https://commands.brigh.id\");");
            yield return ParseStatement($"services.UseBrighidIdentityWithHttp2<{interfaceName}, {implementationName}>(baseUri);");
            yield return ParseStatement($"services.UseBrighidIdentityWithHttp2<{interfaceName}Factory, {implementationName}Factory>(baseUri);");
        }
    }
}
