using AgentFrameworkToolkit.Tools;
using JetBrains.Annotations;

namespace AgentFrameworkToolkit.EasierTools.After;

[PublicAPI]
public class FileSystemToolsAfter
{
    public string RootFolder { get; set; }

    public FileSystemToolsAfter()
    {
        RootFolder = @"C:\ToolCallingExample";
        if (!Directory.Exists(RootFolder))
        {
            Directory.CreateDirectory(RootFolder);
        }
    }

    [AITool("get_root_folder", "Get the root folder to work in")]
    public string GetRootFolder()
    {
        return RootFolder;
    }

    [AITool("create_folder", "Create a new folder")]
    public void CreateFolder(string folderPath)
    {
        Guard(folderPath);
        Directory.CreateDirectory(folderPath);
    }

    [AITool("create_file", "Create a new file")]
    public void CreateFile(string filePath, string content)
    {
        Guard(filePath);
        File.WriteAllText(filePath, content);
    }

    [AITool("get_content_of_file", "Get the content of a file")]
    public string GetContentOfFile(string filePath)
    {
        Guard(filePath);
        return File.ReadAllText(filePath);
    }

    [AITool("move_file", "Move a File")]
    public void MoveFile(string sourceFilePath, string targetFilePath)
    {
        Guard(sourceFilePath);
        Guard(targetFilePath);
        File.Move(sourceFilePath, targetFilePath);
    }

    [AITool("move_folder", "Move a Folder")]
    public void MoveFolder(string sourceFolderPath, string targetFolderPath)
    {
        Guard(sourceFolderPath);
        Guard(targetFolderPath);
        Directory.Move(sourceFolderPath, targetFolderPath);
    }

    [AITool("get_files", "Get Files in a Folder")]
    public string[] GetFiles(string folderPath)
    {
        Guard(folderPath);
        return Directory.GetFiles(folderPath);
    }

    [AITool("get_folders", "Get SubFolders in a Folder")]
    public string[] GetFolders(string folderPath)
    {
        Guard(folderPath);
        return Directory.GetDirectories(folderPath);
    }

    [AITool("delete_folder", "Delete a Folder")]
    public void DeleteFolder(string folderPath)
    {
        if (folderPath == RootFolder)
        {
            throw new Exception("You are not allowed to delete the Root Folder");
        }

        Guard(folderPath);
        Directory.Delete(folderPath);
    }

    [AITool("delete_file", "Delete a File")]
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