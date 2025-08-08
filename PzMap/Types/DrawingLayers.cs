using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PzMap.Types
{
    public class DrawingLayers
    {
        public List<Segment> RoadLayer { get; set; }
        public List<Segment> BuildingLayer { get; set; }
        public List<Segment> WaterLayer { get; set; }
        public List<Segment> DrivewayLayer { get; set; }
        public List<Segment> NaturalLayer { get; set; }
        public List<Segment> RailwayLayer { get; set; }
    }
}
