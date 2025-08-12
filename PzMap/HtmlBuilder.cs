using NUglify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PzMap
{
    public class HtmlBuilder
    {
        private readonly string _assetFolder;
        private readonly string _htmlFilename;

        public HtmlBuilder(
            string assetFolder,
            string htmlFilename)
        {
            _assetFolder = assetFolder;
            _htmlFilename = htmlFilename;
        }

        private Regex FileRegex = new Regex("%([a-zA-Z-]+)\\.([a-z]+)%");

        public string PerformReplacements()
        {
            var indexHtmlPath = Path.Combine(_assetFolder, _htmlFilename);
            var indexHtml = File.ReadAllText(indexHtmlPath);
            return Uglify.Html(FileRegex.Replace(indexHtml, (match) =>
            {
                var name = match.Groups[1].Value;
                var ext = match.Groups[2].Value;
                var itemPath = Path.Combine(_assetFolder, name + "." + ext);
                var fileContents = File.ReadAllText(itemPath);
                if(ext == "js")
                {
                    return $"<script>{Uglify.Js(fileContents).Code}</script>";
                } else if(ext == "css")
                {
                    return $"<style>{Uglify.Css(fileContents).Code}</style>";
                } else if(ext == "json")
                {
                    return $"<script>window.{name}={fileContents}</script>";
                }
                else
                {
                    return fileContents;
                }
            })).Code;
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
