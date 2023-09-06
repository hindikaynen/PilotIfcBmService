using Ascon.Pilot.Common;
using Ascon.Pilot.DataClasses;
using Ascon.Pilot.DataModifier;
using Ascon.Pilot.Server.Api;
using Ascon.Pilot.Transport;
using PilotIfcBmService;
using PilotIfcBmService.Stubs;
using ShellProgressBar;

var url = args[0];
var databaseName = args[1];
var login = args[2];
var password = args[3];
var sourceIfcFilePath = args[4];
var destinationBmFilePath = args[5];

var httpClient = new HttpPilotClient(url);
httpClient.Connect(false);

var auth = httpClient.GetAuthenticationApi();
auth.Login(databaseName, login, password.EncryptAes(), false, 90 /* Pilot-BIM */);

var callback = new ServerCallback();
var serverApi = httpClient.GetServerApi(callback);
var messagesApi = httpClient.GetMessagesApi(new MessagesCallback());
var fileArchiveApi = httpClient.GetFileArchiveApi();

var fileStorageDirectory = "temp";
Directory.CreateDirectory(fileStorageDirectory);
var fileStorageProvider = new FileSystemStorageProvider(fileStorageDirectory); // directory used as file storage
var changesetUploader = new ChangesetUploader(fileArchiveApi, fileStorageProvider);
var backend = new Backend(serverApi, messagesApi, changesetUploader);

var projectType = backend.GetType("project"); // make sure "project" type exists in DB configuration
var fileType = backend.GetType(SystemTypes.PROJECT_FILE);

// Create new project with IFC inside
Console.WriteLine($"Uploading {Path.GetFileName(sourceIfcFilePath)} to Pilot-Server...");

var modifier = new Modifier(backend);
var project = modifier.CreateObject(Guid.NewGuid(), Guid.Empty/* parentless project*/, projectType.Id);
var ifcFile = modifier.CreateObject(Guid.NewGuid(), project.Id, fileType.Id)
    .SetAttribute(SystemAttributes.PROJECT_ITEM_NAME, Path.GetFileName(sourceIfcFilePath));
ifcFile.AddFile(new DocumentInfo(sourceIfcFilePath), fileStorageProvider);
modifier.Apply();

Console.WriteLine($"{Path.GetFileName(sourceIfcFilePath)} uploaded to Pilot-Server.");


// Wait for Pilot-BIM-Server to process IFC file
Console.WriteLine("Waiting for Pilot-BIM-Server to start process IFC file...");
var caller = new ServerCommandCallService<IBimOperationsManager>(serverApi, callback, "Pilot-BIM-Server");
var marshaller = new Marshaller(caller);
var manager = marshaller.Get<IBimOperationsManager>();
using (var progressBar = new ProgressBar(10000, "Parsing IFC"))
{
    while (true)
    {
        var recentOperations = await manager.GetRecentServerOperations(10, 0);
        var operation = recentOperations.FirstOrDefault(x => x.IfcObjectId == ifcFile.Id);
        if (operation != null)
        {
            if (operation.Status == ServerOperationStatus.Error)
                throw new InvalidOperationException($"IFC parse completed with error: {operation.Error}");
        
            var progress = progressBar.AsProgress<float>(i => operation.Status.ToString());
            progress.Report((float)operation.ProgressValue / 100);

            if (operation.Status == ServerOperationStatus.Done)
            {
                progress.Report(1);
                break;
            }
        }
    }
}

Console.WriteLine("IFC parsing completed.");

var ifcFileUpdated = backend.GetObject(ifcFile.Id);
var relatedModelPartId = ifcFileUpdated.Relations.FirstOrDefault(x => x.Type == RelationType.SourceFiles).TargetId;
var relatedModelPart = backend.GetObject(relatedModelPartId);
var bmFile = relatedModelPart.ActualFileSnapshot.Files.FirstOrDefault(x => x.Name.EndsWith(".bm"));
if (bmFile == null)
    throw new InvalidOperationException(".bm file not found");

Console.WriteLine("Downloading .bm file...");
await using (var stream = new RemoteFileStream(fileArchiveApi, bmFile))
await using (var destinationStream = File.Create(destinationBmFilePath))
{
    stream.CopyTo(destinationStream);
}
Console.WriteLine($".bm file saved to {destinationBmFilePath}");
