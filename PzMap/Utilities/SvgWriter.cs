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
        public SvgWriter()
        {
            _sb = new StringBuilder();
        }

        public void AddPolygon(IEnumerable<Vector2> points, string type, string key, string id, (Color?, Color?) color, string? name, int floors)
        {
            AddPolygon(new SvgPolygon(points, type, key, id, color) { Name = name, Floors = floors });
        }

        public void AddPolygon(SvgPolygon polygon)
        {
            _sb.AppendLine(polygon.ToString());
        }

        public void AddBounding(ImageDimensions imageDimensions)
        {
            var offsetX = -imageDimensions.XOffset;
            var offsetY = -imageDimensions.YOffset;
            var points = new List<Vector2>
            {
                new Vector2{X = offsetX, Y = offsetY},
                new Vector2{X = offsetX, Y = imageDimensions.Height + offsetY},
                new Vector2{X = imageDimensions.Width + offsetX, Y = imageDimensions.Height + offsetY},
                new Vector2{X = imageDimensions.Width + offsetX, Y = offsetY}
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
            return $@"<svg style=""background-color: rgb(216, 211, 185)"">
    {_sb.ToString()}
</svg>";
        }
    }

    public class SvgPolygon
    {
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
            return $@"<polygon 
    stroke=""{ColorToString(stroke)}""
    stroke-width=""{StrokeWidth}""
    type=""{Type}""
    key=""{Key}""
    name=""{Name}""
    floors=""{Floors}""
    midpoint-x=""{midPointX}""
    midpoint-y=""{midPointY}""
    fill=""{ColorToString(fill)}""
    points=""{string.Join(" ", Points.Select(x => x.X + "," + x.Y))}"" 
    id=""{Id}""/>";
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
