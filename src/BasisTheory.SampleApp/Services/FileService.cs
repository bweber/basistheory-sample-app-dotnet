using System;
using System.IO;
using System.Threading.Tasks;
using BasisTheory.SampleApp.Entities;

namespace BasisTheory.SampleApp.Services
{
    public interface IFileService
    {
        Task<FileData> GetFileData(string filePath);
        string SaveFileData(FileData fileData);
    }

    public class FileService : IFileService
    {
        public async Task<FileData> GetFileData(string filePath)
        {
            var filePathParsed = ParseFilePath(filePath);

            if (!File.Exists(filePathParsed))
                throw new FileNotFoundException("Invalid file path provided", filePath);

            var file = await File.ReadAllBytesAsync(filePathParsed);
            return new FileData(Path.GetFileName(filePathParsed), file);
        }

        public string SaveFileData(FileData fileData)
        {
            var (fileName, contents) = fileData;

            var filePath = Path.GetFullPath(ParseFilePath(fileName));
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            File.WriteAllBytes(filePath, contents);

            return filePath;
        }

        private static string ParseFilePath(string filePath)
        {
            if (!filePath.StartsWith("~"))
                return filePath;

            string homePath = Environment.OSVersion.Platform is PlatformID.Unix or PlatformID.MacOSX
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            return filePath.Replace("~", homePath);
        }
    }
}
