using System;

namespace Metasia.Editor.Services;

public interface IAudioService : IDisposable
{
    public void InsertQueue(double[] pulse, byte channel);
}