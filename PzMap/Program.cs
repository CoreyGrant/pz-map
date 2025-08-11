using PzMap;
using PzMapTools;

var arguments = args;

var assetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
const string lotHeaderPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ProjectZomboid\\media\\maps\\Muldraugh, KY";
var lotHeadeReader = new LotHeaderReader();
var dataCache = new DataCache();
Action run = () =>
{
    string svg; string metadata; string info;
    if(!dataCache.TryGetCached(out svg, out metadata, out info))
    {
        (svg, metadata, info) = new PzMapSvgBuilder(
        new GameDataReader(),
        new PzMapRoomReader(
            lotHeadeReader,
            lotHeaderPath)).BuildSvg();
        dataCache.Cache(svg, metadata, info);
    }
    
    var htmlBuilder = new HtmlBuilder(
        assetFolder,
        "index.html",
        [
            new AssetReplacement
            {
                FileName = "index.js",
                ReplacementText = "<script id=\"main\"></script>"
            },
            new AssetReplacement
            {
                FileName = "mapSvg.js",
                ReplacementText = "<script id=\"mapSvg\"></script>"
            },
            new AssetReplacement{
                FileName = "index.css",
                ReplacementText = "<style id=\"app-style\"></style>",
                Tag = "style"
            },
            new AssetReplacement{
                FileName = "index-mobile.css",
                ReplacementText = "<style id=\"app-style-mobile\"></style>",
                Tag = "style"
            },
            new AssetReplacement{
                FileName = "popover.js",
                ReplacementText = "<script id=\"popover\"></script>"
            },
            new AssetReplacement{
                FileName = "saveManager.js",
                ReplacementText = "<script id=\"saveManager\"></script>"
            },
            new AssetReplacement{
                FileName = "stateManager.js",
                ReplacementText = "<script id=\"stateManager\"></script>"
            },
            new AssetReplacement{
                FileName = "toolbar.js",
                ReplacementText = "<script id=\"toolbar\"></script>"
            },
            new AssetReplacement{
                FileName = "locator.js",
                ReplacementText = "<script id=\"locator\"></script>"
            },
            new AssetReplacement{
                FileName = "textAnnotater.js",
                ReplacementText = "<script id=\"textAnnotater\"></script>"
            }
        ],
        [
            new JsonReplacement{
                ReplacementText = "<script id=\"metadata\"></script>",
                Json = metadata,
                Name = "metadata",
            },
            new JsonReplacement{
                ReplacementText = "<script id=\"info\"></script>",
                Json = info,
                Name = "info"
            }
        ],
        [
            new TextReplacement{
                ReplacementText = "<svg></svg>",
                Text = svg
            }
        ]);
    htmlBuilder.PerformAndSave("index.html");
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