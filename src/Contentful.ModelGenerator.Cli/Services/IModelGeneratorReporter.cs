namespace Contentful.ModelGenerator.Cli.Services
{
    /// <summary>
    /// Defines information output contracts.
    /// </summary>
    public interface IModelGeneratorReporter
    {
        /// <summary>
        /// Initializes dependencies.
        /// </summary>
        void EnsureInitialized();

        /// <summary>
        /// Outputs a message.
        /// </summary>
        void Log(string message);

        /// <summary>
        /// Outputs a message.
        /// </summary>
        void Log(string message, params object[] args);

        /// <summary>
        /// Outputs a success message.
        /// </summary>
        public void LogSuccess(string message);

        /// <summary>
        /// Outputs a success message.
        /// </summary>
        public void LogSuccess(string message, params object[] args);

        /// <summary>
        /// Outputs a warning message.
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Outputs a warning message.
        /// </summary>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Set class count for progress information.
        /// </summary>
        void SetClassCount(int count);

        /// <summary>
        /// Set file count for progress information.
        /// </summary>
        void SetFileCount(int count);

        /// <summary>
        /// Outputs a download message.
        /// </summary>
        void ReportDownload(string message);

        /// <summary>
        /// Outputs a download complete message.
        /// </summary>
        void ReportDownloadComplete(string message);

        /// <summary>
        /// Outputs a folder message.
        /// </summary>
        void ReportFolder(string message);

        /// <summary>
        /// Outputs a folder complete message.
        /// </summary>
        void ReportFolderComplete(string message);

        /// <summary>
        /// Outputs a class message.
        /// </summary>
        void ReportClass(string message);

        /// <summary>
        /// Outputs a class complete message.
        /// </summary>
        void ReportClassComplete(string message);

        /// <summary>
        /// Outputs a file message.
        /// </summary>
        void ReportFile(string message);

        /// <summary>
        /// Outputs a file complete message.
        /// </summary>
        void ReportFileComplete(string message);
    }
}