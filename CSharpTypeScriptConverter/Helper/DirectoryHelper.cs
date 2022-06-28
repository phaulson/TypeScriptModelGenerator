using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpTypeScriptConverter.Data;

namespace CSharpTypeScriptConverter.Helper;

internal static class DirectoryHelper
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

    public static string GetFileExtension(string filePath)
    {
        return filePath.Substring(filePath.LastIndexOf(".", StringComparison.Ordinal) + 1);
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

    public static void WriteToFile(string path, IEnumerable<TypeScriptFile> tsFiles)
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
}