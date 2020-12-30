using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Contentful.ModelGenerator.Cli.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Contentful.ModelGenerator.Cli.Utils.ModelGeneratorHelper;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Contentful.ModelGenerator.Cli.Utils
{
    /// <summary>
    /// Generates classes based on content models from Contentful.
    /// </summary>
    public class ModelGenerator : IModelGenerator
    {
        private IModelGeneratorReporter Console { get; }
        private HttpClient HttpClient { get; }
        private ModelGeneratorOptions Options { get; set; }
        private IEnumerable<ContentType> ContentTypes { get; set; }

        /// <summary>
        /// Generates classes based on content models from Contentful.
        /// </summary>
        public ModelGenerator(IModelGeneratorReporter console, HttpClient httpClient)
        {
            Console = console;
            HttpClient = httpClient;
        }

        /// <summary>
        /// Generates classes based on content models from Contentful.
        /// </summary>
        public async Task GenerateAsync(ModelGeneratorOptions options, CancellationToken ct = default)
        {
            Options = options;

            Console.ReportDownload("Downloading content types...");

            // Get content types info
            var clientOptions = new ContentfulOptions
            {
                DeliveryApiKey = options.ApiKey,
                SpaceId = options.SpaceId,
                Environment = options.Environment,
            };
            var client = new ContentfulClient(HttpClient, clientOptions);

            ContentTypes = await client.GetContentTypes(ct);

            Console.ReportDownloadComplete("[OK] Download content types.");
            Console.SetClassCount(ContentTypes.Count());
            Console.ReportFolder("Checking destination folder...");

            // Check destination path
            var dir = EnsureDestinationFolder();

            Console.ReportFolderComplete("[OK] Destination folder.");
            Console.ReportClass("Code generation...");

            // Generate compilation unit for each content type
            var toBeWritten = new Dictionary<FileInfo, CompilationUnitSyntax>();
            foreach (var contentType in ContentTypes)
            {
                var className = GetClassName(contentType);
                var fileName = $"{className}.Generated.cs";
                Console.ReportClass(className);

                var file = new FileInfo(Path.Combine(dir.FullName, fileName));
                if (file.Exists && !Options.Force)
                {
                    Console.LogWarning("File '{0}' already exist and will not be overwritten.", fileName);
                    continue;
                }

                var cls = GenerateClassDeclaration(contentType);

                var ns = NamespaceDeclaration(IdentifierName(options.Namepace))
                    .AddMembers(cls);

                var unit = CompilationUnit()
                    .AddMembers(ns)
                    .AddUsings(GetCommonUsings())
                    .WithLeadingTrivia(GetHeaderComment());

                toBeWritten.Add(file, unit);
            }

            Console.ReportClassComplete("[OK] Code generation.");
            Console.SetFileCount(toBeWritten.Count);
            Console.ReportFile("Writing files...");

            // Write files
            foreach (var pair in toBeWritten)
            {
                // Key: FileInfo - Value: CompilationUnit
                var formattedNode = Formatter.Format(pair.Value, new AdhocWorkspace(), options: null, ct);

                Console.ReportFile("Writing " + pair.Key.Name);
                using var writer = pair.Key.CreateText();
                formattedNode.WriteTo(writer);
            }

            Console.ReportFileComplete("[OK] Write files.");

            Console.LogSuccess("Completed! {0} files written.", toBeWritten.Count);
        }

        private DirectoryInfo EnsureDestinationFolder()
        {
            string path;
            if (string.IsNullOrEmpty(Options.Path))
            {
                path = Directory.GetCurrentDirectory();
                Console.Log("No path specified, creating files in current working directory '{0}'.", Options.Path);
            }
            else
            {
                Console.Log("Files will be created at '{0}'.", Options.Path);
                path = Options.Path;
            }

            var dir = new DirectoryInfo(path);

            if (!dir.Exists)
            {
                Console.Log("Path '{0}' does not exist and will be created.", path);
                dir.Create();
            }
            return dir;
        }

        private string GetClassName(ContentType ct)
        {
            // Assume Content Type Id would form like "contentType"
            // Remove invalid file and path chars
            var name = string.Concat(ct.SystemProperties.Id.Split(Path.GetInvalidFileNameChars()));
            name = string.Concat(name.Split(Path.GetInvalidPathChars()));
            return name.FirstCharToUpper();
        }

        private ClassDeclarationSyntax GenerateClassDeclaration(ContentType ct)
        {
            var cls = ClassDeclaration(GetClassName(ct))
               .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword));

            IncludeFieldDefinitions(ref cls, ct);
            return cls;
        }

        private void IncludeFieldDefinitions(ref ClassDeclarationSyntax cls, ContentType ct)
        {
            cls = cls.AddMembers(GetIdConstantDeclaration(ct.SystemProperties.Id));
            cls = cls.AddMembers(GetSystemFieldDeclaration());
            var methodList = new List<MethodDeclarationSyntax>();
            foreach (var field in ct.Fields)
            {
                AddClassField(ref cls, field, out var renderMethod);
                if (renderMethod != null) methodList.Add(renderMethod);
            }

            cls = cls.AddMembers(methodList.ToArray());
        }

        /// <summary>
        /// Generates code like this:<br/>
        /// <c>public object Prop { get; set; }</c>
        /// </summary>
        private void AddClassField(ref ClassDeclarationSyntax cls, Field field, out MethodDeclarationSyntax renderMethod)
        {
            // public object Prop { get; set; }
            var type = GetDataTypeForField(field);
            var name = field.Id.FirstCharToUpper();
            var hasRenderMethod = type == Types.Document;

            var property = PropertyDeclaration(ParseTypeName(type), name)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
                .AddAttributeLists(GetAutoGeneratedAttribute());

            cls = cls.AddMembers(property);

            renderMethod = hasRenderMethod ? GetRenderMethod(name) : null;
        }

        /// <summary>
        /// Generates code like this:<br/>
        /// <c>public async Task&lt;string&gt; RenderProp() => return await new HtmlRenderer().ToHtml(Prop);</c>
        /// </summary>
        private MethodDeclarationSyntax GetRenderMethod(string fieldName)
        {
            // public async Task<string> RenderProp() => return await new HtmlRenderer().ToHtml(Prop);
            return MethodDeclaration(GenericName(
                        Identifier(Types.Task))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                PredefinedType(
                                    Token(SyntaxKind.StringKeyword))))),
                    ClassConventions.RenderAsyncMethod(fieldName))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.AsyncKeyword))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        AwaitExpression(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ObjectCreationExpression(
                                        IdentifierName(Types.HtmlRenderer))
                                    .WithArgumentList(
                                        ArgumentList()),
                                    IdentifierName(Types.HtmlRendererToHtml)))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(
                                            IdentifierName(fieldName))))))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                .AddAttributeLists(GetAutoGeneratedAttribute());
        }

        private string GetDataTypeForField(Field field) =>
            field.Type switch
            {
                FieldTypes.Symbol => Types.String,
                FieldTypes.Text => Types.String,
                FieldTypes.RichText => Types.Document,
                FieldTypes.Integer => Types.Int,
                FieldTypes.Date => Types.DateTime,
                FieldTypes.Number => Types.Float,
                FieldTypes.Boolean => Types.Bool,
                FieldTypes.Location => Types.Location,
                FieldTypes.Link => GetLinkFieldDataType(field),
                FieldTypes.Array => GetListFieldDataType(field),
                FieldTypes.Object => Types.Object,
                _ => Types.Object,
            };

        private string GetLinkFieldDataType(Field field)
        {
            if (field.LinkType == LinkTypes.Asset)
            {
                return Types.Asset;
            }

            if (field.LinkType == LinkTypes.Entry && field.Validations?.Any(c => c is LinkContentTypeValidator) == true)
            {
                var linkContentTypeValidator = field.Validations.Find(c => c is LinkContentTypeValidator) as LinkContentTypeValidator;

                if (linkContentTypeValidator.ContentTypeIds.Count == 1)
                {
                    return GetDataTypeForContentTypeId(linkContentTypeValidator.ContentTypeIds[0]);
                }
            }
            return Types.Object;
        }

        private string GetDataTypeForContentTypeId(string contentTypeId)
        {
            var contentType = ContentTypes.FirstOrDefault(c => c.SystemProperties.Id == contentTypeId);

            if (contentType == null)
            {
                return Types.Object;
            }

            return GetClassName(contentType);
        }

        private string GetListFieldDataType(Field field)
        {
            if (field.Items.LinkType == LinkTypes.Asset)
            {
                return Types.IEnumerableOf(Types.Asset);
            }

            if (field.Items.Type == LinkTypes.Symbol)
            {
                return Types.IEnumerableOf(Types.String);
            }

            if (field.Items.LinkType == LinkTypes.Entry)
            {
                if (field.Items.Validations?.Any(c => c is LinkContentTypeValidator) == true)
                {
                    var linkContentTypeValidator = field.Items.Validations.Find(c => c is LinkContentTypeValidator) as LinkContentTypeValidator;

                    if (linkContentTypeValidator.ContentTypeIds.Count == 1)
                    {
                        return Types.IEnumerableOf(GetDataTypeForContentTypeId(linkContentTypeValidator.ContentTypeIds[0]));
                    }
                }

                return Types.IEnumerableOf(Types.Object);
            }

            return Types.Object;
        }
    }
}