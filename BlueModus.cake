
using System.Net;

Action<string> BuildWebSite = (webSiteFolder) =>
{
	WriteLine("\nBuilding WebSite: " + webSiteFolder);

	// copy all the files in the lib directory
	CopyFiles("./server/lib/*.*", webSiteFolder+"/bin" );

	// copy files that have .refresh dlls
	var extensions = new List<string>(){"dll.refresh","dll.config.refresh","exe.refresh","config.refresh"};
	foreach(var extension in extensions)
	{
		var replaceExtension = extension.Replace(".refresh","");
		WriteLine("Copying *."+replaceExtension);	
		var refreshFiles= System.IO.Directory.GetFiles(webSiteFolder+"/bin","*."+extension, SearchOption.AllDirectories);
		foreach(var refeshFile in refreshFiles)
		{	
			var targetDir = new System.IO.FileInfo(refeshFile).Directory.FullName;
			var sourceFile = System.IO.File.ReadAllLines(refeshFile)[0];
			
			var sourcePath =  webSiteFolder+"/"+sourceFile;
			var targetPath = refeshFile.Replace(extension,replaceExtension);	
			if (System.IO.File.Exists(sourcePath))
			{
				System.IO.File.Copy(sourcePath, targetPath,true);
			}		
			else
			{
				Information("Warning: could not find file "+sourcePath);
			}
		}
		if (refreshFiles.Length>0)
		{
			WriteLine("Copied "+refreshFiles.Length+" to "+webSiteFolder+"/bin");
		}
	}
};

