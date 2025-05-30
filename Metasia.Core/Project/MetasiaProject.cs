﻿using Metasia.Core.Objects;

namespace Metasia.Core.Project
{
    /// <summary>
    /// Metasiaのプロジェクトに含まれるタイムラインオブジェクトを格納するクラス
    /// </summary>
    public class MetasiaProject
    {
        public ProjectInfo Info { get; set; }
        public string RootTimelineId { get; set; } = "RootTimeline";
        public int LastFrame { get; set; } = 100;
        public List<TimelineObject> Timelines { get; set; } = new();
        
        public MetasiaProject(ProjectInfo info)
        {
            Info = info;
        }
        
        // デシリアライズ用のデフォルトコンストラクタ
        public MetasiaProject()
        {
            Info = new ProjectInfo();
        }
    }
}
