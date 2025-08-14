using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PzMap
{
    public class DataCache
    {
        private readonly string _svgFilename = @"svg.svg";
        private readonly string _metadataFilename = @"metadata.json";
        private readonly string _infoFilename = @"info.json";
        private readonly string _baseFolder;
        public DataCache(string baseFolder)
        {
            _baseFolder = baseFolder;
        }

        public bool TryGetCached(int version, out string svg, out string metadata, out string info)
        {
            var svgPath = Path.Combine(_baseFolder, $"b{version}-{_svgFilename}");
            var metadataPath = Path.Combine(_baseFolder, $"b{version}-{_metadataFilename}");
            var infoPath = Path.Combine(_baseFolder, $"b{version}-{_infoFilename}");
            var svgExists = File.Exists(
                svgPath);
            var metadataExists = File.Exists(metadataPath);
            var infoExists = File.Exists(infoPath);
            if(!svgExists || !metadataExists || !infoExists) {
                svg = null;
                metadata = null;
                info = null;
                return false; 
            }
            svg = File.ReadAllText(svgPath);
            metadata = File.ReadAllText(metadataPath);
            info = File.ReadAllText(infoPath);
            return true;
        }
    }
}
