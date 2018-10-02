#load  "BlueModus.cake"
#addin "Cake.Powershell"
#addin "Cake.XdtTransform"
#addin "Cake.MsDeploy"
#addin "Cake.Gulp"
#addin "Cake.Npm"
#addin "Cake.Http"
#addin "Cake.Slack"
#addin "Cake.FileHelpers"
#tool "nuget:?package=ReportGenerator&version=2.4.5"
#tool "nuget:?package=OpenCover"
#tool "nuget:?package=NUnit.ConsoleRunner"

using System.Net;

//////////////////////////////////////////////////////////////////////
// VARIABLES
//////////////////////////////////////////////////////////////////////

// passed in via command-line
var _target = Argument("target", "Default");
var _configuration = Argument("configuration", "Debug");
var _deploymentEnviroment = Argument("DeploymentEnvironment","DEV");
var _publishSettings = Argument("PublishSettings","");
var _slackHookUrl =  Argument("slackHookUrl","https://hooks.slack.com/services/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");
var _teamCityProject = Argument("TeamCityProject","Local");

// build related files directories
var _webSiteFolder = "./server/Demo1.Web";
var _publishFolder = "./server/PrecompiledWeb";
var _solutionFile = "./server/Demo1.sln";
var _projectToPublish = _webSiteFolder + "/Demo1.web.csproj";
var _assemblyInfoFile = _webSiteFolder + "/Properties/AssemblyInfo.cs";
var _unitTestsLocation = "./server/Demo1.UnitTests/bin/"+_configuration+"/Demo1.UnitTests.dll";

// team city 

var _isTeamCityBuild =TeamCity.IsRunningOnTeamCity;

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var consoleColor =  Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(@"AON");

Console.ForegroundColor =consoleColor;

Information("Deployment Environment:"+  _deploymentEnviroment);
Information("Publish Settings:"+ _publishSettings);
Information("slackHookUrl:"+ _slackHookUrl);
Information("Team City Project:"+ _teamCityProject);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////


Task("Clean")
	.Description("Removes contents of the bin and obj directories.")
	.IsDependentOn("InstallBuildTools")
    .Does(() =>
{
	WriteProgressMessage("Cleaning");
	
	// cleans output from front end
	if (DirectoryExists("./node_modules/gulp"))
	{
		//Gulp.Local.Execute(settings => settings.WithArguments("clean"));
	}

	// delete Precompiled web directory - otherwise deleted/ renamed files and directories
	// remain in the directory.
	if (DirectoryExists(_publishFolder))
	{
		DeleteDirectory(_publishFolder, new DeleteDirectorySettings { Recursive = true, Force = true});
	}
		
	// delete NUnit test results
	DeleteFiles("TestResult*.xml");

	// using MSBuilds clean target. Recurively deleting **\*\bin directories
	// resulted in .\server\TheInstitute.Web\CMSAdminControls\Debug directory
	// being deleted. Note - The MSBuild deletes the files from \bin\Debug|Release
	// directories, but does not delete the empty directory.
	MSBuild(_solutionFile, settings => settings.SetConfiguration(_configuration)
	  	.SetVerbosity(Verbosity.Minimal)
		.WithTarget("Clean"));
});


Task("Restore-NuGet-Packages")
	.Description("Restores NuGet packages.")
    .IsDependentOn("Clean")
    .Does(() =>
{
	WriteProgressMessage("Restoring NuGet packages");

    NuGetRestore(_solutionFile, new NuGetRestoreSettings()
	{
		ToolPath=@".\tools\nuget.exe",
		PackagesDirectory="./server/packages"
	});	
});

Task("CIBuild")
    .IsDependentOn("Build")
    .IsDependentOn("UnitTests")
	.IsDependentOn("Publish");

Task("Update-Assembly-Version").Does(() =>
{
	if (_isTeamCityBuild)
	{		
		// update AssemblyInfo.cs manually rather than use the Cake task, otherwise we lose some of the attributes (e.g. AutoDiscoverable) that are
		// are required by Kentico.
 		var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");

		Information("Updating AssemblyInfo. TeamCity build number:"+buildNumber);
		
		var lines = System.IO.File.ReadAllLines(_assemblyInfoFile).ToList().Where(line=>line.Contains("AssemblyVersion")==false).ToList();	
		var version = "1.0.0."+buildNumber;
		lines.Add("[assembly: AssemblyVersion(\""+version+"\")]");
	
		System.IO.File.WriteAllLines(_assemblyInfoFile,lines.ToArray());
        Information("Running in TeamCity, Updated AssemblyInfo file: " + version);
	}
    else
	{
		Information("Not running in TeamCity, will not update AssemblyInfo file");
    }
});


