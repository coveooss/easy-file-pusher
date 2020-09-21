# Easy File Pusher

The Coveo Easy File Pusher is a command-line tool to push documents stored in the local file system to a Coveo Push source. The source must have been created first using the Coveo Administration Console. Then running the tool scans the specified local folder and pushes the files matching the specified filter.

## How to use it

Launching the tool without any arguments displays the usage i.e. the arguments that can be specified on the command line:
```
ERROR(S):
  Required option 'e, environment' is missing.
  Required option 'r, region' is missing.
  Required option 'o, organizationid' is missing.
  Required option 's, sourceid' is missing.
  Required option 'k, apikey' is missing.
  Required option 'f, folder' is missing.

  -e, --environment       Required. Cloud environment: Hipaa, Prod, QA or Dev.

  -r, --region            Required. Cloud region: UsEast1 or EuWest1.

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
Note that for the most used arguments, each can be specified using either a long or a short name.

Here is an example of what the command line should look like when specifying all the required arguments:
```
Coveo.Connectors.EasyFilePusher -e Prod -r UsEast1 -o OrgNameHerez1x2c3v4 -s OrgNameHerez1x2c3v4-q1w2e3r4t5y6u7i8o9p0zxcvbn --apikey xxaaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee -f "C:\Folder\Path\Here"
```

## Supported Operating Systems

Since the tool was developed using .NET Core, it can be executed on the operating systems supported by .NET Core so Windows, Mac and Linux. An executable for each supported operating system is available for download.

Regarding the executable for Linux, after downloading it to a Linux machine, the "is executable" flag must be set on the file before running it. This is necessary for the executable to work.

Regarding the executable for Mac, note that the executable has not been signed with a certificate. Trying to run it on a Mac will cause a security warning to be displayed. Running it on a Mac is possible, but requires additional steps. Refer to the following page for more details:
https://support.apple.com/en-ca/guide/mac-help/mh40616/mac