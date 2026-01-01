using Metasia.Core.Project;
using Metasia.Editor.Models.Projects;
using SkiaSharp;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Coordinate.InterpolationLogic;
using Metasia.Core.Media;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.Models.ProjectGenerate;

public class KariProjectTemplate : IProjectTemplate
{
    public string Name => "Kari Project Template";
    public MetasiaProject Template { get; }

    public KariProjectTemplate()
    {
        Template = new MetasiaProject(new ProjectInfo(60, new SKSize(3840, 2160), 44100, 2));
        Template.LastFrame = 239;

        // オブジェクトの作成
        kariHelloObject kariHello = new kariHelloObject("karihello")
        {
            EndFrame = 120,
        };
        // kariHello.Rotation.AddPoint(new CoordPoint() { Value = 90, Frame = 120 });
        kariHello.Rotation.IsMovable = true;
        kariHello.Rotation.EndPoint.Value = 90;

        kariHelloObject kariHello2 = new kariHelloObject("karihello2")
        {
            EndFrame = 10,
        };
        kariHello2.Y.SetSinglePoint(300);
        kariHello2.Rotation.SetSinglePoint(45);
        kariHello2.Alpha.SetSinglePoint(50);
        kariHello2.Scale.SetSinglePoint(50);
        
        kariHello2.X.IsMovable = true;
        kariHello2.X.EndPoint.Value = 1000;

        kariHello2.AudioEffects.Add(new VolumeFadeEffect() { In = 1, Out = 1 });

        Text text = new Text("konnichiwa")
        {
            EndFrame = 120,
            Font = MetaFontParam.Default,
            Contents = "こんにちは Hello",
        };
        text.TextSize.SetSinglePoint(400);

        Text onesec = new Text("sec1")
        {
            EndFrame = 59,
            Font = MetaFontParam.Default,
            Contents = "1",
        };
        onesec.TextSize.SetSinglePoint(200);
        onesec.X.SetSinglePoint(-1800);
        onesec.Y.SetSinglePoint(900);

        Text twosec = new Text("sec2")
        {
            StartFrame = 60,
            EndFrame = 119,
            Font = MetaFontParam.Default,
            Contents = "2",
        };
        twosec.TextSize.SetSinglePoint(200);
        twosec.X.SetSinglePoint(-1800);
        twosec.Y.SetSinglePoint(900);

        Text foursec = new Text("sec4")
        {
            StartFrame = 180,
            EndFrame = 239,
            Font = MetaFontParam.Default,
            Contents = "4",
        };
        foursec.TextSize.SetSinglePoint(200);
        foursec.X.SetSinglePoint(-1800);
        foursec.Y.SetSinglePoint(900);

        ImageObject image = new ImageObject("image")
        {
            StartFrame = 120,
            EndFrame = 239
        };


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
        karisec.Scale.SetSinglePoint(300);

        secLayer.Objects.Add(karisec);
        layer5.Objects.Add(secondTL);

        // JavaScriptロジックを持つX座標パラメータ
        Text jsClip = new Text("jsClip")
        {
            StartFrame = 240,
            EndFrame = 500,
            Font = MetaFontParam.Default,
            Contents = "JS",
        };
        // X パラメータに5つの中間点を設定し、1つは JavaScriptLogic を使用
        
        jsClip.X.SetSinglePoint(-1300);
        jsClip.X.IsMovable = true;
        jsClip.X.AddPoint(new CoordPoint() { Value = -400, Frame = 60 });
        jsClip.X.AddPoint(new CoordPoint()
        {
            Value = 500,
            Frame = 120,
            InterpolationLogic = new JavaScriptLogic()
            {
                JSLogic = "StartValue + (EndValue - StartValue) * Math.pow((NowFrame - StartFrame) / (EndFrame - StartFrame), 2)"
            }
        });
        jsClip.X.AddPoint(new CoordPoint() { Value = 1400, Frame = 180 });
        jsClip.X.EndPoint.Value = 2300;
        // クリップをレイヤー5に追加
        layer5.Objects.Add(jsClip);

        // メインタイムラインの作成と設定
        TimelineObject mainTL = new TimelineObject("RootTimeline");

        layer1.Objects.Add(kariHello);
        layer1.Objects.Add(image);
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

