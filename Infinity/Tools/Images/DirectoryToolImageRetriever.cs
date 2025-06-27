using System.IO;

namespace NoDev.Infinity.Tools
{
    internal class DirectoryToolImageRetriever : IToolImageRetriever
    {
        internal string ImageDirectory { get; private set; }

        private readonly string _searchPattern;

        internal DirectoryToolImageRetriever(string imageDirectory, string searchPattern = "{0}.dll")
        {
            ImageDirectory = imageDirectory;
            _searchPattern = searchPattern;
        }

        private string GetImagePath(string toolId)
        {
            return ImageDirectory + @"\" + string.Format(_searchPattern, toolId);
        }

        public bool HasToolImage(string toolId)
        {
            return File.Exists(GetImagePath(toolId));
        }

        public string RetrieveToolImageLocation(string toolId)
        {
            var imagePath = GetImagePath(toolId);

            return File.Exists(imagePath) ? Path.GetFullPath(imagePath) : null;
        }
    }
}
