using System.Xml.Serialization;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Sounds;

namespace Metasia.Core.Tests.Objects.AudioEffects;

public class TestSetSampleEffect : AudioEffectBase
{
    public double Value { get; set; }

    [XmlIgnore]
    public int ApplyCallCount { get; private set; }

    public override IAudioChunk Apply(IAudioChunk input, AudioEffectContext context)
    {
        ApplyCallCount++;
        var output = new AudioChunk(input.Format, input.Length);
        for (long i = 0; i < output.Samples.Length; i++)
        {
            output.Samples[i] = Value;
        }

        return output;
    }
}

public class TestAddSampleEffect : AudioEffectBase
{
    public double Value { get; set; }

    [XmlIgnore]
    public int ApplyCallCount { get; private set; }

    public override IAudioChunk Apply(IAudioChunk input, AudioEffectContext context)
    {
        ApplyCallCount++;
        var output = new AudioChunk(input.Format, input.Length);
        for (long i = 0; i < output.Samples.Length; i++)
        {
            output.Samples[i] = input.Samples[i] + Value;
        }

        return output;
    }
}
