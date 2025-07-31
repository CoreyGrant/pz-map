using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PzMap
{
    public class XmlMapReader
    {
        private readonly XmlElement _xmlElement;

        public XmlMapReader(XmlElement xmlElement) 
        {
            _xmlElement = xmlElement;
        }

        public List<MapCell> ReadFeatures()
        {
            var cellsOutput = new List<MapCell>();
            var cells = _xmlElement.GetElementsByTagName("cell");
            foreach (XmlElement cell in cells)
            {
                var mapCell = new MapCell();
                var cellX = int.Parse(cell.GetAttribute("x"));
                var cellY = int.Parse(cell.GetAttribute("y"));
                mapCell.Location = new Vector2 { X = cellX, Y = cellY };
                var mapFeatures = new List<MapFeature>();
                var features = cell.GetElementsByTagName("feature");
                foreach (XmlElement feature in features)
                {
                    var geometries = feature.GetElementsByTagName("geometry");
                    var mapFeature = new MapFeature();
                    var mapGeometries = new List<MapFeatureGeometry>();
                    foreach(XmlElement geom in geometries)
                    {
                        var mapGeometry = new MapFeatureGeometry();
                        mapGeometry.Type = geom.GetAttribute("type");
                        var mapPoints = new List<Vector2>();
                        var coordinates = geom.GetElementsByTagName("coordinates");
                        foreach(XmlElement coord in coordinates)
                        {
                            var points = coord.GetElementsByTagName("point");
                            foreach(XmlElement point in points)
                            {
                                mapPoints.Add(new Vector2
                                {
                                    X = int.Parse(point.GetAttribute("x")),
                                    Y = int.Parse(point.GetAttribute("y"))
                                });
                            }
                        }
                        mapGeometry.Points = mapPoints.ToArray();
                        mapGeometries.Add(mapGeometry);
                    }
                    mapFeature.Geometry = mapGeometries.ToArray();

                    var properties = feature.GetElementsByTagName("properties");
                    var mapProperties = new List<(string, string)>();
                    foreach(XmlElement prop in properties)
                    {
                        var propList = prop.GetElementsByTagName("property");
                        foreach(XmlElement propListItem in propList)
                        {
                            mapProperties.Add((
                                propListItem.GetAttribute("name"),
                                propListItem.GetAttribute("value")
                            ));
                        }
                    }
                    mapFeature.Properties = mapProperties.ToArray();
                    mapFeatures.Add(mapFeature);
                }
                mapCell.Features = mapFeatures.ToArray();
                cellsOutput.Add(mapCell);
            }
            return cellsOutput;
        }
    }

    public class MapCell
    {
        public Vector2 Location { get; set; }
        public MapFeature[] Features { get; set; }
    }

    public class MapFeature
    {
        public MapFeatureGeometry[] Geometry { get; set; }
        public (string, string)[] Properties { get; set; }
    }

    public class MapFeatureGeometry
    {
        public Vector2[] Points { get; set; }
        public string Type { get; set; }
    }
    
    public struct Vector2
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
