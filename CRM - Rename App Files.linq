<Query Kind="Program" />

string filePath = "";
string newAppName = "";

void Main()
{
	// USAGE:
	//
	// CLOSE VISUAL STUDIO
	// 
	// If you don't have a local copy of the freeCRM solution, go to Azure DevOps
	// and download the entire repo as a ZIP from. The URL is:
	//
	// https://wickett.visualstudio.com/CRM
	//
	// Next to the Clone button use the vertial three-dot menu and select Download as ZIP.
	// Extract that ZIP file and copy the .sln file and all the folders from the extracted folder
	// into your location where you are going to run this script. Update the filePath below to reflect that path.
	
	// The path to the root file location where you have copied the freeCRM Application solution.
	// This should be a folder containing the CRM.sln file and all the additional
	// folders for the projects in the solution.
	filePath = @"C:\Working\CRM";
	
	// The name to use to replace CRM with. This should contain no spaces.
	newAppName = "YourNewAppName";
	
	RenameFoldersAndSolutionFiles();
	UpdateSolutionFile();
	UpdateSecretsGuid();
	RenameAllFileContents();
	"Done".Dump();
}

protected void RenameAllFileContents(){
	string[] files = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);

	List<string> updateExtensions = new List<string> { ".cs", ".csproj", ".txt", ".json", ".cshtml", ".ts", ".razor" };
	List<string> blockPaths = new List<string> { @"\obj\", @"\bin\"};
	
	if(files.Any()){
		foreach(var file in files){
			string fileLower = file.ToLower();
			bool blockedPath = false;
			foreach(var item in blockPaths){
				if(!blockedPath && fileLower.Contains(item)){
					blockedPath = true;
				}
			}
			
			if(!blockedPath){
				string ext = Path.GetExtension(fileLower);
				if(!String.IsNullOrWhiteSpace(ext) && updateExtensions.Contains(ext)){
					string contents = File.ReadAllText(file);
					string updated = String.Empty;

					// Replace both freeCRM and CRM with the new app name. Do freeCRM first, as CRM would find the end of that string.
					if (contents.Contains("crmhub") || contents.Contains("freeCRM") || contents.Contains("CRM")) {
						updated = contents
							.Replace("crmhub", newAppName + "hub")
							.Replace("freeCRM", newAppName)
							.Replace("CRM", newAppName);
						File.WriteAllText(file, updated);
					}
				}
			}
		}
	}
}

protected void RenameFoldersAndSolutionFiles() {
	RenameFile("CRM.sln", newAppName + ".sln");

	RenameFolder("CRM", newAppName);
	RenameFolder("CRM.Client", newAppName + ".Client");
	RenameFolder("CRM.DataAccess", newAppName + ".DataAccess");
	RenameFolder("CRM.DataObjects", newAppName + ".DataObjects");
	RenameFolder("CRM.EFModels", newAppName + ".EFModels");
	RenameFolder("CRM.Plugins", newAppName + ".Plugins");

	RenameFile(newAppName + "/CRM.csproj", newAppName + "/" + newAppName + ".csproj");
	RenameFile(newAppName + ".Client/CRM.Client.csproj", newAppName + ".Client/" + newAppName + ".Client.csproj");
	RenameFile(newAppName + ".DataAccess/CRM.DataAccess.csproj", newAppName + ".DataAccess/" + newAppName + ".DataAccess.csproj");
	RenameFile(newAppName + ".DataObjects/CRM.DataObjects.csproj", newAppName + ".DataObjects/" + newAppName + ".DataObjects.csproj");
	RenameFile(newAppName + ".EFModels/CRM.EFModels.csproj", newAppName + ".EFModels/" + newAppName + ".EFModels.csproj");
	RenameFile(newAppName + ".Plugins/CRM.Plugins.csproj", newAppName + ".Plugins/" + newAppName + ".Plugins.csproj");
}

protected void RenameFile(string currentName, string newName)
{
	if (File.Exists(Path.Combine(filePath, currentName))) {
		File.Move(Path.Combine(filePath, currentName), Path.Combine(filePath, newName));
	}
}

protected void RenameFolder(string currentFolder, string newFolder) {
	if(Directory.Exists(Path.Combine(filePath, currentFolder))) {
		Directory.Move(Path.Combine(filePath, currentFolder), Path.Combine(filePath, newFolder));
	}
}

protected void UpdateSolutionFile(){
	string file = Path.Combine(filePath, newAppName + ".sln");
	if(File.Exists(file)){
		string sln = File.ReadAllText(file);
		
		// Replace all the project GUIDs
		sln = sln.Replace("AC08C45F-3926-465F-B17A-09E5D26AA100", Guid.NewGuid().ToString().ToUpper()); // CRM
		sln = sln.Replace("DF8E0710-4710-4FA5-905B-0494E3E3051C", Guid.NewGuid().ToString().ToUpper()); // CRM.Client
		sln = sln.Replace("4CCAF2CF-4AAC-4E55-804E-54AD9F123B39", Guid.NewGuid().ToString().ToUpper()); // CRM.DataAccess
		sln = sln.Replace("3DF897AB-E5ED-42FD-869F-E071F1537C71", Guid.NewGuid().ToString().ToUpper()); // CRM.DataObjects
		sln = sln.Replace("6631DCE7-75DC-4044-936C-36D2F527380C", Guid.NewGuid().ToString().ToUpper()); // CRM.EFModels
		sln = sln.Replace("9C1DB5D4-22EA-4368-A5BC-04C7157D0665", Guid.NewGuid().ToString().ToUpper()); // Solution Items
		sln = sln.Replace("A3796CA0-CA85-46AF-A0DE-7641736C95B9", Guid.NewGuid().ToString().ToUpper()); // Solution Items
		sln = sln.Replace("9A19103F-16F7-4668-BE54-9A1E7A4F7556", Guid.NewGuid().ToString().ToUpper()); // Solution Items

		sln = sln.Replace("CRM", newAppName);

		File.WriteAllText(file, sln);
	}
}

protected void UpdateSecretsGuid()
{
	string file = Path.Combine(filePath, newAppName, newAppName + ".csproj");
	if (File.Exists(file)) {
		string contents = File.ReadAllText(file);

		contents = contents.Replace("<UserSecretsId>8b0c50df-302a-4757-a055-3f66dd5fe0f4</UserSecretsId>", "<UserSecretsId>" + Guid.NewGuid().ToString() + "</UserSecretsId>");

		File.WriteAllText(file, contents);
	}
}