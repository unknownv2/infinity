using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using NoDev.Common.IO;

namespace NoDev.Common.Storage
{
    public class Storage
    {
        private long _tempCounter;
        private readonly string _rootPath;

        public string GetFilePath(string fileName)
        {
            var filePath = Path.Combine(_rootPath, fileName);

            var fileDir = filePath.Substring(0, filePath.LastIndexOf('\\'));

            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            return filePath;
        }

        private string GetTempFilePath(string extension = "")
        {
            var ct = Interlocked.Increment(ref _tempCounter);

            var path = GetFilePath("Temp\\" + ct.ToString(CultureInfo.InvariantCulture) + extension);

            if (Directory.Exists(path))
                Directory.Delete(path);

            return path;
        }

        public string WriteTempFile(string extension, byte[] data)
        {
            var filePath = GetTempFilePath(extension);

            File.WriteAllBytes(filePath, data);

            return filePath;
        }

        public bool ContainsFile(string fileName)
        {
            return File.Exists(GetFilePath(fileName));
        }

        public EndianIO OpenRead(string fileName)
        {
            return new EndianIO(File.OpenRead(GetFilePath(fileName)), EndianType.Little);
        }

        public EndianIO OpenWrite(string fileName)
        {
            return new EndianIO(File.OpenWrite(GetFilePath(fileName)), EndianType.Little);
        }

        public string ReadString(string fileName)
        {
            var filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
                return null;

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        public void WriteString(string fileName, string text)
        {
            File.WriteAllText(GetFilePath(fileName), text, Encoding.UTF8);
        }

        private Storage(Environment.SpecialFolder folderType, string companyName, string productName)
        {
            var dir = Environment.GetFolderPath(folderType);

            if (companyName == null)
                _rootPath = productName == null ? dir : Path.Combine(dir, productName);
            else if (productName == null)
                _rootPath = Path.Combine(dir, companyName);
            else
                _rootPath = Path.Combine(dir, companyName, productName);

            var dirInfo = new DirectoryInfo(_rootPath + "\\Temp");

            if (!dirInfo.Exists)
                dirInfo.Create();
            else
            {
                foreach (var file in dirInfo.GetFiles())
                    file.Delete();
            }
        }

        private static readonly IDictionary<Environment.SpecialFolder, Storage> Instances;

        static Storage()
        {
            Instances = new ConcurrentDictionary<Environment.SpecialFolder, Storage>();
        }

        private static string _companyName;
        public static void SetCompanyName(string companyName)
        {
            if (_companyName != null)
                throw new Exception("Company name already set.");

            _companyName = companyName;
        }

        private static string _productName;
        public static void SetProductName(string productName)
        {
            if (_productName != null)
                throw new Exception("Product name already set.");

            _productName = productName;
        }

        public static Storage GetInstance(Environment.SpecialFolder folderType)
        {
            if (Instances.ContainsKey(folderType))
                return Instances[folderType];

            var storage = new Storage(folderType, _companyName, _productName);

            Instances[folderType] = storage;

            return storage;
        }
    }
}