Task("InstallBuildTools")	
    .Does(() =>
{
	WriteProgressMessage("Installing build tools"); 	
	//TODO - test to see if NPM is installed

	// use a local version of gulp so different projects can use different versions of gulp.
	// npm install rimraf -g
	// rimraf node_modules
	if (!DirectoryExists("./node_modules/gulp"))
	{
		Information("Installing local version of gulp.");
		var settings = new NpmInstallSettings(){
			Global = false,
		};				
		NpmInstall(settings);
	}	
	else
	{
		Information("Gulp already installed.");
	}
});


Task("Build")
	.IsDependentOn("Restore-NuGet-Packages")	
    .Does(() =>
{  
	WriteProgressMessage("Building. Running gulp tools"); 	
	Gulp.Local.Execute(settings => settings.WithArguments("js:compile"));

	WriteProgressMessage("Building. Compiling C#"); 		
  
    MSBuild(_solutionFile, settings => settings.SetConfiguration(_configuration)
		//.SetMaxCpuCount(0)
		.SetVerbosity(Verbosity.Minimal));
		//.UseToolVersion(MSBuildToolVersion.NET46));
});

Task("CodeCoverage")
	//.IsDependentOn("Build")	
    .Does(() =>
{  
	CreateDirectory("./etc/temp");
	CreateDirectory("./etc/coverage_report");
	CreateDirectory("./etc/coverage_history");
	CleanDirectory("./etc/coverage_report");

	OpenCover(tool => {
		tool.NUnit3(_unitTestsLocation,
		new NUnit3Settings {
		ShadowCopy = false
		});
		},
		new FilePath("./etc/temp/CodeCoverge.xml"),
		new OpenCoverSettings()
		.WithFilter("+[Aon]*")
		.WithFilter("-[Aon.UnitTests]*")
	);

	var coverageReportDirectory=@"etc\coverage_report";
		var coverageHistoryDirectory=@"etc\coverage_history";
	ReportGenerator(@"./etc/temp/CodeCoverge.xml",coverageReportDirectory, new ReportGeneratorSettings()
	{
		HistoryDirectory = coverageHistoryDirectory
	});
});

Task("UnitTests")
    .Description("Runs unit tests.")  
    .Does(() =>
{
	WriteProgressMessage("Running unit tests"); 
	NUnit3(_unitTestsLocation, new NUnit3Settings
	{
		Results = new[] { new NUnit3Result { FileName = "./server/Demo1.UnitTests/bin/"+_configuration+"/TestResults.xml" } }
	});
});


Task("Publish")
	.Description("Publishes to PrecompiledWeb directory")  
	.Does(()=>
{
	WriteProgressMessage("Publishing"); 

	// make TMP be shorter so we can handle longer paths
	// by default TMP is %USERPROFILE%\AppData\Local\Temp
	// this causes file paths to exceed 260 characters which
	// causes the publish step to break.
	var tmp = Environment.GetEnvironmentVariable("TMP");
	Environment.SetEnvironmentVariable("TMP",@"C:\TEMP");
	WriteProgressMessage("Publishing"); 

	MSBuild(_projectToPublish, settings => settings
			//.UseToolVersion(MSBuildToolVersion.VS2017)
			.SetConfiguration(_configuration)
			.SetMaxCpuCount(0)
			.SetVerbosity(Verbosity.Minimal)
			.WithProperty("PublishProfile","FolderProfile") 
			.WithProperty("DeployOnBuild","true")
			.WithProperty("AllowUntrustedCertificate", "true"));	

	// reset TMP environment variable back to what it was			
	Environment.SetEnvironmentVariable("TMP",tmp);

	// Copy CI files over to the PrecompiledWeb directory, this way they do not have to be part
	// of the VisualStudio project.

	// if (_deploymentEnviroment.ToLowerInvariant()=="dev")
	// {
	// 	Information("Copying CI files to PrecompiledWeb");
	// 	CopyDirectory(_webSiteFolder+"/App_Data/CIRepository",_publishFolder+"/App_Data/CIRepository");
	// }
	// else
	// {
	// 	Information("Not copying CI files to PrecompiledWeb as these are not required in PROD");
	// }

	// these are not part of the VisualStudio project, so need to be copied afterwards.
	
	//TODO make this a list of directories
	//Information("Copying static HTML files - Netherlands");
	//CopyDirectory(_webSiteFolder+"/netherlands",_publishFolder+"/netherlands");

	//Information("Copying static HTML files - UnitedKingdom");
	//CopyDirectory(_webSiteFolder+"/unitedkingdom",_publishFolder+"/unitedkingdom");

});


