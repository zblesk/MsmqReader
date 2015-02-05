$env:path += ";" + ${env:ProgramFiles(x86)} + "\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\x64"
$sourceFile = ".\bin\Release\MsmqReader.dll"
gacutil /i $sourceFile /f
copy-item $sourceFile $env:ProgramFiles"\Microsoft SQL Server\110\DTS\PipelineComponents"
copy-item $sourceFile ${env:ProgramFiles(x86)}"\Microsoft SQL Server\110\DTS\PipelineComponents"