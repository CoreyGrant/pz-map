// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PzMap;
using System.IO;
using System.Text.RegularExpressions;

var arguments = args;
var defaultFunction = "mapgen";
var function = args.Length == 0 ? defaultFunction : args[0];
if (function == "mapgen")
{
    Console.WriteLine("Performing map generation");
    // Generate the map image
    new RoadReader().Generate();

} else if (function == "propnames")
{
    Console.WriteLine("Printing out property names in worldmap.xml");
    //var lines = File.ReadAllLines(@"C:\Program Files (x86)\Steam\steamapps\common\ProjectZomboid\media\maps\Muldraugh, KY\worldmap.xml");
    //var propertyRegex = new Regex("<property name=[\"a-zA-Z=\\s_\\*\\-]{1,}\\/>");
    //var propertyLines = lines.Where(x => propertyRegex.IsMatch(x)).Select(x => x.Trim()).Distinct();
    //Console.Write(string.Join("\n", propertyLines));
} else if(function == "lotheader")
{
    Console.WriteLine("Converting lotheader files into json");
    var folderPath = @"C:\Program Files (x86)\Steam\steamapps\common\ProjectZomboid\media\maps\Muldraugh, KY";
    var outputPath = @"C:\Dev\logs";
    var directory = Directory.GetFiles(folderPath, "*.lotheader");
    var jss = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
    var index = 0;
    var keptFiles = 0;
    var strings = new List<string>();
    foreach (var file in directory)
    {
        index++;
        var outputFileName = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file) + ".json");
        using (var fs = File.OpenRead(file))
        {
            var lhReader = new LotHeaderReader(fs);
            var lhFile = lhReader.Read();
            var lhJson = JsonConvert.SerializeObject(lhFile, Formatting.Indented, jss);
            File.WriteAllText(outputFileName, lhJson);
            strings.AddRange(lhFile.UnknownStrings);
        }
        if (index % 100 == 0)
        {
            Console.WriteLine($"Processed '{index}', kept '{keptFiles}'");
        }
    }
    var numberRegex = new Regex("_[0-9]{1,2}_[0-9]{1,2}");
    string[] excludeStrings = ["railroad_", "advertising_", "appliances_", "grassoverlays_", "blends_natural_"];
    var stringsSorted = strings.Select(x => numberRegex.Replace(x, "")).Distinct().Where(x => excludeStrings.All(e => !x.Contains(e))).Order();
    Console.WriteLine($"Processed '{index}', kept '{keptFiles}'");
    var json = JsonConvert.SerializeObject(stringsSorted, Formatting.Indented, jss);
    File.WriteAllText(Path.Combine(outputPath, "000Strings.json"), json);
}