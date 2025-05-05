using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        private const string ENVIRONMENT_X_IS_INVALID_FOR_REGION_Y = "The {0} environment is invalid for the region {1}.";

        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="p_Args">Command-line arguments.</param>
        private static void Main(string[] p_Args)
        {
            if (p_Args.Length == 0)
            {
                // Read the values interactively.
                IndexFiles(GetProgramArgumentsInteractively());
            }
            else
            {
                // Use the values specified on the command line.
                new Parser(settings =>
                {
                    settings.CaseInsensitiveEnumValues = true;
                    settings.HelpWriter = Console.Out;
                })
                    .ParseArguments<ProgramArguments>(p_Args)
                    .WithParsed(parsedArgs =>
                    {
                        IndexFiles(parsedArgs);
                    });
            }
        }

        /// <summary>
        /// Gets the program arguments by reading them interactively.
        /// </summary>
        /// <returns>Input arguments read from the keyboard.</returns>
        private static ProgramArguments GetProgramArgumentsInteractively()
        {
            ProgramArguments programArgs = new ProgramArguments();

            foreach (PropertyInfo property in typeof(ProgramArguments).GetProperties())
            {
                string helpText = "";
                bool isRequired = false;
                object? defaultValue = null;
                foreach (CustomAttributeData attrData in property.CustomAttributes)
                {
                    if (attrData.AttributeType == typeof(OptionAttribute))
                    {
                        foreach (CustomAttributeNamedArgument namedArg in attrData.NamedArguments)
                        {
                            switch (namedArg.MemberName)
                            {
                                case nameof(OptionAttribute.HelpText):
                                    helpText = (string)(namedArg.TypedValue.Value ?? "");
                                    break;
                                case nameof(OptionAttribute.Required):
                                    isRequired = (bool)(namedArg.TypedValue.Value ?? false);
                                    break;
                                case nameof(OptionAttribute.Default):
                                    defaultValue = namedArg.TypedValue.Value;
                                    break;
                            }
                        }
                        break;
                    }
                    Debug.Assert(isRequired != (defaultValue != null));
                }

                Console.WriteLine(helpText);
                bool success = false;
                while (!success)
                {
                    Console.Write($"{property.Name}{(defaultValue == null ? "" : " [" + defaultValue + "]")}: ");

                    string valueStr = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (property.PropertyType == typeof(CloudEnvironment))
                    {
                        Debug.Assert(isRequired);
                        success = Enum.TryParse(valueStr, true, out CloudEnvironment environment);
                        property.SetValue(programArgs, environment);
                    }
                    else if (property.PropertyType == typeof(CloudRegion))
                    {
                        Debug.Assert(isRequired);
                        success = Enum.TryParse(valueStr, true, out CloudRegion region);
                        property.SetValue(programArgs, region);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        if (valueStr != "")
                        {
                            property.SetValue(programArgs, valueStr);
                            success = true;
                        }
                        else if (defaultValue != null)
                        {
                            property.SetValue(programArgs, defaultValue);
                            success = true;
                        }
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        if (valueStr != "")
                        {
                            success = int.TryParse(valueStr, out int intValue);
                            property.SetValue(programArgs, intValue);
                        }
                        else if (defaultValue != null)
                        {
                            property.SetValue(programArgs, defaultValue);
                            success = true;
                        }
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        if (valueStr != "")
                        {
                            success = bool.TryParse(valueStr, out bool boolValue);
                            property.SetValue(programArgs, boolValue);
                        }
                        else if (defaultValue != null)
                        {
                            property.SetValue(programArgs, defaultValue);
                            success = true;
                        }
                    }
                    else
                    {
                        Debug.Fail("Unsupported value type.");
                    }

                    if (!success)
                    {
                        ConsoleColor originalColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid value.");
                        Console.ForegroundColor = originalColor;
                    }
                }

                Console.WriteLine();
            }

            return programArgs;
        }

        /// <summary>
        /// Indexes the files located in the specified folder.
        /// </summary>
        /// <param name="p_Args">Parsed command-line arguments.</param>
        private static void IndexFiles(ProgramArguments p_Args)
        {
            string folder = Path.GetFullPath(p_Args.folder);
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Console.WriteLine($"Pushing files \"{p_Args.include}\" from folder \"{folder}\"...");

            ulong orderingId = RequestOrderingUtilities.CreateOrderingId();

            ICoveoPlatformConfig platformConfig = new CoveoPlatformConfig(
                GetPushApiUrl(p_Args),
                GetPlatformApiUrl(p_Args),
                p_Args.apikey,
                p_Args.organizationid
            );
            using (ICoveoPlatformClient platformClient = new CoveoPlatformClient(platformConfig))
            {
                IList<PushDocument> documentBatch = new List<PushDocument>();
                foreach (
                    FileInfo fileInfo in new DirectoryInfo(folder).EnumerateFiles(
                        p_Args.include,
                        p_Args.recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                    )
                )
                {
                    if (!fileInfo.FullName.StartsWith(folder))
                    {
                        throw new Exception("Unexpected file gathered from outside the source folder.");
                    }
                    Console.WriteLine(fileInfo.FullName.Substring(folder.Length));

                    PushDocument document = new PushDocument(new Uri(fileInfo.FullName).AbsoluteUri) { ModifiedDate = fileInfo.LastWriteTimeUtc };
                    document.AddMetadata("title", fileInfo.Name);
                    document.AddMetadata("fileextension", fileInfo.Extension);
                    if (fileInfo.Length > 0)
                    {
                        PushDocumentHelper.SetBinaryContentFromFileAndCompress(document, fileInfo.FullName);
                    }
                    documentBatch.Add(document);

                    if (documentBatch.Count >= p_Args.batchSize)
                    {
                        // Push this batch of documents.
                        SendBatch(platformClient, documentBatch, p_Args.sourceid, orderingId);
                    }
                }

                // Send the (partial) final batch of documents.
                SendBatch(platformClient, documentBatch, p_Args.sourceid, orderingId);

                // Delete the already indexed files that no longer exist.
                platformClient.DocumentManager.DeleteDocumentsOlderThan(p_Args.sourceid, orderingId, null);
            }
        }

        /// <summary>
        /// Sends a batch of documents to the platform.
        /// </summary>
        /// <param name="p_PlatformClient">The instance of <see cref="ICoveoPlatformClient"/> to use.</param>
        /// <param name="p_DocumentBatch">The batch of documents to send.</param>
        /// <param name="p_SourceId">ID of the source in which to push documents.</param>
        /// <param name="p_OrderingId">The ordering identifier.</param>
        private static void SendBatch(ICoveoPlatformClient p_PlatformClient, IList<PushDocument> p_DocumentBatch, string p_SourceId, ulong p_OrderingId)
        {
            if (p_DocumentBatch.Count == 0)
            {
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
            switch (p_Args.region)
            {
                case CloudRegion.UsEast1:
                    switch (p_Args.environment)
                    {
                        case CloudEnvironment.Hipaa:
                            return Constants.Endpoint.UsEast1.HIPAA_PUSH_API_URL;
                        case CloudEnvironment.Prod:
                            return Constants.Endpoint.UsEast1.PROD_PUSH_API_URL;
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                case CloudRegion.EuWest1:
                    switch (p_Args.environment)
                    {
                        case CloudEnvironment.Hipaa:
                            throw new InvalidEnumArgumentException(string.Format(ENVIRONMENT_X_IS_INVALID_FOR_REGION_Y, p_Args.environment, p_Args.region));
                        case CloudEnvironment.Prod:
                            return Constants.Endpoint.EuWest1.PROD_PUSH_API_URL;
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                case CloudRegion.ApSouthEast2:
                    switch (p_Args.environment)
                    {
                        case CloudEnvironment.Prod:
                            return Constants.Endpoint.ApSoutheast2.PROD_PUSH_API_URL;
                        case CloudEnvironment.Hipaa:
                            throw new InvalidEnumArgumentException(string.Format(ENVIRONMENT_X_IS_INVALID_FOR_REGION_Y, p_Args.environment, p_Args.region));
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
            switch (p_Args.region)
            {
                case CloudRegion.UsEast1:
                    switch (p_Args.environment)
                    {
                        case CloudEnvironment.Hipaa:
                            return Constants.PlatformEndpoint.UsEast1.HIPAA_PLATFORM_API_URL;
                        case CloudEnvironment.Prod:
                            return Constants.PlatformEndpoint.UsEast1.PROD_PLATFORM_API_URL;
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                case CloudRegion.EuWest1:
                    switch (p_Args.environment)
                    {
                        case CloudEnvironment.Hipaa:
                            throw new InvalidEnumArgumentException(string.Format(ENVIRONMENT_X_IS_INVALID_FOR_REGION_Y, p_Args.environment, p_Args.region));
                        case CloudEnvironment.Prod:
                            return Constants.PlatformEndpoint.EuWest1.PROD_PLATFORM_API_URL;
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                case CloudRegion.ApSouthEast2:
                    switch (p_Args.environment)
                    {
                        case CloudEnvironment.Prod:
                            return Constants.PlatformEndpoint.ApSoutheast2.PROD_PLATFORM_API_URL;
                        case CloudEnvironment.Hipaa:
                            throw new InvalidEnumArgumentException(string.Format(ENVIRONMENT_X_IS_INVALID_FOR_REGION_Y, p_Args.environment, p_Args.region));
                        default:
                            throw new InvalidEnumArgumentException(INVALID_CLOUD_ENVIRONMENT);
                    }
                default:
                    throw new InvalidEnumArgumentException(INVALID_CLOUD_REGION);
            }
        }
    }
}
