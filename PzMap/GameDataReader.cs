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
        private readonly string _xmlFileName;
        private int _id = 0;
        public GameDataReader(
            string xmlFileName = "worldmap.xml")
        {
            _xmlFileName = xmlFileName;
        }

        public DrawingLayers ReadGameData(string folder, VersionSettings settings)
        {
            var allRoadSegments = new List<Segment>();
            var allBuildingSegments = new List<Segment>();
            var allWaterSegments = new List<Segment>();
            var allDrivewaySegments = new List<Segment>();
            var allMapNames = new List<MapName>();
            var allNaturalSegments = new List<Segment>();
            var allRailwaySegments = new List<Segment>();

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
                    AddSegmentsToList(allRoadSegments, "highway", property, cell, feature, settings);
                    AddSegmentsToList(allBuildingSegments, "building", property, cell, feature, settings);
                    AddSegmentsToList(allWaterSegments, "water", property, cell, feature, settings, true);
                    AddSegmentsToList(allDrivewaySegments, "driveway", property, cell, feature, settings);
                    AddSegmentsToList(allNaturalSegments, "natural", property, cell, feature, settings);
                    AddSegmentsToList(allRailwaySegments, "railway", property, cell, feature, settings);

                    if (feature.Properties.Any(x => x.Item1 == "name_en"))
                    {
                        var type = feature.Properties.Single(x => x.Item1 == "name_en").Item2;
                        foreach (var geo in feature.Geometry)
                        {
                            var points = geo.Points.Select(x => new Vector2
                            {
                                X = x.X + settings.CellWidth * cell.Location.X,
                                Y = x.Y + settings.CellHeight * cell.Location.Y
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
            MapFeature feature,
            VersionSettings settings,
            bool overlap = false)
        {
            if (property.Item1 == key)
            {
                var type = property.Item2;
                foreach (var geo in feature.Geometry)
                {
                    if (overlap)
                    {
                        geo.Points = geo.Points.Select(p =>
                        {
                            var x = p.X;
                            var y = p.Y;
                            if(x == 0) { x -= 2; }
                            if(x == settings.CellWidth) { x += 2; }
                            if (y == 0) { y -= 2; }
                            if (y == settings.CellHeight) { y += 2; }
                            return new Vector2 { X = x, Y = y };
                        }).ToArray();
                    }
                    var points = geo.Points.Select(x => new Vector2
                    {
                        X = x.X + settings.CellWidth * cell.Location.X,
                        Y = x.Y + settings.CellHeight * cell.Location.Y
                    }).ToArray();
                    segments.Add(new Segment
                    {
                        Points = points,
                        Type = type,
                        Key = key,
                        Id = (_id++).ToString()
                    });
                }
            }
        }
    }
}
