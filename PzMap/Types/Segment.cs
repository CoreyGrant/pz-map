using PzMap.Types.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PzMap.Types
{
    public class Segment
    {
        public Vector2[] Points { get; set; }
        public string Type { get; set; }
        public string Key { get; set; }
        public string Id { get; set; }
    }
}
