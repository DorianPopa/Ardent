using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;

public class LocalProjectManager : MonoBehaviour
{
    private string _basePath;

    void Awake()
    {
        _basePath = Application.persistentDataPath;
    }


    public async Task<string> WriteNewFile(Stream projectStream, string projectId, string projectHash)
    {
        string projectDirectoryPath = Path.Combine(_basePath, projectId);
        string tempArchiveDirectory = Path.Combine(_basePath, "temp");
        string tempArchivePath = Path.Combine(tempArchiveDirectory, projectId);
        string hashFilePath = Path.Combine(projectDirectoryPath, $"{projectId}.hash");

        try
        {
            if (!Directory.Exists(tempArchiveDirectory))
                Directory.CreateDirectory(tempArchiveDirectory);

            // Delete existing files for this project
            if(Directory.Exists(projectDirectoryPath))
                DeleteProjectFiles(projectId);

            // Create a directory for this project
            Directory.CreateDirectory(projectDirectoryPath);
            print($"Created directory for project with id {projectId}");
                
            using (Stream stream = new FileStream(tempArchivePath, FileMode.CreateNew))
            {
                // Write the archive into the filesystem
                await projectStream.CopyToAsync(stream);
            }

            // Extract the archive files into the project directory
            ZipFile.ExtractToDirectory(tempArchivePath, projectDirectoryPath);

            // Remove the temp archive
            File.Delete(tempArchivePath);

            // Write the current hash op the project to a file
            File.WriteAllText(hashFilePath, projectHash);

            string[] projectObjFiles = Directory.GetFiles(projectDirectoryPath, "*.obj");
            if (projectObjFiles.Length > 0)
                return projectObjFiles[0];

            throw new Exception("Could not find an obj file");
        }
        catch(Exception e)
        {
            print(e.Message);
            throw e;
        }
    }

    public bool ProjectExistsLocally(string projectId)
    {
        string projectDirectoryPath = Path.Combine(_basePath, projectId);
        if (Directory.Exists(projectDirectoryPath))
            if(Directory.GetFiles(projectDirectoryPath, "*.obj").Length > 0)
                return true;
        return false;
    }

    public bool HashMatchesWithLocalFile(string projectId, string hash)
    {
        string projectDirectoryPath = Path.Combine(_basePath, projectId);
        string hashFilePath = Path.Combine(projectDirectoryPath, $"{projectId}.hash");
        try
        {
            string localFileHash = File.ReadAllText(hashFilePath);
            if (localFileHash == hash)
                return true;
            return false;
        }
        catch(Exception e)
        {
            print(e.Message);
            return false;
        }
    }

    public string GetPathToObjProject(string projectId)
    {
        string projectDirectoryPath = Path.Combine(_basePath, projectId);

        string[] projectObjFiles = Directory.GetFiles(projectDirectoryPath, "*.obj");
        if (projectObjFiles.Length > 0)
            return projectObjFiles[0];

        return null;
    }

    public void DeleteProjectFiles(string projectId)
    {
        string projectDirectoryPath = Path.Combine(_basePath, projectId);

        Directory.Delete(projectDirectoryPath, true);
    }
}
