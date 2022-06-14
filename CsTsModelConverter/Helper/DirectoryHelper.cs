﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsTsSModelConverter.Data;

namespace CsTsSModelConverter.Helper
{
    public static class DirectoryHelper
    {
        public static string GetMiddlePath(string filePath, string srcPath)
        {
            var newPart = filePath.Substring(srcPath.Length, filePath.Length - srcPath.Length);
            return newPart.Substring(0,
                newPart.LastIndexOf("\\", StringComparison.Ordinal) != -1
                    ? newPart.LastIndexOf("\\", StringComparison.Ordinal)
                    : 0);
        }

        public static string GetFileName(string filePath)
        {
            var startingIndex = filePath.LastIndexOf("\\", StringComparison.Ordinal) != -1 ? 
                filePath.LastIndexOf("\\", StringComparison.Ordinal) : 0;
            return filePath.Substring(startingIndex, filePath.LastIndexOf(".", StringComparison.Ordinal) - startingIndex)
                .Replace("\\", "");
        }

        public static string CalculateRelativePath(string srcPath, string destPath)
        {
            var path1 = srcPath.Split(new[] {"\\"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var path2 = destPath.Split(new[] {"\\"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            var removedPath = "";
            var newPath = "";
            while(path1.Any() && path2.Any()) 
            {
                if (path1[0] != path2[0]) {
                    newPath += "../";
                    removedPath += $"{path2[0]}/";
                }

                path1.RemoveAt(0);
                path2.RemoveAt(0);
            }
            newPath += string.Join("", path1.Select(_ => "../"));
            if (!newPath.StartsWith(".")) newPath = $"./{newPath}";

            var complete = newPath + removedPath + string.Join("/", path2);
            if (complete.EndsWith("/")) complete = complete.Remove(complete.Length - 1);

            return complete;
        }

        public static void WriteToFile(string path, IEnumerable<TypescriptFile> tsFiles)
        {
            Parallel.ForEach(tsFiles, tsFile =>
            {
                var dirPath = $"{path}{tsFile.RelativePath}";
                if (!dirPath.EndsWith("\\")) dirPath = $"{dirPath}\\";

                if (!string.IsNullOrWhiteSpace(dirPath))
                    Directory.CreateDirectory(dirPath);

                File.WriteAllText(tsFile.FullPath, tsFile.Code);
            });
        }

        public static void Cleanup(string parentDirectory, string sourcePath, List<string> filesToIgnore)
        {
            Parallel.ForEach(Directory.GetDirectories(parentDirectory), directory =>
            {
                Cleanup(directory, sourcePath, filesToIgnore);
                Parallel.ForEach(Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => !filesToIgnore.Contains(f)), File.Delete);
                
                if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory, false);
            });
            
            Parallel.ForEach(Directory.EnumerateFiles(parentDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => !filesToIgnore.Contains(f)), File.Delete);
        }
    }
}