Task("Deploy")
	.Description("Deploys with WebDeploy")     
	//.IsDependentOn("Publish") 	
	.Does(()=>
{
	WriteProgressMessage("Deploying"); 		
	
	Information("Deploying to "+_deploymentEnviroment);
	Information("Applying web.config transforms");
	var publishDir = MakeAbsolute(new DirectoryPath(_publishFolder));
	var source = File(_webSiteFolder + "/web.config");	
	var tranform = File(string.Format(_webSiteFolder + "/Web.{0}.config",_deploymentEnviroment));
	var dest = File(string.Format(_publishFolder + "/web.config"));	
	XdtTransformConfig(source, tranform, dest);

	// commented out because slows build down.
	// TODO: pass a switch in via PowerShell to turn this on/off
	// if (_isTeamCityBuild)
	// {
	// 	// files that we would on the server, but do not wish to 
	// 	// check into GitHub. 
	// 	var sourceDirectory = @"D:\AonComImagesAndAttachments";
	// 	var directoriesToCopy = new []{"images","siteImages","attachments"};
	// 	foreach(var directoryToCopy in directoriesToCopy)
	// 	{
	// 		Information("Copying "+directoryToCopy+" to PrecompiledWeb");
	// 		var srcDir= System.IO.Path.Combine(sourceDirectory, directoryToCopy);
	// 		var destDir = System.IO.Path.Combine(_publishFolder,directoryToCopy);
	// 		CopyDirectory(srcDir, destDir);
	// 	}
	// }
	
	if (_isTeamCityBuild)
	{
		Information("Writing BuildInfo.html");
		var html = @"<html>
		<head>
			<title>Aon.Com 2017 - BuildInformation</title>
		</head>
		<body>
			<h1>Build Information</h1>
			<table>
				<tr>
					<td>Build Date Time</td>
					<td>"+ System.DateTime.Now.ToString()+@"</td>
				</td>
				<tr>
					<td>TeamCity Project</td>
					<td>"+_teamCityProject+@"</td>
				</td>
				<tr>
					<td>Build Number</td>
					<td>"+EnvironmentVariable("BUILD_NUMBER")+@"</td>
				</td>			
			</table>
		</body>
		</html>";
		FileWriteText(_publishFolder+"/__BuildInfo__.html",html);
	}

	// we we are deploying to two DEV servers.
	// we only need to run Kentico CI on the first one
	// the other one just needs the latest code
	var isDev1 = _publishSettings.ToLowerInvariant().EndsWith("dev-aon-com.publishsettings");

	var doNotDeleteRule=true;
	if (_deploymentEnviroment.ToLowerInvariant()=="dev" && isDev1)
	{
		// delete everything to ensure we get a 'clean' build on the dev server
		doNotDeleteRule = false;
	}

	MSDeployUsingPublishSettingsFile(publishDir.FullPath, _publishSettings, doNotDeleteRule);
			
	if (_deploymentEnviroment.ToLowerInvariant()=="dev" && isDev1)
	{
		RunKenticoWebJob("ContinuousIntegration", _publishSettings);
	}
	else
	{
		Information("Skipping running Kentico ContinuousIntegration");
	}

	if (_isTeamCityBuild)
	{
		var webSiteUrl = XmlPeek(_publishSettings, "/publishData/publishProfile[@publishMethod='MSDeploy']/@destinationAppUrl");
		var siteName = XmlPeek(_publishSettings, "/publishData/publishProfile[@publishMethod='MSDeploy']/@msdeploySite");

		var slackMessage ="A new build of "+siteName+" has been deployed to <"+webSiteUrl+"|"+_deploymentEnviroment+">";
		SlackBuildStatus(_slackHookUrl, _deploymentEnviroment, slackMessage);
	}
});


Task("Default")
	.Description("Builds the app and run unit tests.")
	.IsDependentOn("Build")
    .IsDependentOn("UnitTests");
   

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(_target);