Action<string,string, bool> MSDeployUsingPublishSettingsFile = (publishFolder,publishSettingsFile,addDoNotDeleteRule)=>
{
	if (!FileExists(publishSettingsFile))	
	{
		throw new FileNotFoundException("Could not find settings file:"+publishSettingsFile);
	}

	Information("PublishSettingsFile:"+publishSettingsFile);

	var publishUrl = XmlPeek(publishSettingsFile, "/publishData/publishProfile[@publishMethod='MSDeploy']/@publishUrl");
	var username = XmlPeek(publishSettingsFile, "/publishData/publishProfile[@publishMethod='MSDeploy']/@userName");
	var password = XmlPeek(publishSettingsFile, "/publishData/publishProfile[@publishMethod='MSDeploy']/@userPWD");
	var siteName = XmlPeek(publishSettingsFile, "/publishData/publishProfile[@publishMethod='MSDeploy']/@msdeploySite");
	
	publishUrl = "https://"+publishUrl+"/msdeploy.axd";
	Information("Publish URL:"+publishUrl);
	Information("SiteName:"+siteName);
	Information("Username:"+username);

	var directoriesToSkip=new string[]
	{		
		string.Format(@"^{0}\\images",siteName),
		string.Format(@"^{0}\\siteImages",siteName),
		string.Format(@"^{0}\\attachments",siteName),
		string.Format(@"^{0}\\belgium",siteName),
		string.Format(@"^{0}\\attachments",siteName),

	
		// round 1
		string.Format(@"^{0}\\belgium",siteName),
		string.Format(@"^{0}\\denmark",siteName),
		string.Format(@"^{0}\\finland",siteName),
		string.Format(@"^{0}\\sweden",siteName),
		string.Format(@"^{0}\\norway",siteName),
		string.Format(@"^{0}\\netherlands",siteName),
		string.Format(@"^{0}\\unitedkingdom",siteName),

		// round 2
		string.Format(@"^{0}\\apac",siteName),
		string.Format(@"^{0}\\china",siteName),
		string.Format(@"^{0}\\france",siteName),
		string.Format(@"^{0}\\germany",siteName),
		string.Format(@"^{0}\\hongkong",siteName),
		string.Format(@"^{0}\\india",siteName),
		string.Format(@"^{0}\\indonesia",siteName),
		string.Format(@"^{0}\\japan",siteName),
		string.Format(@"^{0}\\korea",siteName),
		string.Format(@"^{0}\\malaysia",siteName),
		string.Format(@"^{0}\\pakistan",siteName),
		string.Format(@"^{0}\\philippines",siteName),
		string.Format(@"^{0}\\poland",siteName),
		string.Format(@"^{0}\\saipan",siteName),
		string.Format(@"^{0}\\taiwan",siteName),
		string.Format(@"^{0}\\thailand",siteName),
		string.Format(@"^{0}\\singapore",siteName),
		string.Format(@"^{0}\\vietnam",siteName),

		// round3
		string.Format(@"^{0}\\about-aon",siteName),
		string.Format(@"^{0}\\argentina",siteName),
		string.Format(@"^{0}\\austria",siteName),
		string.Format(@"^{0}\\botswana",siteName),
		string.Format(@"^{0}\\brasil",siteName),
		string.Format(@"^{0}\\canada",siteName),
		string.Format(@"^{0}\\captives",siteName),
		string.Format(@"^{0}\\chile",siteName),
		string.Format(@"^{0}\\colombia",siteName),
		string.Format(@"^{0}\\industry-expertise",siteName),
		string.Format(@"^{0}\\ireland",siteName),
		string.Format(@"^{0}\\italy",siteName),
		string.Format(@"^{0}\\kenya",siteName),
		string.Format(@"^{0}\\mexico",siteName),
		string.Format(@"^{0}\\middle-east",siteName),
		string.Format(@"^{0}\\puertorico",siteName),
		string.Format(@"^{0}\\spain",siteName),
		string.Format(@"^{0}\\switzerland",siteName),
		string.Format(@"^{0}\\thewhiterockgroup",siteName),
		


		// directories with static HTML sites
		string.Format(@"^{0}\\2017-women-to-watch",siteName),
		string.Format(@"^{0}\\2011polriskmap",siteName),
		string.Format(@"^{0}\\2012politicalriskmap",siteName),
		string.Format(@"^{0}\\2013GlobalRisk",siteName),
		string.Format(@"^{0}\\2013politicalriskmap",siteName),
		string.Format(@"^{0}\\2014politicalriskmap",siteName),
		string.Format(@"^{0}\\2015GlobalRisk",siteName),
		string.Format(@"^{0}\\2015politicalriskmap",siteName),
		string.Format(@"^{0}\\2016fireport",siteName),
		string.Format(@"^{0}\\2016londonmarketreview",siteName),
		string.Format(@"^{0}\\2016politicalriskmap",siteName),
		string.Format(@"^{0}\\2016techreport",siteName),
		string.Format(@"^{0}\\2017-global-insurance-market-opportunities-report",siteName),
		string.Format(@"^{0}\\2017GlobalRisk",siteName),
		string.Format(@"^{0}\\2017-global-risk-management-survey",siteName),
		string.Format(@"^{0}\\2017-political-risk-terrorism-and-political-violence-maps",siteName),


		// Kentico temp files
		//string.Format(@"^{0}\\App_Data\\CMSModules\\CMSInstallation\\Packages\\Installed",siteName),
		string.Format(@"^{0}\\App_Data\\CMSModules\\DeviceProfiles",siteName),
		string.Format(@"^{0}\\App_Data\\CMSModules\\WebFarm",siteName),
		string.Format(@"^{0}\\App_Data\\Persistent",siteName),
		string.Format(@"^{0}\\App_Data\\51Degrees",siteName)
	};

	var filesToSkip= new string[]
	{
		// @"^images",
		// @"^siteImages",
		// @"^attachments"		
	};

	// we have to not delete files in the specified directories AND not delete directories
	var directoriesToSkipParams =directoriesToSkip.Select(dir=>string.Format(@"-skip:skipaction='Delete',objectname='filePath',absolutepath='{0}\\.*' -skip:skipaction='Delete',objectname='dirPath',absolutepath='{0}' ",dir));
	
	// skip specific files
	var filesToSkipParams =filesToSkip.Select(dir=>string.Format("-skip:File=\"{0}\"",dir.Replace("\\","\\\\")));
	var skipParams  = directoriesToSkipParams.Concat(filesToSkipParams).ToList();
		
	
	// find Kentico sites, these are directories that have a media sub-directory.
	// var siteDirectories = new List<string>();
	// foreach(var dir in System.IO.Directory.EnumerateDirectories(_publishFolder,"*"))
	// {	
	// 	var mediaPath = System.IO.Path.Combine(dir,"media");
	// 	if (System.IO.Directory.Exists(mediaPath))
	// 	{		
	// 		// skip deletes, but allow updates and adds in the media directory for Kentico results
	// 		var mediaDir = 	mediaPath.Replace(_publishFolder+"\\","").Replace("\\","\\\\");
	// 		skipParams.Add(string.Format(@"-skip:skipaction='Delete',objectname='filePath',absolutepath='{0}\\.*'",mediaDir));
	// 	}
	// }
	
	Information("Skipping the follwoing:");
	skipParams.ToList().ForEach(p=> Information(string.Format("  {0} {1}",skipParams.IndexOf(p)+1,p)));
		
	var msDeploy="C:/Program Files (x86)/IIS/Microsoft Web Deploy V3/msdeploy.exe";
	var arguments = new List<string>()
	{ 
		"-verb:sync",
		"-allowUntrusted:true",	
		"-source:contentPath="+publishFolder,
		"-dest:contentPath="+siteName+ ",wmsvc="+publishUrl+",password="+password+	",username="+username,
		"-enableRule:AppOffline",
		String.Join(" ", skipParams)		
	};

	if (addDoNotDeleteRule)
	{
		arguments.Add("-enableRule:DoNotDeleteRule");
	}

	var process = StartAndReturnProcess(msDeploy,new ProcessSettings
	{ 
		Arguments = string.Join(" ",arguments)
	});
    
	process.WaitForExit();
	var exitCode = process.GetExitCode();
	if (exitCode != 0)
	{
		throw new Exception(String.Format("MSdeploy failed. (Exit code was {0}().",exitCode));   
	}	
};



Action<string,string> RunKenticoWebJob = (webJobName, publishSettings)=>{	
	WriteProgressMessage("Running Kentico continuousIntegration.exe");
	
	var resultCollection = StartPowershellFile(@".\etc\AzureKenticoCIWebJob.ps1", args =>
	{
		args.Append("publishSettingsFilename", publishSettings)
			.Append("-triggerJob");
	});
	Information("Result:"+resultCollection[0].BaseObject.ToString());
    var returnCode = int.Parse(resultCollection[0].BaseObject.ToString());
	Information("Result: {0}", returnCode);

	if (returnCode != 0) {
		Error("Script failed to execute");
	}
};

Action<string,string,string> SlackBuildStatus = (slackHookUrl, deploymentEnviroment,message)=>{
	Information("Sending Slack message");

	var postMessageResult = Slack.Chat.PostMessage(
		channel:"help-devops",
		text:message,
		messageSettings:new SlackChatMessageSettings { IncomingWebHookUrl = slackHookUrl }
	);

	if (postMessageResult.Ok)
	{
		Information("Message successfully sent");
	}
	else
	{
		Error("Failed to send message: {0}", postMessageResult.Error);
	}
};


Action<string> WriteProgressMessage = (message)=>
{
	if (TeamCity.IsRunningOnTeamCity)
	{
		TeamCity.WriteProgressMessage(message);
	}
};

static Action<string> WriteLine = (message)=>
{
	System.Console.WriteLine(message);
};