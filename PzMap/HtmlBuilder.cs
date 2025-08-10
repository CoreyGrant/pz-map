using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PzMap
{
    public class HtmlBuilder
    {
        private readonly string _assetFolder;
        private readonly string _htmlFilename;
        private readonly IEnumerable<AssetReplacement> _assetReplacements;
        private readonly IEnumerable<JsonReplacement> _jsonReplacements;
        private readonly IEnumerable<TextReplacement> _textReplacements;

        public HtmlBuilder(
            string assetFolder,
            string htmlFilename,
            IEnumerable<AssetReplacement> assetReplacements,
            IEnumerable<JsonReplacement> jsonReplacements,
            IEnumerable<TextReplacement> textReplacements)
        {
            _assetFolder = assetFolder;
            _htmlFilename = htmlFilename;
            _assetReplacements = assetReplacements;
            _jsonReplacements = jsonReplacements;
            _textReplacements = textReplacements;
        }

        public string PerformReplacements()
        {
            var indexHtmlPath = Path.Combine(_assetFolder, _htmlFilename);
            var indexHtml = File.ReadAllText(indexHtmlPath);
            foreach(var replacement in _assetReplacements)
            {
                var assetPath = Path.Combine(_assetFolder, replacement.FileName);
                var assetFile = File.ReadAllText(assetPath);
                indexHtml = indexHtml.Replace(
                    replacement.ReplacementText,
                    $"<{replacement.Tag}>{assetFile}</{replacement.Tag}>");
            }
            foreach(var replacement in _jsonReplacements)
            {
                indexHtml = indexHtml.Replace(
                    replacement.ReplacementText,
                    @$"<script>
window.{replacement.Name} = {replacement.Json}
</script>");
            }
            foreach(var replacement in _textReplacements)
            {
                indexHtml = indexHtml.Replace(
                    replacement.ReplacementText,
                    replacement.Text);
            }
            return indexHtml;
        }

        public void PerformAndSave(string outputPath)
        {
            var htmlOutput = PerformReplacements();
            File.WriteAllText(outputPath, htmlOutput);
        }
    }

    public abstract class Replacement
    {
        public string ReplacementText { get; set; }
    }

    public class AssetReplacement : Replacement
    {
        public string FileName { get; set; }
        public string Tag { get; set; } = "script";
    }

    public class JsonReplacement : Replacement
    {
        public string Name { get; set; }
        public string Json { get; set; }
    }

    public class TextReplacement : Replacement
    {
        public string Text { get; set; }
    }
}
