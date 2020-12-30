using CliFx;
using CliFx.Attributes;
using System.Threading.Tasks;

namespace Contentful.ModelGenerator.Cli.Commands
{
    /// <summary>
    /// Generates classes representing Content models.
    /// </summary>
    [Command("generate", Description = "Generates classes representing Content models.")]
    public class GenerateModelsCommand : ICommand
    {
        /// <summary>
        /// The Contentful API key for the Content Delivery API.
        /// </summary>
        [CommandOption("api-key", 'a', Description = "The Contentful API key for the Content Delivery API.", IsRequired = true)]
        public string ApiKey { get; set; }

        /// <summary>
        /// The Space ID to fetch content model from.
        /// </summary>
        [CommandOption("space", 's', Description = "The Space ID to fetch content model from.", IsRequired = true)]
        public string SpaceId { get; set; }

        /// <summary>
        /// The Environment to fetch the content model from.
        /// </summary>
        [CommandOption("environment", 'e', Description = "The Environment to fetch the content model from.", IsRequired = false)]
        public string Environment { get; set; } = "master";

        /// <summary>
        /// The namespace of the generated classes.
        /// </summary>
        [CommandOption("namespace", 'n', Description = "The namespace of the generated classes.", IsRequired = false)]
        public string Namespace { get; set; } = "Contentful.ContentModels";

        /// <summary>
        /// Path to the directory where the files will be created.
        /// </summary>
        [CommandOption("path", 'p', Description = "Path to the directory where the files will be created.", IsRequired = false)]
        public string Path { get; set; } = ".";

        /// <summary>
        /// Overwrite already existing files.
        /// </summary>
        [CommandOption("force", 'f', Description = "Overwrite already existing files.", IsRequired = false)]
        public bool Force { get; set; }

        private Utils.IModelGenerator Generator { get; }

        /// <summary>
        /// Creates an instance.
        /// </summary>
        public GenerateModelsCommand(Utils.IModelGenerator generator)
        {
            Generator = generator;
        }

        /// <summary>
        /// Executes the model generator tool.
        /// </summary>
        public async ValueTask ExecuteAsync(IConsole console)
        {
            var ct = console.GetCancellationToken();
            var options = new Utils.ModelGeneratorOptions
            {
                ApiKey = ApiKey,
                SpaceId = SpaceId,
                Environment = Environment,
                Namepace = Namespace,
                Path = Path,
                Force = Force,
            };
            await Generator.GenerateAsync(options, ct);
        }
    }
}