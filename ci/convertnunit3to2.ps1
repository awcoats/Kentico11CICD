

$dir =   (Split-Path -parent $PSCommandPath) -replace '\ci',''


$assemblyNunitEngine = $dir + '\nunit.engine.api.dll';
$assemblyNunitWriter = $dir + '\nunit-v2-result-writer.dll';
$inputV3Xml = $dir + '.\..\server\Demo1.UnitTests\bin\Debug\TestResults.xml';
$outputV2Xml = $dir + '.\..\server\Demo1.UnitTests\bin\Debug\TestResultsV2.xml';



Add-Type -Path $assemblyNunitEngine;
Add-Type -Path $assemblyNunitWriter;
$xmldoc = New-Object -TypeName System.Xml.XmlDataDocument;
$fs = New-Object -TypeName System.IO.FileStream -ArgumentList $inputV3Xml,'Open','Read';
$xmldoc.Load($fs);
$xmlnode = $xmldoc.GetElementsByTagName('test-run').Item(0);
$writer = New-Object -TypeName NUnit.Engine.Addins.NUnit2XmlResultWriter;
$writer.WriteResultFile($xmlnode, $outputV2Xml);