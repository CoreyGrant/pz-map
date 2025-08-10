using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PzMap
{
    public class DataCache
    {
        private const string SvgPath = @"C:\Dev\logs\svg.svg";
        private const string MetadataPath = @"C:\Dev\logs\metadata.json";
        private const string InfoPath = @"C:\Dev\logs\info.json";

        public bool TryGetCached(out string svg, out string metadata, out string info)
        {
            var svgExists = File.Exists(SvgPath);
            var metadataExists = File.Exists(MetadataPath);
            var infoExists = File.Exists(InfoPath);
            if(!svgExists || !metadataExists || !infoExists) {
                svg = null;
                metadata = null;
                info = null;
                return false; 
            }
            svg = File.ReadAllText(SvgPath);
            metadata = File.ReadAllText(MetadataPath);
            info = File.ReadAllText(InfoPath);
            return true;
        }

        public void Cache(string svg, string metadata, string info)
        {
            File.WriteAllText(SvgPath, svg);
            File.WriteAllText(MetadataPath, metadata);
            File.WriteAllText(InfoPath, info);
        }
    }
}
