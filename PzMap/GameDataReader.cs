using PzMap.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PzMap
{
    public class GameDataReader
    {
        private readonly string _mapFolder;
        private readonly string _xmlFileName;
        private readonly int _cellWidth;
        private readonly int _cellHeight;

        public GameDataReader(
            string mapFolder = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\ProjectZomboid\\media\\maps",
            string xmlFileName = "worldmap.xml",
            int cellWidth = 300,
            int cellHeight = 300)
        {
            _mapFolder = mapFolder;
            _xmlFileName = xmlFileName;
            _cellWidth = cellWidth;
            _cellHeight = cellHeight;
        }

        public DrawingLayers ReadGameData()
        {
            var folders = Directory.GetDirectories(_mapFolder);
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
                var fullPath = Path.Combine(folder, _xmlFileName);
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
                                    X = x.X + _cellWidth * cell.Location.X,
                                    Y = x.Y + _cellHeight * cell.Location.Y
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

            return drawingLayers;
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
                        X = x.X + _cellWidth * cell.Location.X,
                        Y = x.Y + _cellHeight * cell.Location.Y
                    }).ToArray();
                    segments.Add(new Segment
                    {
                        Points = points,
                        Type = type,
                        Key = key,
                        Id = string.Join(".", points.Select(x => x.X + "," + x.Y))
                    });
                }
            }
        }

        private bool IsValidSubFolder(string path)
        {
            return path.Contains("Muldraugh, KY");
        }
    }
}
