using Newtonsoft.Json;
using PzMap.Types;
using PzMap.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PzMap
{
    public class PzMapSvgBuilder
    {
        private readonly GameDataReader _gameDataReader;
        private readonly PzMapRoomReader _pzMapRoomReader;
        private readonly string _outputPath;
        private readonly string _assetPath;

        public PzMapSvgBuilder(
            GameDataReader gameDataReader,
            PzMapRoomReader pzMapRoomReader,
            string outputPath,
            string assetPath) 
        {
            _gameDataReader = gameDataReader;
            _pzMapRoomReader = pzMapRoomReader;
            _outputPath = outputPath;
            _assetPath = assetPath;
        }
        public void BuildAndSaveVersions(List<VersionSettings> versionSettings)
        {
            foreach(var settings in versionSettings)
            {
                var version = settings.VersionNumber;
                var (svg, metadata, info) = BuildSvg(settings);
                var svgOutputPath = Path.Combine(_outputPath, $"b{version}-svg.svg");
                var infoOutputPath = Path.Combine(_outputPath, $"b{version}-info.json");
                var metadataOutputPath = Path.Combine(_outputPath, $"b{version}-metadata.json");
                File.WriteAllText(svgOutputPath, svg);
                File.WriteAllText(infoOutputPath, info);
                File.WriteAllText(metadataOutputPath, metadata);
            }
            var versionsJson = JsonConvert.SerializeObject(versionSettings.Select(x => x.VersionNumber).ToList());
            File.WriteAllText(Path.Combine(_outputPath, "versions.json"), versionsJson);
        }

        public (string, string, string) BuildSvg(VersionSettings settings)
        {
            var drawingLayers = _gameDataReader.ReadGameData(
                Path.Combine(_assetPath, "b" + settings.VersionNumber),
                settings);
            var imageDimensions = GetImageDimensions(drawingLayers);
            var svgWriter = new SvgWriter(imageDimensions);
            var metadataDict = _pzMapRoomReader.AssignBuildingTypesFromLotHeaders(drawingLayers.BuildingLayer, settings);

            svgWriter.AddStyleOptions();
            AddToSvg(drawingLayers.WaterLayer, svgWriter); 
            AddToSvg(drawingLayers.RailwayLayer, svgWriter); 
            AddToSvg(drawingLayers.RoadLayer, svgWriter);
            AddToSvg(drawingLayers.BuildingLayer, svgWriter);
            svgWriter.AddBounding();

            var metadata = JsonConvert.SerializeObject(metadataDict);

            var info = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["mapWidth"] = imageDimensions.Width,
                ["mapHeight"] = imageDimensions.Height,
                ["offsetX"] = imageDimensions.XOffset,
                ["offsetY"] = imageDimensions.YOffset
            });
            return (svgWriter.ToString(), metadata, info);
        }

        private void AddToSvg(List<Segment> segments, SvgWriter svgWriter)
        {
            foreach (var segment in segments)
            {
                svgWriter.AddPolygon(
                    segment.Points, 
                    segment.Key == "building" ? segment.Type : null,
                    segment.Key == "building" ? segment.Key : null, 
                    segment.Id, 
                    GetColor(segment.Type, segment.Key), 
                    segment.Name, 
                    segment.Floors);
            }
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

        private (Color?, Color?) GetColor(string type, string key)
        {
            Color? fill = null;
            Color? stroke = null;
            switch (key)
            {
                case "building":
                    if(type == "RetailAndCommercial") { fill = Color.FromArgb(184, 205, 84); }
                    if(type == "yes") { fill = Color.FromArgb(225, 176, 126); }
                    if(type == "Industrial") { fill = Color.FromArgb(56, 55, 53); }
                    if(type == "RestaurantsAndEntertainment") { fill = Color.FromArgb(245, 225, 60); }
                    if(type == "CommunityServices") { fill = Color.FromArgb(140, 118, 235); }
                    if(type == "Medical") { fill = Color.FromArgb(229, 129, 151); }
                    if(type == "Hospitality") { fill = Color.FromArgb(128, 206, 225); }
                    stroke = Color.Black;
                    break;
                case "water":
                    fill = Color.FromArgb(68, 139, 146);
                    break;
                case "highway":
                    if(type == "trail") { fill = Color.Orange; } 
                    if(type == "tertiary") { fill = Color.Red; } 
                    if(type == "secondary") { fill = Color.Green; } 
                    if(type == "primary") { fill = Color.Blue; }
                    break;
                case "railway":
                    fill = Color.Gold;
                    break;
            }
            return (fill, stroke);
        }
    }

    public class VersionSettings
    {
        public int VersionNumber { get; set; }
        public int CellWidth { get; set; }
        public int CellHeight { get; set; }
    }
}
