using PzMap;
using PzMapTools;

var arguments = args;

var assetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var distFolder = Path.Combine(assetFolder, "dist");
const string folderPath = "C:\\Dev\\pz-assets";
var lotHeadeReader = new LotHeaderReader();
Action run = () =>
{
    if (!Directory.Exists(distFolder)) 
    {  
        Directory.CreateDirectory(distFolder); 
    }
    var versionPaths = new Dictionary<int, string>
    {
        [41] = "C:\\Dev\\pz-assets\\b41",
        //[42] = "C:\\Dev\\pz-assets\\b42",
    };
    new PzMapSvgBuilder(
        new GameDataReader(),
        new PzMapRoomReader(
            lotHeadeReader,
            folderPath),
        distFolder
        ).BuildAndSaveVersions(versionPaths);

    var htmlBuilder = new HtmlBuilder(
        assetFolder,
        "index.html");
    htmlBuilder.PerformAndSave(Path.Combine(distFolder, "index.html"));
};
run();

var watcher = new FileSystemWatcher(Path.Combine(Directory.GetCurrentDirectory().Split("bin")[0], "wwwroot"));
watcher.EnableRaisingEvents = true;
watcher.NotifyFilter = NotifyFilters.LastWrite;
watcher.Filter = "*.*";
watcher.Changed += (Object o, FileSystemEventArgs e) =>
{
    Console.WriteLine("Running with file changed:");
    Console.WriteLine(e.FullPath);
    run();
};
Console.WriteLine("Watching folder for changes");
new System.Threading.AutoResetEvent(false).WaitOne();