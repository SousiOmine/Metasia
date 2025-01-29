using System;
using System.IO;
using Metasia.Core.Json;
using Metasia.Core.Project;

namespace Metasia.Editor.Models;

public class ProjectLoader
{
    public static MetasiaProject LoadProjectFromMTPJ(string filePath)
    {
        string json = File.ReadAllText(filePath);
        try
        {
            return ProjectSerializer.DeserializeFromMTPJ(json);
        }
        catch (Exception ex)
        {
            throw new Exception(".mtpjファイルからプロジェクトを読み込めませんでした。", ex);
        }
    }
}