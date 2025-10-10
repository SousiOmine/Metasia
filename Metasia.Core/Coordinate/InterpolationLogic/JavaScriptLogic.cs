using Jint;

namespace Metasia.Core.Coordinate.InterpolationLogic;

public class JavaScriptLogic : InterpolationLogicBase
{
    public override string Identify { get; } = "JavaScriptLogic";

    public string JSLogic = """
if(StartValue == EndValue) return StartValue;
StartValue + (EndValue - StartValue) * (NowFrame - StartFrame) / (EndFrame - StartFrame)
""";

    private Engine jsEngine = new Engine(opts => opts
        .MaxStatements(10000)
        .LimitRecursion(10000)
        .TimeoutInterval(TimeSpan.FromMilliseconds(100))
    );

    public override double Calculate(double startValue, double endValue, int nowFrame, int startFrame, int endFrame)
    {
        jsEngine.SetValue("StartValue", startValue)
            .SetValue("EndValue", endValue)
            .SetValue("NowFrame", nowFrame)
            .SetValue("StartFrame", startFrame)
            .SetValue("EndFrame", endFrame);
        try
        {
            double midValue = jsEngine.Evaluate(JSLogic).AsNumber();
            return midValue;
        }
        catch (Exception e)
        {
            throw new Exception("JavaScriptLogic Calculate Error", e);
        }
    }

    /// <summary>
    /// 自身をハードコピーします。
    /// </summary>
    /// <returns>現在のインスタンスのハードコピー</returns>
    public override InterpolationLogicBase HardCopy()
    {
        JavaScriptLogic copy = new JavaScriptLogic();
        copy.JSLogic = this.JSLogic;
        return copy;
    }
}