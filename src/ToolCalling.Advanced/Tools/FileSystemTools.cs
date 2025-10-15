using JetBrains.Annotations;

namespace ToolCalling.Advanced.Tools;

[PublicAPI]
public class FileSystemTools
{
    public string RootFolder { get; set; }

    public FileSystemTools()
    {
        RootFolder = @"C:\FunctionCallingExample";
        if (!Directory.Exists(RootFolder))
        {
            Directory.CreateDirectory(RootFolder);
        }
    }

    public string GetRootFolder()
    {
        return RootFolder;
    }

    public void CreateFolder(string folderPath)
    {
        Guard(folderPath);
        Directory.CreateDirectory(folderPath);
    }

    public void CreateFile(string filePath, string content)
    {
        Guard(filePath);
        File.WriteAllText(filePath, content);
    }

    public string GetContentOfFile(string filePath)
    {
        Guard(filePath);
        return File.ReadAllText(filePath);
    }

    public void MoveFile(string sourceFilePath, string targetFilePath)
    {
        Guard(sourceFilePath);
        Guard(targetFilePath);
        File.Move(sourceFilePath, targetFilePath);
    }

    public void MoveFolder(string sourceFolderPath, string targetFolderPath)
    {
        Guard(sourceFolderPath);
        Guard(targetFolderPath);
        Directory.Move(sourceFolderPath, targetFolderPath);
    }

    public string[] GetFiles(string folderPath)
    {
        Guard(folderPath);
        return Directory.GetFiles(folderPath);
    }

    public string[] GetFolders(string folderPath)
    {
        Guard(folderPath);
        return Directory.GetDirectories(folderPath);
    }

    public void DeleteFolder(string folderPath)
    {
        if (folderPath == RootFolder)
        {
            throw new Exception("You are not allowed to delete the Root Folder");
        }

        Guard(folderPath);
        Directory.Delete(folderPath);
    }

    public void DeleteFile(string filePath)
    {
        Guard(filePath);
        File.Delete(filePath);
    }

    private void Guard(string folderPath)
    {
        if (!folderPath.StartsWith(RootFolder))
        {
            throw new Exception("No you don't!");
        }
    }
}