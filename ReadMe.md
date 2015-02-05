# MsmqReader

While SSIS packages can communicate through MSMQ, there doesn't really seem to be a comfortable way of just reading data from your custom queue. (From what I've found it virtually seems the only supported scenario is using MSMQ to pass messages in a specific format, between two SSIS packages.)
This component provides the queue reading functionality.

As always when registering a new component to be used by the SSIS (and in the Data Tools designer), you will have to copy the DLL into the [special SQL folders and register it in GAC](http://agilebi.com/mcole/2009/08/18/adding-custom-components-to-ssis/). A script doing this with the Release version of the DLL is included - see `registerComponent.ps1`.

The component reads three of the MSMQ message's properties: body, label and ID. I hadn't needed any other, and YAGNI and all that. Adding more fields should be fairly straightforward, though.

## Additional remarks

* The script only registers the component for SQL Server 11 (2012).
* I have never been able to get the registered component to show up in Visual Studio 2013. Data Tools for 2010 and 2012 work fine.
* If you are unable to compile the project because of missing Microsoft.SQLServer references, installing the Data Tools (Business Intelligence) package might help.
* **I didn't encrypt the signing key.** Don't consider it safe. (I wanted to include it in the solution, so you can just grab the code and run it.)

## Troubleshooting

For a long time I haven't been able to figure out why the package complains about not being able to write to a property (at runtime), and other similar errors.

Turns out I had to use `gacutil` to register the DLL. Since I didn't want to copy the .NET SDK to the production servers, I've tried different approaches to registering the library with GAC, but even though most of them seemed to have worked (judging from their output), the SSIS failed to load the library. Only after copying
