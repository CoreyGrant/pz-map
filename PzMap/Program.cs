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
watcher.Changed += (Object o, FileSystemEventArgs e) =>
{
    Console.WriteLine("Running with file changed:");
    Console.WriteLine(e.FullPath);
    run();
};
Console.WriteLine("Watching folder for changes");
new System.Threading.AutoResetEvent(false).WaitOne();

//var defaultFunction = "mapgen";
//var function = args.Length == 0 ? defaultFunction : args[0];
//if (function == "mapgen")
//{
//    Console.WriteLine("Performing map generation");
//    // Generate the map image
//    new RoadReader().Generate();

//} else if (function == "propnames")
//{
//    Console.WriteLine("Printing out property names in worldmap.xml");
//    //var lines = File.ReadAllLines(@"C:\Program Files (x86)\Steam\steamapps\common\ProjectZomboid\media\maps\Muldraugh, KY\worldmap.xml");
//    //var propertyRegex = new Regex("<property name=[\"a-zA-Z=\\s_\\*\\-]{1,}\\/>");
//    //var propertyLines = lines.Where(x => propertyRegex.IsMatch(x)).Select(x => x.Trim()).Distinct();
//    //Console.Write(string.Join("\n", propertyLines));
//} else if(function == "lotheader")
//{
//    Console.WriteLine("Converting lotheader files into json");
//    var folderPath = @"C:\Program Files (x86)\Steam\steamapps\common\ProjectZomboid\media\maps\Muldraugh, KY";
//    var outputPath = @"C:\Dev\logs";
//    var directory = Directory.GetFiles(folderPath, "*.lotheader");
//    var jss = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
//    var index = 0;
//    var keptFiles = 0;
//    var strings = new List<string>();
//    foreach (var file in directory)
//    {
//        index++;
//        var outputFileName = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file) + ".json");
//        using (var fs = File.OpenRead(file))
//        {
//            var lhReader = new LotHeaderReader(fs);
//            var lhFile = lhReader.Read();
//            var lhJson = JsonConvert.SerializeObject(lhFile, Formatting.Indented, jss);
//            File.WriteAllText(outputFileName, lhJson);
//            strings.AddRange(lhFile.UnknownStrings);
//        }
//        if (index % 100 == 0)
//        {
//            Console.WriteLine($"Processed '{index}', kept '{keptFiles}'");
//        }
//    }
//    var numberRegex = new Regex("_[0-9]{1,2}_[0-9]{1,2}");
//    string[] excludeStrings = ["railroad_", "advertising_", "appliances_", "grassoverlays_", "blends_natural_"];
//    var stringsSorted = strings.Select(x => numberRegex.Replace(x, "")).Distinct().Where(x => excludeStrings.All(e => !x.Contains(e))).Order();
//    Console.WriteLine($"Processed '{index}', kept '{keptFiles}'");
//    var json = JsonConvert.SerializeObject(stringsSorted, Formatting.Indented, jss);
//    File.WriteAllText(Path.Combine(outputPath, "000Strings.json"), json);
//}