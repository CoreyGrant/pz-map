using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PzMap
{
    class LotHeaderReader
    {
        private readonly Stream _stream;
        private const byte StringDelim = 0x0A;

        public LotHeaderReader(Stream stream) 
        {
            _stream = stream;
        }
        public LotHeaderFile Read()
        {
            var allBytes = new byte[_stream.Length];
            _stream.ReadExactly(allBytes);
            _stream.Position = 0;
            var file = new LotHeaderFile();
            file.FormatVersion = ReadInt();
            file.NumberOfTiles = ReadInt();
            file.UnknownStrings = ReadStrings();
            file.Width = ReadInt();
            file.Height = ReadInt();
            file.VerticalLevels = ReadInt();
            file.RoomCount = ReadInt();
            file.Rooms = ReadRooms(file.RoomCount);
            file.BuildingCount = ReadInt();
            file.Buildings = ReadBuildings(file.BuildingCount);
            return file;
        }

        private int ReadInt()
        {
            var intValue = new byte[4];
            _stream.ReadExactly(intValue);
            return BitConverter.ToInt32(intValue);
        }

        private string ReadString()
        {
            var bytes = new List<byte>();
            while (true)
            {
                var stringByte = (byte)_stream.ReadByte();
                if(stringByte == StringDelim)
                {
                    return new string(bytes.Select(x => (char)x).ToArray());
                }
                bytes.Add(stringByte);
            }
        }

        private string[] ReadStrings()
        {
            var strings = new List<string>();
            while (true)
            {
                var peekedByte = _stream.ReadByte();
                if(peekedByte == 0x00) { return strings.ToArray(); }
                _stream.Position--;
                strings.Add(ReadString());
            }
        }

        private List<Room> ReadRooms(int roomCount)
        {
            var rooms = new List<Room>();
            for(var i = 0; i < roomCount; i++)
            {
                var room = new Room();
                room.Name = ReadString();
                room.Level = ReadInt();
                room.RectangleCount = ReadInt();
                room.Rectangles = ReadRoomRectangles(room.RectangleCount);
                room.ObjectCount = ReadInt();
                room.Objects = ReadRoomObjects(room.ObjectCount);
                rooms.Add(room);
            }
            return rooms;
        }

        private List<RoomRectangle> ReadRoomRectangles(int rectangleCount)
        {
            var roomRectangles = new List<RoomRectangle>();
            for (var i = 0; i < rectangleCount; i++)
            {
                var roomRectangle = new RoomRectangle();
                roomRectangle.X = ReadInt();
                roomRectangle.Y = ReadInt();
                roomRectangle.Width = ReadInt();
                roomRectangle.Height = ReadInt();
                roomRectangles.Add(roomRectangle);
            }
            return roomRectangles;
        }

        private List<RoomObject> ReadRoomObjects(int objectCount)
        {
            var roomObjects = new List<RoomObject>();
            for (var i = 0; i < objectCount; i++)
            {
                var roomObject = new RoomObject();
                roomObject.Id = ReadInt();
                roomObject.X = ReadInt();
                roomObject.Y = ReadInt();
                roomObjects.Add(roomObject);
            }
            return roomObjects;
        }

        private List<Building> ReadBuildings(int buildingCount)
        {
            var buildings = new List<Building>();
            for (var i = 0; i < buildingCount; i++)
            {
                var building = new Building();
                building.RoomCount = ReadInt();
                building.RoomIndices = new List<int>();
                for(var j = 0; j < building.RoomCount; j++)
                {
                    building.RoomIndices.Add(ReadInt());
                }
                buildings.Add(building);
            }
            return buildings;
        }
    }

    class LotHeaderFile
    {
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

    class Room
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int RectangleCount { get; set; }
        public List<RoomRectangle> Rectangles { get; set; }
        public int ObjectCount { get; set; }
        public List<RoomObject> Objects { get; set; }
    }

    class Building
    {
        public int RoomCount { get; set; }

        public List<int> RoomIndices { get; set; }
    }

    class RoomRectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    class RoomObject
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }
}
