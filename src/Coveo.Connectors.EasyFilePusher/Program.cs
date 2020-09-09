using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using CommandLine;
using Coveo.Connectors.Utilities.PlatformSdk;
using Coveo.Connectors.Utilities.PlatformSdk.Config;
using Coveo.Connectors.Utilities.PlatformSdk.Helpers;
using Coveo.Connectors.Utilities.PlatformSdk.Model.Document;
using Coveo.Connectors.Utilities.PlatformSdk.Request;

namespace Coveo.Connectors.EasyFilePusher
{
    /// <summary>
    /// Main class of the program.
    /// </summary>
    public class Program
    {
        private const string INVALID_CLOUD_REGION = "Invalid cloud region.";
        private const string INVALID_CLOUD_ENVIRONMENT = "Invalid cloud environment.";
        private const string HIPAA_INVALID_FOR_EUWEST1 = "The " + nameof(CloudEnvironment.Hipaa) + " environment is not available for the region " + nameof(CloudRegion.EuWest1) + ".";

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="p_Args">Command-line arguments.</param>
        private static void Main(string[] p_Args)
        {
            new Parser(settings => {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = Console.Out;
            }).ParseArguments<ProgramArguments>(p_Args).WithParsed(parsedArgs => {
                IndexFiles(parsedArgs);
            });
        }

