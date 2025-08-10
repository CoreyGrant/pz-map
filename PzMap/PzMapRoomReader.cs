using PzMap.Types;
using PzMapTools;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace PzMap
{
    public class PzMapRoomReader
    {
        private readonly LotHeaderReader _lotHeaderReader;
        private readonly string _lotHeaderPath;
        private readonly int _cellWidth;
        private readonly int _cellHeight;

        // read all the lot headers,
        // map the positions using cell/location within cell
        // pull out rooms
        // get the ids for each room
        // figure out which buildings from the svg mapgen are which type

        public PzMapRoomReader(LotHeaderReader lotHeaderReader, string lotHeaderPath, int cellWidth = 300, int cellHeight = 300) 
        {
            _lotHeaderReader = lotHeaderReader;
            _lotHeaderPath = lotHeaderPath;
            _cellWidth = cellWidth;
            _cellHeight = cellHeight;
        }

        public Dictionary<string, object> AssignBuildingTypesFromLotHeaders(List<Segment> buildingSegments)
        {
            var lotHeaders = _lotHeaderReader.ReadFolder(_lotHeaderPath);
            var combinedLotHeaders = CombineLotHeaders(lotHeaders);
            return AssignBuildingTypes(buildingSegments, combinedLotHeaders);
        }

        private List<PzMapRoom> CombineLotHeaders(
            List<LotHeaderFile> lotHeaders)
        {
            var mappedRooms = new List<PzMapRoom>();
            foreach(var header in lotHeaders)
            {
                mappedRooms.AddRange(header.Rooms.Select(x => new PzMapRoom
                {
                    Name = x.Name,
                    Rectangles = x.Rectangles.Select(x => new PzMapRoomRectangle
                    {
                        X = x.X + header.CellX * _cellWidth,
                        Y = x.Y + header.CellY * _cellHeight,
                        Width = x.Width,
                        Height = x.Height,
                        Midpoint = new System.Numerics.Vector2
                        {
                            X = x.X + header.CellX * _cellWidth + ((float)x.Width)/2,
                            Y = x.Y + header.CellY * _cellHeight + ((float)x.Height)/2,
                        }
                    }).ToList()
                }));
            }

            return mappedRooms;
        }

        private Dictionary<string, object> AssignBuildingTypes(
            List<Segment> buildingSegments,
            List<PzMapRoom> rooms)
        {
            var buildingTypeMetadata = new Dictionary<string, object>();

            var metadataRooms = new Dictionary<string, object>();
            buildingTypeMetadata["rooms"] = metadataRooms;
            Console.WriteLine("Calculating metadata " + buildingSegments.Count);
            var i = 0;
            foreach(var segment in buildingSegments)
            {
                // calculate a maximum bounding box for the segment
                // calculate the middle of a relevant room

                // find all rooms whose midpoints are inside the building segment

                var maxX = segment.Points.Select(x => x.X).Max();
                var minX = segment.Points.Select(x => x.X).Min();
                var maxY = segment.Points.Select(x => x.Y).Max();
                var minY = segment.Points.Select(x => x.Y).Min();
                List<string> matchingRooms = new List<string>();
                foreach(var room in rooms)
                {
                    var isInBuilding = room.Rectangles.All(x =>
                    {
                        return x.Midpoint.X < maxX
                            && x.Midpoint.Y < maxY
                            && x.Midpoint.X > minX
                            && x.Midpoint.Y > minY;
                    });
                    if (isInBuilding)
                    {
                        matchingRooms.Add(room.Name);
                    }
                }
                metadataRooms[segment.Id] = matchingRooms.Distinct().Order();
                if(i % 100 == 0) { Console.WriteLine("Calculated " + i); }
                i++;
            }
            var lotHeaderNames = rooms.Select(x => x.Name).Distinct();
            var names = new List<string>();
            foreach (var name in lotHeaderNames)
            {
                if (!names.Contains(name)) { names.Add(name); }
            }
            buildingTypeMetadata["roomNames"] = names.Order();
            return buildingTypeMetadata;
        }
    }

    public class PzMapRoom
    {
        public string Name { get; set; }
        public List<PzMapRoomRectangle> Rectangles { get; set; }
    }

    public class PzMapRoomRectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public System.Numerics.Vector2 Midpoint { get; set; }
    }
}
