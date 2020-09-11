# easy-file-pusher

The Coveo Easy File Pusher is a command-line tool to push documents stored in the local file system to a Coveo Push source. The source must have been created first using the Coveo Administration Console. Then the tool scans the specified local folder and pushes the files matching the specified filter. Launching the tool without any arguments displays the usage i.e. the arguments that can be specified on the command line.

Since the tool was developed using .NET Core, it can be executed on the operating systems supported by .NET Core: Windows, Mac and Linux. An executable for each supported operating system is available for download.

Regarding the executable for Linux, after downloading it to a Linux machine, the "is executable" flag must be set on the file before running it. This is necessary for the executable to work.

Regarding the executable for Mac, note that the executable has not been signed with a certificate. Trying to run it on a Mac will cause a security warning to be displayed. Running it on a Mac is possible, but requires additional steps. Refer to the following page for more details:
https://support.apple.com/en-ca/guide/mac-help/mh40616/mac