        /// <summary>
        /// Indexes the files located in the specified folder.
        /// </summary>
        /// <param name="p_Args">Parsed command-line arguments.</param>
        private static void IndexFiles(ProgramArguments p_Args)
        {
            string folder = Path.GetFullPath(p_Args.Folder);
            if (!folder.EndsWith(Path.DirectorySeparatorChar)) {
                folder += Path.DirectorySeparatorChar;
            }
            Console.WriteLine($"Pushing files \"{p_Args.Include}\" from folder \"{folder}\"...");

            ulong orderingId = RequestOrderingUtilities.CreateOrderingId();

            ICoveoPlatformConfig platformConfig = new CoveoPlatformConfig(GetPushApiUrl(p_Args),  GetPlatformApiUrl(p_Args), p_Args.ApiKey, p_Args.Organization);
            using (ICoveoPlatformClient platformClient = new CoveoPlatformClient(platformConfig)) {
                IList<PushDocument> documentBatch = new List<PushDocument>();
                foreach (FileInfo fileInfo in new DirectoryInfo(folder).EnumerateFiles(p_Args.Include, p_Args.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {
                    if (!fileInfo.FullName.StartsWith(folder)) {
                        throw new Exception("Unexpected file gathered from outside the source folder.");
                    }
                    Console.WriteLine(fileInfo.FullName.Substring(folder.Length));

                    PushDocument document = new PushDocument(new Uri(fileInfo.FullName).AbsoluteUri) {
                        ModifiedDate = fileInfo.LastWriteTimeUtc
                    };
                    PushDocumentHelper.SetBinaryContentFromFileAndCompress(document, fileInfo.FullName);
                    documentBatch.Add(document);

                    if (documentBatch.Count >= p_Args.BatchSize) {
                        // Push this batch of documents.
                        SendBatch(platformClient, documentBatch, p_Args.SourceId, orderingId);
                    }
                }

                // Send the (partial) final batch of documents.
                SendBatch(platformClient, documentBatch, p_Args.SourceId, orderingId);

                // Delete the already indexed files that no longer exist.
                platformClient.DocumentManager.DeleteDocumentsOlderThan(p_Args.SourceId, orderingId, null);
            }
        }

        /// <summary>
        /// Sends a batch of documents to the platform.
        /// </summary>
        /// <param name="p_PlatformClient">The instance of <see cref="ICoveoPlatformClient"/> to use.</param>
        /// <param name="p_DocumentBatch">The batch of documents to send.</param>
        /// <param name="p_SourceId">ID of the source in which to push documents.</param>
        /// <param name="p_OrderingId">The ordering identifier.</param>
        private static void SendBatch(ICoveoPlatformClient p_PlatformClient,
                                      IList<PushDocument> p_DocumentBatch,
                                      string p_SourceId,
                                      ulong p_OrderingId)
        {
            if (p_DocumentBatch.Count == 0) {
                return;
            }

            p_PlatformClient.DocumentManager.AddOrUpdateDocuments(p_SourceId, p_DocumentBatch, p_OrderingId);

            p_DocumentBatch.Clear();
        }

        /// <summary>
        /// Gets the push API endpoint URL to use that matches the command-line arguments specified.
        /// </summary>
        /// <param name="p_Args">Parsed command-line arguments.</param>
        /// <returns>The push API endpoint URL to use.</returns>
        private static string GetPushApiUrl(ProgramArguments p_Args)
        {
            switch (p_Args.Region) {
                case CloudRegion.UsEast1:
                    switch (p_Args.Environment) {
                        case CloudEnvironment.Hipaa:
                            return Constants.Endpoint.UsEast1.HIPAA_PUSH_API_URL;
                        case CloudEnvironment.Prod:
                            return Constants.Endpoint.UsEast1.PROD_PUSH_API_URL;
                        case CloudEnvironment.QA:
                            return Constants.Endpoint.UsEast1.QA_PUSH_API_URL;
                        case CloudEnvironment.Dev:
                            return Constants.Endpoint.UsEast1.DEV_PUSH_API_URL;
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                case CloudRegion.EuWest1:
                    switch (p_Args.Environment) {
                        case CloudEnvironment.Hipaa:
                            throw new InvalidEnumArgumentException(HIPAA_INVALID_FOR_EUWEST1);
                        case CloudEnvironment.Prod:
                            return Constants.Endpoint.EuWest1.PROD_PUSH_API_URL;
                        case CloudEnvironment.QA:
                            return Constants.Endpoint.EuWest1.QA_PUSH_API_URL;
                        case CloudEnvironment.Dev:
                            return Constants.Endpoint.EuWest1.DEV_PUSH_API_URL;
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                default:
                    throw new InvalidEnumArgumentException(INVALID_CLOUD_REGION);
            }
        }

        /// <summary>
        /// Gets the platform endpoint URL to use that matches the command-line arguments specified.
        /// </summary>
        /// <param name="p_Args">Parsed command-line arguments.</param>
        /// <returns>The platform endpoint URL to use.</returns>
        private static string GetPlatformApiUrl(ProgramArguments p_Args)
        {
            switch (p_Args.Region) {
                case CloudRegion.UsEast1:
                    switch (p_Args.Environment) {
                        case CloudEnvironment.Hipaa:
                            return Constants.PlatformEndpoint.UsEast1.HIPAA_PLATFORM_API_URL;
                        case CloudEnvironment.Prod:
                            return Constants.PlatformEndpoint.UsEast1.PROD_PLATFORM_API_URL;
                        case CloudEnvironment.QA:
                            return Constants.PlatformEndpoint.UsEast1.QA_PLATFORM_API_URL;
                        case CloudEnvironment.Dev:
                            return Constants.PlatformEndpoint.UsEast1.DEV_PLATFORM_API_URL;
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                case CloudRegion.EuWest1:
                    switch (p_Args.Environment) {
                        case CloudEnvironment.Hipaa:
                            throw new InvalidEnumArgumentException(HIPAA_INVALID_FOR_EUWEST1);
                        case CloudEnvironment.Prod:
                            return Constants.PlatformEndpoint.EuWest1.PROD_PLATFORM_API_URL;
                        case CloudEnvironment.QA:
                            return Constants.PlatformEndpoint.EuWest1.QA_PLATFORM_API_URL;
                        case CloudEnvironment.Dev:
                            return Constants.PlatformEndpoint.EuWest1.DEV_PLATFORM_API_URL;
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                default:
                    throw new InvalidEnumArgumentException(INVALID_CLOUD_REGION);
            }
        }
    }
}
