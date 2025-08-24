using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using DynamicData;
using Metasia.Core.Xml;
using Metasia.Core.Objects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;

namespace Metasia.Editor.Models;

public class ProjectSaveLoadManager
{
    public static void Save(MetasiaEditorProject editorProject)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
        };
        string jsonString = JsonSerializer.Serialize(editorProject.ProjectFile, options);
        File.WriteAllText(Path.Combine(editorProject.ProjectPath.Path, "metasia.json"), jsonString);

        foreach (var timeline in editorProject.Timelines)
        {
            //string timelineJsonString = JsonSerializer.Serialize(timeline, options);
            string timelineJsonString = TimelineXmlSerializer.SerializeTimeline(timeline.Timeline);
            File.WriteAllText(Path.Combine(editorProject.ProjectPath.Path, timeline.TimelineFilePath.Path), timelineJsonString);
        }
    }

    public static MetasiaEditorProject Load(DirectoryEntity projectPath)
    {
        MetasiaEditorProject editorProject = new MetasiaEditorProject(projectPath, null);

        if (!File.Exists(Path.Combine(projectPath.Path, "metasia.json")))
        {
            throw new FileNotFoundException("metasia.jsonが見つかりません。このフォルダはMetasia Editorプロジェクトではありません。");
        }

        //プロジェクトファイルを読み込む
        MetasiaProjectFile? projectFile = JsonSerializer.Deserialize<MetasiaProjectFile>(File.ReadAllText(Path.Combine(projectPath.Path, "metasia.json")));
        if (projectFile == null)
        {
            throw new Exception("metasia.jsonのフォーマットが不正です。");
        }
        editorProject.ProjectFile = projectFile;

        //タイムラインを取り込む対象のフォルダをプロジェクトファイルから取得
        DirectoryEntity[] scanFolders = projectFile.TimelineFolders.Select(folder => new DirectoryEntity(Path.Combine(projectPath.Path, folder))).ToArray();
        scanFolders = scanFolders.Concat(new[] { projectPath }).ToArray();

        //タイムラインを取り込む対象のフォルダを探索
        foreach (var folder in scanFolders)
        {
            if (Directory.Exists(folder.Path))
            {
                //拡張子がmttlのファイルを取得
                var files = Directory.GetFiles(folder.Path, "*.mttl");
                foreach (var file in files)
                {
                    try
                    {
                        TimelineObject? timelineObject = TimelineXmlSerializer.DeserializeTimeline(File.ReadAllText(file));
                        if (timelineObject == null)
                        {
                            throw new Exception("タイムラインファイルのフォーマットが不正です。");
                        }
                        FileEntity timelineFilePath = new FileEntity(file);

                        TimelineFile timelineFile = new TimelineFile(timelineFilePath, timelineObject);
                        editorProject.Timelines.Add(timelineFile);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

            }
        }



        return editorProject;
    }
}

