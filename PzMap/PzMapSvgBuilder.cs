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

        public PzMapSvgBuilder(
            GameDataReader gameDataReader,
            PzMapRoomReader pzMapRoomReader) 
        {
            _gameDataReader = gameDataReader;
            _pzMapRoomReader = pzMapRoomReader;
        }

        public (string, string, string) BuildSvg()
        {
            var drawingLayers = _gameDataReader.ReadGameData();
            var imageDimensions = GetImageDimensions(drawingLayers);
            var svgWriter = new SvgWriter();
            var metadataDict = _pzMapRoomReader.AssignBuildingTypesFromLotHeaders(drawingLayers.BuildingLayer);

            svgWriter.AddStyleOptions();
            AddToSvg(drawingLayers.WaterLayer, svgWriter); 
            AddToSvg(drawingLayers.RailwayLayer, svgWriter); 
            AddToSvg(drawingLayers.RoadLayer, svgWriter);
            AddToSvg(drawingLayers.BuildingLayer, svgWriter);
            svgWriter.AddBounding(imageDimensions);

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

            foreach (var segment in segments)
            {
                svgWriter.AddPolygon(segment.Points, segment.Type, segment.Key, segment.Id, GetColor(segment.Type, segment.Key), segment.Name);
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
}
