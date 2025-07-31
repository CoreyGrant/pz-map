using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PzMap
{
    #region Options

    public class DrawingLayers
    {
        public List<Segment> RoadLayer { get; set; }
        public List<Segment> BuildingLayer { get; set; }
        public List<Segment> WaterLayer { get; set; }
        public List<Segment> DrivewayLayer { get; set; }
        public List<Segment> NaturalLayer { get; set; }
        public List<Segment> RailwayLayer { get; set; }
    }

    public class DrawingOptions
    {
        public MapBackgroundMode MapBackgroundMode { get; set; }
        public MapMode MapMode { get; set; }
    }

    public enum MapBackgroundMode
    {
        Tiled,
        Paper,
        Black
    }

    public enum MapMode
    {
        Road,
        All
    }
    #endregion

    public class RoadReader
    {
        private const string FolderLocation = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ProjectZomboid\\media\\maps";
        private const string XmlFileName = "worldmap.xml";
        private const string OutputFilename = "C:\\Dev\\logs\\project-zomboid-b41-map{0}{1}.png";
        private const int CellWidth = 300;
        private const int CellHeight = 300;
        private readonly Color PaperColor = Color.FromArgb(216, 211, 185);
        public void Generate()
        {
            var folders = Directory.GetDirectories(FolderLocation);
            var allRoadSegments = new List<Segment>();
            var allBuildingSegments = new List<Segment>();
            var allWaterSegments = new List<Segment>();
            var allDrivewaySegments = new List<Segment>();
            var allMapNames = new List<MapName>();
            var allNaturalSegments = new List<Segment>();
            var allRailwaySegments = new List<Segment>();
            foreach (var folder in folders)
            {
                if (!IsValidSubFolder(folder)) { continue; }
                Console.WriteLine($"Loading folder '{folder}'");
                var fullPath = Path.Combine(folder, XmlFileName);
                var xmlString = File.ReadAllText(fullPath);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString);
                var element = doc.DocumentElement;
                var reader = new XmlMapReader(element);
                var cells = reader.ReadFeatures();
                foreach (var cell in cells)
                {
                    foreach (var feature in cell.Features)
                    {
                        if (!feature.Properties.Any()) { continue; }
                        var property = feature.Properties.First();
                        AddSegmentsToList(allRoadSegments, "highway", property, cell, feature);
                        AddSegmentsToList(allBuildingSegments, "building", property, cell, feature);
                        AddSegmentsToList(allWaterSegments, "water", property, cell, feature);
                        AddSegmentsToList(allDrivewaySegments, "driveway", property, cell, feature);
                        AddSegmentsToList(allNaturalSegments, "natural", property, cell, feature);
                        AddSegmentsToList(allRailwaySegments, "railway", property, cell, feature);

                        if (feature.Properties.Any(x => x.Item1 == "name_en"))
                        {
                            var type = feature.Properties.Single(x => x.Item1 == "name_en").Item2;
                            foreach (var geo in feature.Geometry)
                            {
                                var points = geo.Points.Select(x => new Vector2
                                {
                                    X = x.X + CellWidth * cell.Location.X,
                                    Y = x.Y + CellHeight * cell.Location.Y
                                }).ToArray();
                                allMapNames.Add(new MapName
                                {
                                    Point = points.Single(),
                                    Name = type,
                                });
                            }
                        }
                    }
                }
            }
            var drawingLayers = new DrawingLayers 
            { 
                BuildingLayer = allBuildingSegments, 
                DrivewayLayer = allDrivewaySegments, 
                NaturalLayer = allNaturalSegments, 
                RailwayLayer = allRailwaySegments, 
                RoadLayer = allRoadSegments.OrderBy(x =>
                {
                    if (x.Type == "road_trail") { return 0; }
                    if (x.Type == "road_tertiary") { return 1; }
                    if (x.Type == "road_secondary") { return 2; }
                    return 3;
                }).ToList(),
                WaterLayer = allWaterSegments
            };
            DrawImage(drawingLayers, new DrawingOptions { MapBackgroundMode = MapBackgroundMode.Tiled, MapMode = MapMode.All });
            DrawImage(drawingLayers, new DrawingOptions { MapBackgroundMode = MapBackgroundMode.Black, MapMode = MapMode.All });
            DrawImage(drawingLayers, new DrawingOptions { MapBackgroundMode = MapBackgroundMode.Paper, MapMode = MapMode.All });
            DrawImage(drawingLayers, new DrawingOptions { MapBackgroundMode = MapBackgroundMode.Tiled, MapMode = MapMode.Road });
            DrawImage(drawingLayers, new DrawingOptions { MapBackgroundMode = MapBackgroundMode.Black, MapMode = MapMode.Road });
            DrawImage(drawingLayers, new DrawingOptions { MapBackgroundMode = MapBackgroundMode.Paper, MapMode = MapMode.Road });
        }

        private void DrawImage(DrawingLayers layers, DrawingOptions options)
        {
            Console.WriteLine($"Drawing image: {options.MapMode.ToString()} - {options.MapBackgroundMode.ToString()}");
            var imageDimensions = GetImageDimensions(layers);
            Bitmap bmpImage = new Bitmap(imageDimensions.Width, imageDimensions.Height, PixelFormat.Format32bppPArgb);
            Graphics graphics = Graphics.FromImage(bmpImage);
            var brushes = TypeColours.ToDictionary(x => x.Key, x => new SolidBrush(x.Value));
            switch (options.MapBackgroundMode)
            {
                case MapBackgroundMode.Tiled:
                    DrawImageBackground(graphics, imageDimensions);
                    break;
                case MapBackgroundMode.Paper:
                    graphics.FillRectangle(
                        new SolidBrush(PaperColor),
                        0, 0,
                        imageDimensions.Width, imageDimensions.Height);
                    break;
                case MapBackgroundMode.Black:
                    graphics.FillRectangle(
                        new SolidBrush(Color.Black),
                        0, 0,
                        imageDimensions.Width, imageDimensions.Height);
                    break;
            }

            if(options.MapBackgroundMode != MapBackgroundMode.Tiled)
            {
                DrawSegments(layers.WaterLayer, brushes, graphics, imageDimensions);
                //DrawSegments(layers.DrivewayLayer, brushes, graphics, imageDimensions);
            }
            if (options.MapMode == MapMode.All)
            {
                if (options.MapBackgroundMode != MapBackgroundMode.Tiled)
                {
                    DrawSegments(layers.NaturalLayer, brushes, graphics, imageDimensions);
                }

                DrawSegments(layers.BuildingLayer, brushes, graphics, imageDimensions);
            }
            DrawSegments(layers.RailwayLayer, brushes, graphics, imageDimensions);
            DrawSegments(layers.RoadLayer, brushes, graphics, imageDimensions);

            DrawLegend(graphics, options);

            var mapModeString = options.MapMode == MapMode.All ? "" : "-road";
            var mapModeBackgroundString = options.MapBackgroundMode == MapBackgroundMode.Tiled 
                ? "-tiled" 
                : (options.MapBackgroundMode == MapBackgroundMode.Paper ? "-paper" : "-black");
            
            var outputFilename = string.Format(OutputFilename, mapModeBackgroundString, mapModeString);
            bmpImage.Save(outputFilename, ImageFormat.Png);
        }

        private const int TileSize = 300;
        private void DrawImageBackground(Graphics graphics, ImageDimensions imageDimensions)
        {
            var projectDirectoryPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var filesDirectoryPath = Path.Combine(projectDirectoryPath, "Files");
            var filesDirectory = Directory.GetDirectories(filesDirectoryPath);
            foreach(var directory1 in filesDirectory)
            {
                var subDirectories = Directory.GetDirectories(directory1);
                foreach(var subDir in subDirectories)
                {
                    var files = Directory.GetFiles(subDir);
                    var file = files.Single(x => !x.EndsWith("_veg.png"));
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var splitFileName = fileName.Split("x");
                    var x = int.Parse(splitFileName[0]);
                    var y = int.Parse(splitFileName[1]);
                    var imageTile = File.OpenRead(file);
                    var image = Bitmap.FromStream(imageTile);
                    graphics.DrawImage(image, new PointF { X = x * TileSize + imageDimensions.XOffset, Y = y * TileSize + imageDimensions.YOffset});
                }
            }
        }

        private void AddSegmentsToList(
            List<Segment> segments,
            string key,
            (string, string) property,
            MapCell cell,
            MapFeature feature)
        {
            if (property.Item1 == key)
            {
                var type = property.Item2;
                foreach (var geo in feature.Geometry)
                {
                    var points = geo.Points.Select(x => new Vector2
                    {
                        X = x.X + CellWidth * cell.Location.X,
                        Y = x.Y + CellHeight * cell.Location.Y
                    }).ToArray();
                    segments.Add(new Segment
                    {
                        Points = points,
                        Type = key + "_" + type,
                    });
                }
            }
        }

        private void DrawSegments(
            List<Segment> segments,
            Dictionary<string, SolidBrush> brushes,
            Graphics graphics,
            ImageDimensions imageDimensions)
        {
            foreach (var segment in segments)
            {
                DrawSegment(segment, brushes, graphics, imageDimensions);
            }
        }

        private void DrawSegment(
            Segment segment,
            Dictionary<string, SolidBrush> brushes,
            Graphics graphics,
            ImageDimensions imageDimensions)
        {
            var brush = brushes[segment.Type];
            var points = segment.Points.Select(x => new PointF
            {
                X = x.X + imageDimensions.XOffset,
                Y = x.Y + imageDimensions.YOffset
            }).ToArray();
            graphics.FillPolygon(brush, points);
            if (segment.Type.Contains("building_"))
            {
                graphics.DrawPolygon(new Pen(Color.Black, 1), points);
            }
        }

        private bool IsValidSubFolder(string path)
        {
            return path.Contains("Muldraugh, KY");
        }

        private ImageDimensions GetImageDimensions(DrawingLayers drawingLayers)
        {
            var points = drawingLayers.RoadLayer
                .Concat(drawingLayers.RailwayLayer)
                .Concat(drawingLayers.WaterLayer)
                .Concat(drawingLayers.NaturalLayer)
                .Concat(drawingLayers.DrivewayLayer)
                .Concat(drawingLayers.BuildingLayer)
                .SelectMany(x => x.Points);
            var minX = points.Select(x => x.X).Min();
            var minY = points.Select(x => x.Y).Min();
            var maxX = points.Select(x => x.X).Max();
            var maxY = points.Select(x => x.Y).Max();

            var width = maxX - minX;
            var height = maxY - minY;
            var xOffset = -minX;
            var yOffset = -minY;
            return new ImageDimensions
            {
                Width = width,
                Height = height,
                XOffset = xOffset,
                YOffset = yOffset
            };
        }

        private readonly Dictionary<string, Color> TypeColours = new Dictionary<string, Color>
        {
            ["highway_trail"] = Color.Orange,
            ["highway_tertiary"] = Color.Red,
            ["highway_secondary"] = Color.Green,
            ["highway_primary"] = Color.Blue,
            ["building_RetailAndCommercial"] = Color.FromArgb(184, 205, 84),
            ["building_yes"] = Color.FromArgb(225, 176, 126),
            ["building_Industrial"] = Color.FromArgb(56, 55, 53),
            ["building_RestaurantsAndEntertainment"] = Color.FromArgb(245, 225, 60),
            ["building_CommunityServices"] = Color.FromArgb(140, 118, 235),
            ["building_Medical"] = Color.FromArgb(229, 129, 151),
            ["building_Hospitality"] = Color.FromArgb(128, 206, 225),
            ["water_river"] = Color.Turquoise,
            ["driveway_gravel"] = Color.FromArgb(118,118,114),
            ["natural_forest"] = Color.LightGreen,
            ["natural_wood"] = Color.PaleGreen,
            ["railway_*"] = Color.Gold
        };
        private readonly Dictionary<string, string> TypeLabels = new Dictionary<string, string>
        {
            ["highway_trail"] = "Trail",
            ["highway_tertiary"] = "Small road",
            ["highway_secondary"] = "Main Road",
            ["highway_primary"] = "Highway",
            ["building_RetailAndCommercial"] = "Retail and commercial",
            ["building_yes"] = "Residential",
            ["building_Industrial"] = "Industrial",
            ["building_RestaurantsAndEntertainment"] = "Resturants and entertainment",
            ["building_CommunityServices"] = "Community services",
            ["building_Medical"] = "Medical",
            ["building_Hospitality"] = "Hospitality",
            ["railway_*"] = "Railway"
        };

        private readonly string[] LegendRoadKeys = [
            "highway_primary", "highway_secondary", "highway_tertiary", "highway_trail",
            "railway_*"
        ];

        private readonly string[] LegendBuildingKeys = [
            "building_CommunityServices",
            "building_RetailAndCommercial",
            "building_Industrial",
            "building_yes",
            "building_RestaurantsAndEntertainment",
            "building_Hospitality",
            "building_Medical",
        ];

        private void DrawLegend(Graphics graphics, DrawingOptions options)
        {
            // the legend should show the colour with descriptive text
            // paper/tiled use same bg, black text
            // black uses black bg, white text
            var textColor = options.MapBackgroundMode == MapBackgroundMode.Black ? Color.White : Color.Black;
            var bgColor = options.MapBackgroundMode == MapBackgroundMode.Black ? Color.Black : PaperColor;

            var lineHeight = 160;
            var fontSize = 100;
            var lineSpacing = 50;
            var legendTopLeftOffset = 200;
            var legendWidth = 2200;

            var totalHeight = lineSpacing;
            var textBrush = new SolidBrush(textColor);
            var textFont = new Font("Arial", fontSize);

            var fullHeight = (LegendRoadKeys.Length * (lineSpacing + lineHeight)) + lineSpacing
                + (options.MapMode != MapMode.Road ? (lineHeight  + (LegendBuildingKeys.Length * (lineSpacing + lineHeight))) : 0);

            graphics.FillRectangle(new SolidBrush(bgColor),
                legendTopLeftOffset,
                legendTopLeftOffset,
                legendWidth,
                fullHeight);

            foreach (var roadKey in LegendRoadKeys)
            {
                var color = TypeColours[roadKey];
                var text = TypeLabels[roadKey];

                graphics.FillRectangle(
                    new SolidBrush(color),
                    legendTopLeftOffset + lineSpacing,
                    legendTopLeftOffset + totalHeight,
                    lineHeight, lineHeight);
                graphics.DrawRectangle(new Pen(textColor, 4), legendTopLeftOffset + lineSpacing,
                    legendTopLeftOffset + totalHeight,
                    lineHeight, lineHeight);

                graphics.DrawString(
                    text,
                    textFont,
                    textBrush,
                    legendTopLeftOffset + lineSpacing + lineHeight + lineSpacing,
                    legendTopLeftOffset + totalHeight);
                totalHeight += lineSpacing + lineHeight;
            }
            if (options.MapMode != MapMode.Road)
            {
                totalHeight += lineHeight;

                foreach (var buildingKey in LegendBuildingKeys)
                {
                    var color = TypeColours[buildingKey];
                    var text = TypeLabels[buildingKey];
                    graphics.FillRectangle(
                        new SolidBrush(color),
                        legendTopLeftOffset + lineSpacing,
                        legendTopLeftOffset + totalHeight,
                        lineHeight, lineHeight);
                    graphics.DrawRectangle(new Pen(textColor, 4), legendTopLeftOffset + lineSpacing,
                        legendTopLeftOffset + totalHeight,
                        lineHeight, lineHeight);

                    graphics.DrawString(
                        text,
                        textFont,
                        textBrush,
                        legendTopLeftOffset + lineSpacing + lineHeight + lineSpacing,
                        legendTopLeftOffset + totalHeight);
                    totalHeight += lineSpacing + lineHeight;
                }
            }
            graphics.DrawRectangle(new Pen(textColor, 4),
                legendTopLeftOffset,
                legendTopLeftOffset,
                legendWidth,
                totalHeight);

            DrawAttribution(graphics, textColor);
        }

        private void DrawAttribution(Graphics graphics, Color textColor)
        {
            graphics.DrawString("Project Zomboid Build 41 Map", new Font("Arial", 150), new SolidBrush(textColor), new PointF { X = 3000, Y = 200 });
            //graphics.DrawString("By Useablelobster", new Font("Arial", 100), new SolidBrush(Color.Black), new PointF { X = 3000, Y = 500 });
        }
    }

    

    public class ImageDimensions
    {
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Segment
    {
        public Vector2[] Points { get; set; }
        public string Type { get; set; }
    }

    public class MapName
    {
        public Vector2 Point { get; set; }
        public string Name { get; set; }
    }
}
