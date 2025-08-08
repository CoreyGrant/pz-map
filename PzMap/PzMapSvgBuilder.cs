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

        public PzMapSvgBuilder(GameDataReader gameDataReader) 
        {
            _gameDataReader = gameDataReader;
        }

        public (string, string, string) BuildSvg()
        {
            var drawingLayers = _gameDataReader.ReadGameData();
            var imageDimensions = GetImageDimensions(drawingLayers);
            var svgWriter = new SvgWriter();
            var metadataDict = new Dictionary<string, Dictionary<string, string>>();
            
            AddToSvg(drawingLayers.WaterLayer, svgWriter);
            AddToSvg(drawingLayers.RailwayLayer, svgWriter); 
            AddToSvg(drawingLayers.BuildingLayer, svgWriter);
            AddToSvg(drawingLayers.RoadLayer, svgWriter);

            AddToMetadata(drawingLayers.BuildingLayer, metadataDict);
            AddToMetadata(drawingLayers.RoadLayer, metadataDict);

            var metadata = JsonConvert.SerializeObject(metadataDict);

            var info = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                ["mapWidth"] = imageDimensions.Width,
                ["mapHeight"] = imageDimensions.Height,
            });
            return (svgWriter.ToString(), metadata, info);
        }

        private void AddToSvg(List<Segment> segments, SvgWriter svgWriter)
        {
            foreach(var segment in segments)
            {
                svgWriter.AddPolygon(segment.Points, segment.Type, segment.Key, segment.Id, GetColor(segment.Type, segment.Key));
            }
        }

        private void AddToMetadata(List<Segment> segments, Dictionary<string, Dictionary<string, string>> metadata)
        {
            foreach (var segment in segments) 
            {
                metadata[segment.Id] = new Dictionary<string, string>();
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
            ["driveway_gravel"] = Color.FromArgb(118, 118, 114),
            ["natural_forest"] = Color.LightGreen,
            ["natural_wood"] = Color.PaleGreen,
            ["railway_*"] = Color.Gold
        };
    }
}
