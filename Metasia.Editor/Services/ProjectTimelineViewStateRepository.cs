using Metasia.Editor.Models;
using Metasia.Editor.Models.States;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Metasia.Editor.Services;

/// <summary>
/// プロジェクト別のタイムライン表示状態を JSON ファイルとして保存・読込する実装です。
/// </summary>
public sealed class ProjectTimelineViewStateRepository : IProjectTimelineViewStateRepository
{
    private const string ProjectViewStatesDirectoryName = "ProjectViewStates";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _projectViewStatesDirectory;

    public ProjectTimelineViewStateRepository()
        : this(MetasiaPaths.AppDataDirectory)
    {
    }

    public ProjectTimelineViewStateRepository(string baseDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        _projectViewStatesDirectory = Path.Combine(baseDirectory, ProjectViewStatesDirectoryName);
    }

    public ProjectTimelineViewStateSnapshot? Load(string projectFilePath)
    {
        try
        {
            var stateFilePath = GetStateFilePath(projectFilePath);
            if (!File.Exists(stateFilePath))
            {
                return null;
            }

            var json = File.ReadAllText(stateFilePath);
            return JsonSerializer.Deserialize<ProjectTimelineViewStateSnapshot>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"プロジェクト別タイムライン状態の読み込みエラー: {ex.Message}");
            return null;
        }
    }

    public void Save(ProjectTimelineViewStateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.ProjectFilePath);

        try
        {
            Directory.CreateDirectory(_projectViewStatesDirectory);

            var stateFilePath = GetStateFilePath(snapshot.ProjectFilePath);
            var tempFilePath = stateFilePath + ".tmp";
            var json = JsonSerializer.Serialize(snapshot, JsonOptions);

            File.WriteAllText(tempFilePath, json, Encoding.UTF8);

            if (File.Exists(stateFilePath))
            {
                File.Replace(tempFilePath, stateFilePath, null);
            }
            else
            {
                File.Move(tempFilePath, stateFilePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"プロジェクト別タイムライン状態の保存エラー: {ex.Message}");
        }
    }

    public string GetStateFilePath(string projectFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFilePath);

        var normalizedPath = NormalizeProjectFilePath(projectFilePath);
        var fileName = ComputeHash(normalizedPath) + ".json";
        return Path.Combine(_projectViewStatesDirectory, fileName);
    }

    private static string NormalizeProjectFilePath(string projectFilePath)
    {
        var normalized = Path.GetFullPath(projectFilePath);
        return OperatingSystem.IsWindows()
            ? normalized.ToUpperInvariant()
            : normalized;
    }

    private static string ComputeHash(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }
}
