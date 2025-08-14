//// See https://aka.ms/new-console-template for more information

//using Newtonsoft.Json;
//using PzMapTools;
//using System.Linq;
Console.WriteLine("Hello world");
//var path = @"C:\Program Files (x86)\Steam\steamapps\common\ProjectZomboid\media\maps\Muldraugh, KY";

//var outputFolder = @"C:\Dev\logs";

//var files = Directory.GetFiles(path).Where(x => Path.GetExtension(x) == ".lotheader" );

//var names = new List<string>();
//var lotHeaderReader = new LotHeaderReader();
//foreach (var file in files)
//{
//    //Console.WriteLine("Handling " + file);
//    var stream = File.OpenRead(file);
//    var lotHeader = lotHeaderReader.Read(stream, file);

//    //var updatedFilename = Path.GetFileNameWithoutExtension(file) + ".json";
//    //File.WriteAllText(
//    //    Path.Combine(outputFolder, updatedFilename),
//    //    JsonConvert.SerializeObject(lotHeader, Formatting.Indented));
//    var lotHeaderNames = lotHeader.Rooms.Select(x => x.Name).Distinct();
//    foreach (var name in lotHeaderNames)
//    {
//        if (!names.Contains(name)) { names.Add(name); }
//    }
//}
//File.WriteAllText(Path.Combine(outputFolder, "AAANames.json"), JsonConvert.SerializeObject(names.Order(), Formatting.Indented));
