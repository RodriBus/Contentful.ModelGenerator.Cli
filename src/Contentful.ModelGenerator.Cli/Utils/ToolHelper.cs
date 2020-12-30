using System.Reflection;

namespace Contentful.ModelGenerator.Cli.Utils
{
    internal static class ToolHelper
    {
        public static string GetToolVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        public static string GetToolName()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        public static string GetToolExecutableName()
        {
            // Mathces .csproj <ToolCommandName>
            return "contentful-model-generator";
        }
    }
}