using PzMap.Types;
using PzMap.Types.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PzMap.Utilities
{
    public class SvgWriter
    {
        private StringBuilder _sb;
        private readonly ImageDimensions _imageDimensions;

        public SvgWriter(ImageDimensions imageDimensions)
        {
            _sb = new StringBuilder();
            _imageDimensions = imageDimensions;
        }

        public void AddPolygon(IEnumerable<Vector2> points, string type, string key, string id, (Color?, Color?) color, string? name, int floors)
        {
            points = points.Select(x => new Vector2
            {
                X = x.X + _imageDimensions.XOffset,
                Y = x.Y + _imageDimensions.YOffset,
            });
            AddPolygon(new SvgPolygon(points, type, key, id, color) { Name = name, Floors = floors });
        }

        private void AddPolygon(SvgPolygon polygon)
        {
            _sb.AppendLine(polygon.ToString());
        }

        public void AddBounding()
        {
            var points = new List<Vector2>
            {
                new Vector2{X = 0, Y = 0},
                new Vector2{X = 0, Y = _imageDimensions.Height},
                new Vector2{X = _imageDimensions.Width, Y = _imageDimensions.Height},
                new Vector2{X = _imageDimensions.Width, Y = 0}
            };
            var polygon = new SvgPolygon(points, "", "", "bounding-box", (null, Color.Black)) { StrokeWidth = 10 };
            AddPolygon(polygon);
        }

        public void AddStyleOptions()
        {
            _sb.AppendLine("<pattern id=\"diagonalHatch\" patternUnits=\"userSpaceOnUse\" width=\"4\" height=\"4\">\r\n  <path d=\"M-1,1 l2,-2\r\n           M0,4 l4,-4\r\n           M3,5 l2,-2\" \r\n        style=\"stroke:black; stroke-width:1\" />\r\n</pattern>");
        }

        public string ToString()
        {
            return $@"<svg style=""background-color: rgb(216, 211, 185)"" preserveAspectRatio=""xMidYMid"">
    {_sb.ToString()}
</svg>";
        }
    }

    public class SvgPolygon
    {
        public static int _id = 0;
        public IEnumerable<Vector2> Points { get; }
        public string Type { get; }
        public string Key { get; }
        public string Id { get; }
        public (Color?, Color?) Color { get; }
        public string Name { get; set; }
        public int Floors { get; set; }
        public int StrokeWidth { get; set; } = 1;
        public SvgPolygon(IEnumerable<Vector2> points, string type, string key, string id, (Color?, Color?) color)
        {
            Points = points;
            Type = type;
            Key = key;
            Id = id;
            Color = color;
        }

        public override string ToString()
        {
            var (fill, stroke) = Color;
            var xPoints = Points.Select(x => x.X);
            var yPoints = Points.Select(x => x.Y);
            var midPointX = (xPoints.Max() + xPoints.Min())/2;
            var midPointY = (yPoints.Max() + yPoints.Min())/2;
            var type = string.IsNullOrEmpty(Type) ? "" : @$"type=""{Type}""";
            var key = string.IsNullOrEmpty(Key) ? "" : $@"key=""{Key}""";
            var name = string.IsNullOrEmpty(Name) ? "" : $@"name=""{Name}""";
            var midpoint = string.IsNullOrEmpty(Key) ? "" : $@"x=""{midPointX}"" y=""{midPointY}""";
            return $@"<polygon stroke=""{ColorToString(stroke)}"" stroke-width=""{StrokeWidth}"" {type} {key} {name} floors=""{Floors}"" {midpoint} fill=""{ColorToString(fill)}"" points=""{string.Join(" ", Points.Select(x => x.X + "," + x.Y))}"" id=""{Id}""/>";
        }

        private string ColorToString(Color? color)
        {
            if (!color.HasValue)
            {
                return "none";
            }
            return $"rgb({color.Value.R}, {color.Value.G}, {color.Value.B})";
        }
    }
}
