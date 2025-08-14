using PzMap;
using PzMapTools;

var arguments = args;

var assetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var distFolder = Path.Combine(assetFolder, "dist");
const string folderPath = "C:\\Dev\\pz-assets";
var lotHeaderReader = new LotHeaderReader();
var gameDataReader = new GameDataReader();
var pzMapRoomReader = new PzMapRoomReader(lotHeaderReader, folderPath);
var pzMapSvgBuilder = new PzMapSvgBuilder(gameDataReader, pzMapRoomReader, distFolder, folderPath);
if (!Directory.Exists(distFolder))
{
    Directory.CreateDirectory(distFolder);
}
var versionSettings = new List<VersionSettings>
{
    new VersionSettings{ VersionNumber = 41, CellWidth = 300, CellHeight = 300},
    //new VersionSettings{ VersionNumber = 42, CellWidth = 256, CellHeight = 256},
};
Action run = () =>
{
    pzMapSvgBuilder.BuildAndSaveVersions(versionSettings);

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