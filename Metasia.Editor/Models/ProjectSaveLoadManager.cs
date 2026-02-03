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

namespace Metasia.Editor.Models;

public class ProjectSaveLoadManager
{
    private const string ProjectJsonEntryName = "project.json";
    private const string TimelinesFolderName = "timelines/";

    public static void Save(MetasiaEditorProject editorProject, string projectFilePath)
    {
        // 既存のファイルがあれば削除
        if (File.Exists(projectFilePath))
        {
            File.Delete(projectFilePath);
        }

        // ZIPアーカイブを作成
        using (var archive = ZipFile.Open(projectFilePath, ZipArchiveMode.Create))
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
    }

    public static MetasiaEditorProject Load(string projectFilePath)
    {
        if (!File.Exists(projectFilePath))
        {
            throw new FileNotFoundException($"プロジェクトファイルが見つかりません: {projectFilePath}");
        }

        MetasiaEditorProject editorProject = new MetasiaEditorProject(
            new DirectoryEntity(Path.GetDirectoryName(projectFilePath)!),
            null
        );

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

            MetasiaProjectFile? projectFile = JsonSerializer.Deserialize<MetasiaProjectFile>(jsonContent);
            if (projectFile == null)
            {
                throw new Exception($"{ProjectJsonEntryName}のフォーマットが不正です。");
            }
            editorProject.ProjectFile = projectFile;

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

                    editorProject.Timelines.Add(timelineObject);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"タイムライン '{timelineEntry.FullName}' の読み込みに失敗: {e.Message}");
                }
            }
        }

        return editorProject;
    }

    private static string GetTimelineFileName(TimelineObject timeline)
    {
        // Timeline.Idをベースにファイル名を生成
        string timelineId = timeline?.Id ?? Guid.NewGuid().ToString();
        return $"{timelineId}.xml";
    }
}
