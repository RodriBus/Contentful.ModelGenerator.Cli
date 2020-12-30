namespace Contentful.ModelGenerator.Cli.Utils
{
    /// <summary>
    /// Contains options for the model generator.
    /// </summary>
    public class ModelGeneratorOptions
    {
        /// <summary>
        /// Api key for the Contentful API.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The Contentful Space ID.
        /// </summary>
        public string SpaceId { get; set; }

        /// <summary>
        /// The Contentful Environment name.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// The namespace for the generated models.
        /// </summary>
        public string Namepace { get; set; }

        /// <summary>
        /// The path for the generated files.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// If the tool should override the files.
        /// </summary>
        public bool Force { get; set; }
    }
}