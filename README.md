# Easy File Pusher

The Coveo Easy File Pusher is a command-line tool that pushes items stored in a local file system to a Coveo Push source. To use the Easy File Pusher, you must first [create a Push source using the Coveo Administration Console](https://docs.coveo.com/en/1546/). Then, run the tool to scan the specified local folder and push the files matching the specified filter.

## Configure the Easy File Pusher

There are two ways to provide the configuration values to the tool:

- [Specifying them on the command line](#specifying-the-configuration-valyes-on-the-command-line)

- [Specifying them interactively](#specifying-the-configuration-values-interactively)

### Specifying the Configuration Values on the Command Line

Running the tool with the `--help` argument displays the arguments that can be specified on the command line:

```
  -e, --environment       Required. Cloud environment: Hipaa or Prod.

  -r, --region            Required. Cloud region: UsEast1, EuWest1 or ApSouthEast2.

  -o, --organizationid    Required. ID of the organization in which to push documents.

  -s, --sourceid          Required. ID of the source in which to push documents.

  -k, --apikey            Required. API key to use.

  -f, --folder            Required. Path of the local folder that contains the documents to index.

  --include               (Default: *) Wildcard expression for which matching files will be pushed. All files are pushed
                          by default.

  --recursive             (Default: true) Whether to recursively search in sub-folders for files to push. Sub-folders
                          are searched by default.

  --batchsize             (Default: 10) How many files to push per batch.

  --help                  Display this help screen.

  --version               Display version information.
```

The most used arguments can be specified using either a long or a short name.

If you specify all required arguments, the command line should look as follows:

```
Coveo.Connectors.EasyFilePusher -e Prod -r UsEast1 -o OrgNameHerez1x2c3v4 -s OrgNameHerez1x2c3v4-q1w2e3r4t5y6u7i8o9p0zxcvbn --apikey xxaaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee -f "C:\Folder\Path\Here"
```

### Specifying the Configuration Values Interactively

Alternatively, if you choose to launch the tool without any argument, it will then prompt you to enter the configuration values listed in the previous section, one after the other.

## Supported Operating Systems

Since the tool was developed using .NET Core, it can be executed on the operating systems supported by .NET Core, i.e., Windows, macOS, and Linux. An executable for each supported operating system is available for download.

Note that the macOS executable has not been signed with a certificate. As a result, running it requires a minor adjustment to work around a security warning:

1. Unzip the executable file.
2. Control-click (or right-click) the file > **Open**/**Open with** > **Utilities** > **Terminal**. If **Terminal** isn't available, change **Enable: Recommended Applications** to **Enable: All Applications**.
3. Enter the requested configuration values.

For further information on Control-click, see [Open a Mac app from an unidentified developer](https://support.apple.com/en-ca/guide/mac-help/mh40616/mac).
