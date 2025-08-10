using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PzMapTools
{
    public class LotHeaderReader
    {
        private const byte StringDelim = 0x0A;

        public List<LotHeaderFile> ReadFolder(string lotHeaderFolder)
        {
            return Directory.GetFiles(lotHeaderFolder)
                .Where(x => Path.GetExtension(x) == ".lotheader")
                .Select(x => Read(File.OpenRead(x), x))
                .ToList();
        }

        public LotHeaderFile Read(Stream stream, string filename)
        {
            var allBytes = new byte[stream.Length];
            stream.ReadExactly(allBytes);
            stream.Position = 0;
            var file = new LotHeaderFile();
            file.FormatVersion = ReadInt(stream);
            file.NumberOfTiles = ReadInt(stream);
            file.UnknownStrings = ReadStrings(stream);
            file.Width = ReadInt(stream);
            file.Height = ReadInt(stream);
            file.VerticalLevels = ReadInt(stream);
            file.RoomCount = ReadInt(stream);
            file.Rooms = ReadRooms(file.RoomCount, stream);
            file.BuildingCount = ReadInt(stream);
            file.Buildings = ReadBuildings(file.BuildingCount, stream);
            var name = Path.GetFileNameWithoutExtension(filename);
            var split = name.Split("_").Select(x => int.Parse(x)).ToArray();
            file.CellX = split[0];
            file.CellY = split[1];
            return file;
        }

        private int ReadInt(Stream stream)
        {
            var intValue = new byte[4];
            stream.ReadExactly(intValue);
            return BitConverter.ToInt32(intValue);
        }

        private string ReadString(Stream stream)
        {
            var bytes = new List<byte>();
            while (true)
            {
                var stringByte = (byte)stream.ReadByte();
                if (stringByte == StringDelim)
                {
                    return new string(bytes.Select(x => (char)x).ToArray());
                }
                bytes.Add(stringByte);
            }
        }

        private string[] ReadStrings(Stream stream)
        {
            var strings = new List<string>();
            while (true)
            {
                var peekedByte = stream.ReadByte();
                if (peekedByte == 0x00) { return strings.ToArray(); }
                stream.Position--;
                strings.Add(ReadString(stream));
            }
        }

        private List<Room> ReadRooms(int roomCount, Stream stream)
        {
            var rooms = new List<Room>();
            for (var i = 0; i < roomCount; i++)
            {
                var room = new Room();
                room.Name = ReadString(stream);
                room.Level = ReadInt(stream);
                room.RectangleCount = ReadInt(stream);
                room.Rectangles = ReadRoomRectangles(room.RectangleCount, stream);
                room.ObjectCount = ReadInt(stream);
                room.Objects = ReadRoomObjects(room.ObjectCount, stream);
                rooms.Add(room);
            }
            return rooms;
        }

        private List<RoomRectangle> ReadRoomRectangles(int rectangleCount, Stream stream)
        {
            var roomRectangles = new List<RoomRectangle>();
            for (var i = 0; i < rectangleCount; i++)
            {
                var roomRectangle = new RoomRectangle();
                roomRectangle.X = ReadInt(stream);
                roomRectangle.Y = ReadInt(stream);
                roomRectangle.Width = ReadInt(stream);
                roomRectangle.Height = ReadInt(stream);
                roomRectangles.Add(roomRectangle);
            }
            return roomRectangles;
        }

        private List<RoomObject> ReadRoomObjects(int objectCount, Stream stream)
        {
            var roomObjects = new List<RoomObject>();
            for (var i = 0; i < objectCount; i++)
            {
                var roomObject = new RoomObject();
                roomObject.Id = ReadInt(stream);
                roomObject.X = ReadInt(stream);
                roomObject.Y = ReadInt(stream);
                roomObjects.Add(roomObject);
            }
            return roomObjects;
        }

        private List<Building> ReadBuildings(int buildingCount, Stream stream)
        {
            var buildings = new List<Building>();
            for (var i = 0; i < buildingCount; i++)
            {
                var building = new Building();
                building.RoomCount = ReadInt(stream);
                building.RoomIndices = new List<int>();
                for (var j = 0; j < building.RoomCount; j++)
                {
                    building.RoomIndices.Add(ReadInt(stream));
                }
                buildings.Add(building);
            }
            return buildings;
        }
    }

    public class LotHeaderFile
    {
        public int CellX { get; set; }
        public int CellY { get; set; }
        public int FormatVersion { get; set; }
        public int NumberOfTiles { get; set; }
        public string[] UnknownStrings { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int VerticalLevels { get; set; }
        public int RoomCount { get; set; }
        public List<Room> Rooms { get; set; }
        public int BuildingCount { get; set; }
        public List<Building> Buildings { get; set; }
    }

    public class Room
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int RectangleCount { get; set; }
        public List<RoomRectangle> Rectangles { get; set; }
        public int ObjectCount { get; set; }
        public List<RoomObject> Objects { get; set; }
    }

    public class Building
    {
        public int RoomCount { get; set; }

        public List<int> RoomIndices { get; set; }
    }

    public class RoomRectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class RoomObject
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }
}
