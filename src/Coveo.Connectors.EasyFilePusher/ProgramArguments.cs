using CommandLine;

namespace Coveo.Connectors.EasyFilePusher
{
    /// <summary>
    /// Definition of the program input arguments.
    /// </summary>
    internal class ProgramArguments
    {
        [Option(
            'e',
            nameof(environment),
            Required = true,
            HelpText = $"Cloud environment: {nameof(CloudEnvironment.Hipaa)} or {nameof(CloudEnvironment.Prod)}."
        )]
        public CloudEnvironment environment { get; set; }

        [Option(
            'r',
            nameof(region),
            Required = true,
            HelpText = "Cloud region: " + nameof(CloudRegion.UsEast1) + ", " + nameof(CloudRegion.EuWest1) + " or " + nameof(CloudRegion.ApSouthEast2) + "."
        )]
        public CloudRegion region { get; set; }

        [Option('o', nameof(organizationid), Required = true, HelpText = "ID of the organization in which to push documents.")]
        public string organizationid { get; set; } = "";

        [Option('s', nameof(sourceid), Required = true, HelpText = "ID of the source in which to push documents.")]
        public string sourceid { get; set; } = "";

        [Option('k', nameof(apikey), Required = true, HelpText = "API key to use.")]
        public string apikey { get; set; } = "";

        [Option('f', nameof(folder), Required = true, HelpText = "Path of the local folder that contains the documents to index.")]
        public string folder { get; set; } = "";

        [Option(Default = "*", HelpText = "Wildcard expression for which matching files will be pushed. All files are pushed by default.")]
        public string include { get; set; } = "";

        [Option(Default = true, HelpText = "Whether to recursively search in sub-folders for files to push. Sub-folders are searched by default.")]
        public bool recursive { get; set; }

        [Option(Default = 10, HelpText = "How many files to push per batch.")]
        public int batchSize { get; set; }
    }

    /// <summary>
    /// Cloud environments in which documents can be pushed.
    /// </summary>
    internal enum CloudEnvironment
    {
        Hipaa,
        Prod,
    }

    /// <summary>
    /// Cloud regions in which documents can be pushed.
    /// </summary>
    internal enum CloudRegion
    {
        UsEast1,
        EuWest1,
        ApSouthEast2,
    }
}
