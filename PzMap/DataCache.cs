using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PzMap
{
    public class DataCache
    {
        private readonly string _svgPath = @"svg.svg";
        private readonly string _metadataPath = @"metadata.json";
        private readonly string _infoPath = @"info.json";
        private readonly string _baseFolder;
        public DataCache(string baseFolder)
        {
            _baseFolder = baseFolder;
            _svgPath = Path.Combine(_baseFolder, "svg.svg");
            _metadataPath = Path.Combine(_baseFolder, "metadata.json");
            _infoPath = Path.Combine(_baseFolder, "info.json");
        }

        public bool TryGetCached(out string svg, out string metadata, out string info)
        {
            var svgExists = File.Exists(_svgPath);
            var metadataExists = File.Exists(_metadataPath);
            var infoExists = File.Exists(_infoPath);
            if(!svgExists || !metadataExists || !infoExists) {
                svg = null;
                metadata = null;
                info = null;
                return false; 
            }
            svg = File.ReadAllText(_svgPath);
            metadata = File.ReadAllText(_metadataPath);
            info = File.ReadAllText(_infoPath);
            return true;
        }

        public void Cache(string svg, string metadata, string info)
        {
            File.WriteAllText(_svgPath, svg);
            File.WriteAllText(_metadataPath, metadata);
            File.WriteAllText(_infoPath, info);
        }
    }
}
