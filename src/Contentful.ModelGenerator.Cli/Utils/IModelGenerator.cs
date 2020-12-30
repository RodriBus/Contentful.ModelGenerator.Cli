using System.Threading;
using System.Threading.Tasks;

namespace Contentful.ModelGenerator.Cli.Utils
{
    /// <summary>
    /// Generates classes based on content models from Contentful.
    /// </summary>
    public interface IModelGenerator
    {
        /// <summary>
        /// Generates classes based on content models from Contentful.
        /// </summary>
        public Task GenerateAsync(ModelGeneratorOptions options, CancellationToken ct = default);
    }
}