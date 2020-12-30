using Contentful.ModelGenerator.Cli.Utils;
using Konsole;
using System;

namespace Contentful.ModelGenerator.Cli.Services
{
    internal class ModelGeneratorReporter : IModelGeneratorReporter
    {
        private IConsole Console { get; }
        private ProgressBar DownloadPb { get; set; }
        private ProgressBar FolderPb { get; set; }
        private ProgressBar ClassPb { get; set; }
        private ProgressBar FilePb { get; set; }
        private bool Initialized { get; set; }

        public ModelGeneratorReporter(IConsole console)
        {
            Console = console;
        }

        public void EnsureInitialized()
        {
            // To prevent early inicialization of the box
            // Avoid initialize at constructor
            if (Initialized) return;

            var box = Console.OpenBox($"{ToolHelper.GetToolName()} {ToolHelper.GetToolVersion()}", Console.WindowWidth, 7);

            DownloadPb = new ProgressBar(box, PbStyle.SingleLine, 1);
            FolderPb = new ProgressBar(box, PbStyle.SingleLine, 1);
            ClassPb = new ProgressBar(box, PbStyle.SingleLine, 0);
            FilePb = new ProgressBar(box, PbStyle.SingleLine, 0);
            Initialized = true;
        }

        public void Log(string message)
        {
            Log(message, new object[0]);
        }

        public void Log(string message, params object[] args)
        {
            EnsureInitialized();
            Console.WriteLine(message, args);
        }

        public void LogSuccess(string message)
        {
            LogSuccess(message, new object[0]);
        }

        public void LogSuccess(string message, params object[] args)
        {
            EnsureInitialized();
            Console.WriteLine(ConsoleColor.DarkGreen, message, args);
        }

        public void LogWarning(string message)
        {
            LogWarning(message, new object[0]);
        }

        public void LogWarning(string message, params object[] args)
        {
            EnsureInitialized();
            Console.WriteLine(ConsoleColor.DarkYellow, message, args);
        }

        public void ReportClass(string message)
        {
            EnsureInitialized();
            ClassPb.Next(message);
        }

        public void ReportClassComplete(string message)
        {
            EnsureInitialized();
            ClassPb.Refresh(ClassPb.Max, message);
        }

        public void ReportDownload(string message)
        {
            EnsureInitialized();
            DownloadPb.Next(message);
        }

        public void ReportDownloadComplete(string message)
        {
            EnsureInitialized();
            DownloadPb.Refresh(DownloadPb.Max, message);
        }

        public void ReportFile(string message)
        {
            EnsureInitialized();
            FilePb.Next(message);
        }

        public void ReportFileComplete(string message)
        {
            EnsureInitialized();
            FilePb.Refresh(FilePb.Max, message);
        }

        public void ReportFolder(string message)
        {
            EnsureInitialized();
            FolderPb.Next(message);
        }

        public void ReportFolderComplete(string message)
        {
            EnsureInitialized();
            FolderPb.Refresh(FolderPb.Max, message);
        }

        public void SetClassCount(int count)
        {
            EnsureInitialized();
            ClassPb.Max = count;
        }

        public void SetFileCount(int count)
        {
            EnsureInitialized();
            FilePb.Max = count;
        }
    }
}