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

        public void AddPolygon(IEnumerable<Vector2> points, string type, string key, string id, (Color?, Color?) color)
        {
            AddPolygon(new SvgPolygon(points, type, key, id, color));
        }

        public void AddPolygon(SvgPolygon polygon)
        {
            _sb.AppendLine(polygon.ToString());
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
            return $@"<polygon 
    stroke=""{ColorToString(stroke)}""
    stroke-width=""1""
    type=""{Type}""
    key=""{Key}""
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
