using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using DynamicData;
using Metasia.Core.Xml;
using Metasia.Core.Objects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using System.Collections.Generic;

namespace Metasia.Editor.Models;

public class ProjectSaveLoadManager
{
    private const string ProjectJsonEntryName = "project.json";
    private const string TimelinesFolderName = "timelines/";

    public static void Save(MetasiaEditorProject editorProject, string projectFilePath)
    {
        string tempProjectFilePath = projectFilePath + ".tmp";
        string backupProjectFilePath = projectFilePath + ".bak";

        if (File.Exists(tempProjectFilePath))
        {
            File.Delete(tempProjectFilePath);
        }

        try
        {
            // ZIPアーカイブを作成
            using (var archive = ZipFile.Open(tempProjectFilePath, ZipArchiveMode.Create))
            {
                // 1. project.jsonを追加
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true,
                };
                string jsonString = JsonSerializer.Serialize(editorProject.ProjectFile, options);
                var projectEntry = archive.CreateEntry(ProjectJsonEntryName);
                using (var entryStream = projectEntry.Open())
                using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    writer.Write(jsonString);
                }

                // 2. タイムラインをtimelines/以下に保存
                foreach (var timeline in editorProject.Timelines)
                {
                    string timelineFileName = GetTimelineFileName(timeline);
                    string timelineXmlString = MetasiaObjectXmlSerializer.Serialize(timeline);
                    string entryName = TimelinesFolderName + timelineFileName;

                    var timelineEntry = archive.CreateEntry(entryName);
                    using (var entryStream = timelineEntry.Open())
                    using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                    {
                        writer.Write(timelineXmlString);
                    }
                }
            }

            if (File.Exists(projectFilePath))
            {
                File.Replace(tempProjectFilePath, projectFilePath, backupProjectFilePath, true);
                if (File.Exists(backupProjectFilePath))
                {
                    File.Delete(backupProjectFilePath);
                }
            }
            else
            {
                File.Move(tempProjectFilePath, projectFilePath);
            }
        }
        catch
        {
            if (File.Exists(tempProjectFilePath))
            {
                File.Delete(tempProjectFilePath);
            }

            throw;
        }
    }

    public static MetasiaEditorProject Load(string projectFilePath)
    {
        if (!File.Exists(projectFilePath))
        {
            throw new FileNotFoundException($"プロジェクトファイルが見つかりません: {projectFilePath}");
        }

        string? dirName = Path.GetDirectoryName(projectFilePath);
        if (string.IsNullOrEmpty(dirName))
        {
            throw new ArgumentException(
                $"プロジェクトファイルのディレクトリを特定できません: {projectFilePath}",
                nameof(projectFilePath)
            );
        }

        MetasiaProjectFile? projectFile;
        List<TimelineObject> timelines = [];

        using (var archive = ZipFile.OpenRead(projectFilePath))
        {
            // 1. project.jsonを読み込み
            var projectEntry = archive.GetEntry(ProjectJsonEntryName);
            if (projectEntry == null)
            {
                throw new Exception($"{ProjectJsonEntryName}が見つかりません。ファイルが破損している可能性があります。");
            }

            string jsonContent;
            using (var entryStream = projectEntry.Open())
            using (var reader = new StreamReader(entryStream, Encoding.UTF8))
            {
                jsonContent = reader.ReadToEnd();
            }

            projectFile = JsonSerializer.Deserialize<MetasiaProjectFile>(jsonContent);
            if (projectFile is null)
            {
                throw new Exception($"{ProjectJsonEntryName}のフォーマットが不正です。");
            }

            // 2. timelines/以下のXMLファイルを読み込み
            var timelineEntries = archive.Entries
                .Where(e => e.FullName.StartsWith(TimelinesFolderName) &&
                           e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

            foreach (var timelineEntry in timelineEntries)
            {
                try
                {
                    string xmlContent;
                    using (var entryStream = timelineEntry.Open())
                    using (var reader = new StreamReader(entryStream, Encoding.UTF8))
                    {
                        xmlContent = reader.ReadToEnd();
                    }

                    TimelineObject? timelineObject = MetasiaObjectXmlSerializer.Deserialize<TimelineObject>(xmlContent);
                    if (timelineObject == null)
                    {
                        throw new Exception("タイムラインファイルのフォーマットが不正です。");
                    }

                    timelines.Add(timelineObject);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"タイムライン '{timelineEntry.FullName}' の読み込みに失敗: {e.Message}");
                }
            }
        }

        MetasiaEditorProject editorProject = new(
            new DirectoryEntity(dirName),
            projectFile
        );
        foreach (TimelineObject timeline in timelines)
        {
            editorProject.Timelines.Add(timeline);
        }

        return editorProject;
    }

    private static string GetTimelineFileName(TimelineObject timeline)
    {
        // Timeline.Idをベースにファイル名を生成
        if (timeline is null)
        {
            throw new ArgumentNullException(nameof(timeline));
        }

        if (string.IsNullOrWhiteSpace(timeline.Id))
        {
            timeline.Id = Guid.NewGuid().ToString();
        }

        return $"{timeline.Id}.xml";
    }
}
