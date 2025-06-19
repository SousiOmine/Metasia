using Metasia.Core.Project;
using Metasia.Editor.Models.Projects;
using SkiaSharp;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate;

namespace Metasia.Editor.Models.ProjectGenerate;

public class KariProjectTemplate : IProjectTemplate
{
    public string Name => "Kari Project Template";
    public MetasiaProject Template { get; }

    public KariProjectTemplate()
    {
        Template = new MetasiaProject(new ProjectInfo() { Framerate = 60, Size = new SKSize(3840, 2160) });
        Template.LastFrame = 239;

        // オブジェクトの作成
        kariHelloObject kariHello = new kariHelloObject("karihello")
        { 
            EndFrame = 120,
        };
        kariHello.Rotation.Params.Add(new CoordPoint() { Value = 90, Frame = 120 });

        kariHelloObject kariHello2 = new kariHelloObject("karihello2")
        {
            EndFrame = 10,
        };
        kariHello2.Y.Params[0].Value = 300;
        kariHello2.Rotation.Params[0].Value = 45;
        kariHello2.Alpha.Params[0].Value = 50;
        kariHello2.Scale.Params[0].Value = 50;
        kariHello2.X.Params.Add(new CoordPoint() { Value = 1000, Frame = 10 });

        Text text = new Text("konnichiwa")
        {
            EndFrame = 120,
            TypefaceName = "LINE Seed JP_TTF",
            Contents = "こんにちは Hello",
        };
        text.TextSize.Params[0].Value = 400;

        Text onesec = new Text("sec1")
        {
            EndFrame = 59,
            TypefaceName = "LINE Seed JP_TTF",
            Contents = "1",
        };
        onesec.TextSize.Params[0].Value = 200;
        onesec.X.Params[0].Value = -1800;
        onesec.Y.Params[0].Value = 900;

        Text twosec = new Text("sec2")
        {
            StartFrame = 60,
            EndFrame = 119,
            TypefaceName = "LINE Seed JP_TTF",
            Contents = "2",
        };
        twosec.TextSize.Params[0].Value = 200;
        twosec.X.Params[0].Value = -1800;
        twosec.Y.Params[0].Value = 900;

        Text foursec = new Text("sec4")
        {
            StartFrame = 180,
            EndFrame = 239,
            TypefaceName = "LINE Seed JP_TTF",
            Contents = "4",
        };
        foursec.TextSize.Params[0].Value = 200;
        foursec.X.Params[0].Value = -1800;
        foursec.Y.Params[0].Value = 900;

        // レイヤーの作成
        LayerObject layer1 = new LayerObject("layer1", "Layer 1");
        LayerObject layer2 = new LayerObject("layer2", "Layer 2");
        LayerObject layer3 = new LayerObject("layer3", "Layer 3");
        LayerObject layer4 = new LayerObject("layer4", "Layer 4");
        LayerObject layer5 = new LayerObject("layer5", "Layer 5");

        // セカンドタイムラインの作成
        TimelineObject secondTL = new TimelineObject("SecondTimeline")
        {
            StartFrame = 60,
            EndFrame = 119,
        };
        LayerObject secLayer = new LayerObject("secLayer", "Layer 1");
        secondTL.Layers.Add(secLayer);

        kariHelloObject karisec = new kariHelloObject("karihello3")
        {
            EndFrame = 1200,
        };
        karisec.Scale.Params[0].Value = 300;

        secLayer.Objects.Add(karisec);
        layer5.Objects.Add(secondTL);

        // グループ制御オブジェクトのサンプル作成
        GroupControlObject sampleGroupControl = new GroupControlObject("sampleGroup", "Sample Group")
        {
            StartFrame = 30,   // フレーム30から
            EndFrame = 150,    // フレーム150まで有効
        };
        
        // グループ制御の設定（2段下のレイヤーまで影響）
        sampleGroupControl.TargetLayerDepth = 2;
        sampleGroupControl.Scale.Params[0].Value = 80;  // 80%スケール
        sampleGroupControl.X.Params[0].Value = 100;     // X座標オフセット
        sampleGroupControl.Alpha.Params[0].Value = 20;  // 20%透明度
        sampleGroupControl.Rotation.Params[0].Value = 15; // 15度回転

        // メインタイムラインの作成と設定
        TimelineObject mainTL = new TimelineObject("RootTimeline");

        // レイヤー1にグループ制御オブジェクトを配置（レイヤー2, 3に影響）
        layer1.Objects.Add(sampleGroupControl);
        layer1.Objects.Add(kariHello);
        layer2.Objects.Add(kariHello2);
        layer3.Objects.Add(text);
        layer4.Objects.Add(onesec);
        layer4.Objects.Add(twosec);
        layer4.Objects.Add(foursec);
        mainTL.Layers.Add(layer1);
        mainTL.Layers.Add(layer2);
        mainTL.Layers.Add(layer3);
        mainTL.Layers.Add(layer4);
        mainTL.Layers.Add(layer5);

        // タイムラインをプロジェクトに追加
        Template.Timelines.Add(mainTL);
        Template.Timelines.Add(secondTL);
    }
}
