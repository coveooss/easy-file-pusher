using CommandLine;

namespace Coveo.Connectors.EasyFilePusher
{
    /// <summary>
    /// Definition of the program input arguments.
    /// </summary>
    public class ProgramArguments
    {
        /// <summary>
        /// 
        /// </summary>
        [Option(Required = true, HelpText = "Cloud environment: " + nameof(CloudEnvironment.Hipaa) + ", " + nameof(CloudEnvironment.Prod) + ", " + nameof(CloudEnvironment.QA) + " or " + nameof(CloudEnvironment.Dev) + ".")]
        public CloudEnvironment Environment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Option(Required = true, HelpText = "Cloud region: " + nameof(CloudRegion.UsEast1) + " or " + nameof(CloudRegion.EuWest1) + ".")]
        public CloudRegion Region { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Option(Required = true, HelpText = "ID of the organization in which to push documents.")]
        public string Organization { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        [Option(Required = true, HelpText = "ID of the source in which to push documents.")]
        public string SourceId { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        [Option(Required = true, HelpText = "API key to use.")]
        public string ApiKey { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        [Option(Required = true, HelpText = "Path of the local folder that contains the documents to index.")]
        public string Folder { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        [Option(Default = "*", HelpText = "Wildcard expression for which matching files will be pushed. All files are pushed by default.")]
        public string Include { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        [Option(Default = true, HelpText = "Whether to recursively search in sub-folders for files to push. Sub-folders are searched by default.")]
        public bool Recursive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Option(Default = 10, HelpText = "How many files to push per batch.")]
        public int BatchSize { get; set; }
    }

    /// <summary>
    /// Cloud environments in which documents can be pushed.
    /// </summary>
    public enum CloudEnvironment
    {
        /// <summary>
        /// 
        /// </summary>
        Hipaa,

        /// <summary>
        /// 
        /// </summary>
        Prod,

        /// <summary>
        /// 
        /// </summary>
        QA,

        /// <summary>
        /// 
        /// </summary>
        Dev
    }

    /// <summary>
    /// Cloud regions in which documents can be pushed.
    /// </summary>
    public enum CloudRegion
    {
        /// <summary>
        /// 
        /// </summary>
        UsEast1,

        /// <summary>
        /// 
        /// </summary>
        EuWest1
    }
}
