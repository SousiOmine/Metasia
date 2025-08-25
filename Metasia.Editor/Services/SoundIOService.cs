using System;
using System.Collections.Concurrent;
using Metasia.Core.Sounds;
using SoundIOSharp;

namespace Metasia.Editor.Services;

public class SoundIOService : IAudioService
{
    private readonly SoundIO soundIo;
    private readonly SoundIODevice device;
    private readonly SoundIOOutStream outStream;
    private Action<IntPtr, double>? write_sample;
    
    private readonly ConcurrentQueue<double> soundQueue = new();

    public SoundIOService()
    {
        soundIo = new SoundIO();
        soundIo.Connect();
        soundIo.FlushEvents();

        device = soundIo.GetOutputDevice(soundIo.DefaultOutputDeviceIndex);
        if (device is null) throw new InvalidOperationException("出力オーディオデバイスが見つかりませんでした");
        
        outStream = device.CreateOutStream();
        outStream.WriteCallback = (min, max) => write_callback(outStream, min, max);
        outStream.SampleRate = 44100;
        
        if (device.SupportsFormat(SoundIODevice.Float32FE))
        {
            outStream.Format = SoundIODevice.Float32FE;
            write_sample = write_sample_float32ne;
        }
        else if (device.SupportsFormat(SoundIODevice.Float64NE))
        {
            outStream.Format = SoundIODevice.Float64NE;
            write_sample = write_sample_float64ne;
        }
        else if (device.SupportsFormat(SoundIODevice.S32NE))
        {
            outStream.Format = SoundIODevice.S32NE;
            write_sample = write_sample_s32ne;
        }
        else if (device.SupportsFormat(SoundIODevice.S16NE))
        {
            outStream.Format = SoundIODevice.S16NE;
            write_sample = write_sample_s16ne;
        }
        else
        {
            throw new InvalidOperationException("出力可能なフォーマットが見つかりませんでした");
        }
        
        outStream.Open();
        outStream.Start();
    }

    public void InsertQueue(IAudioChunk chunk)
    {
        if (chunk.Format.SampleRate != outStream.SampleRate || chunk.Format.ChannelCount != outStream.Layout.ChannelCount)
        {
            throw new InvalidOperationException("チャンネル数またはサンプルレートが一致していません");
        }
        foreach (var sample in chunk.Samples)
        {
            soundQueue.Enqueue(sample);
        }
    }

    public void ClearQueue()
	{
		soundQueue.Clear();
	}

    public long GetQueuedSamplesCount()
    {
        return soundQueue.Count / outStream.Layout.ChannelCount;
    }

    public void Dispose()
    {
        outStream.Dispose();
        device.RemoveReference();
        soundIo.Dispose();
    }
    
    //チャンネル数が違う音声を変換する
    private double[] ConvertChannel(double[] pulse, byte before_channel, byte after_channel)
    {
        if(before_channel == after_channel)
            return pulse;
        throw new InvalidOperationException("チャンネル数の変換は実装されていません");
    }
    
    private void write_callback(SoundIOOutStream outStream, int min, int max)
    {
        if (write_sample is null) throw new InvalidOperationException("write_sample is null");

        double float_sample_rate = outStream.SampleRate;
        double seconds_per_frame = 1.0 / float_sample_rate;

        int frames_left = max;
        if (max > 1470)
        {
            frames_left = 1470;
        }
			
        int frame_count = 0;

        for (;;)
        {
            frame_count = frames_left;
            var results = outStream.BeginWrite(ref frame_count);

            if (frame_count == 0)
                break;

            SoundIOChannelLayout layout = outStream.Layout;
            
            if (soundQueue.Count > 0)
            {
                int count = soundQueue.Count;
                for (int frame = 0; frame < frame_count && frame < count; frame += 1)
                {
                    double sample = 0;
						
                    for (int channel = 0; channel < layout.ChannelCount; channel += 1)
                    {
                        soundQueue.TryDequeue(out sample);
                        var area = results.GetArea(channel);
                        write_sample(area.Pointer, sample);
                        area.Pointer += area.Step;
                    }
                }
            }
            else
            {
                for (int frame = 0; frame < frame_count; frame += 1)
                {
						
                    for (int channel = 0; channel < layout.ChannelCount; channel += 1)
                    {

                        var area = results.GetArea(channel);
                        write_sample(area.Pointer, 0);
                        area.Pointer += area.Step;
                    }
                }
            }
            
            outStream.EndWrite();

            frames_left -= frame_count;
            if (frames_left <= 0)
                break;
        }
    }
    
    static unsafe void write_sample_s16ne(IntPtr ptr, double sample)
    {
        short* buf = (short*)ptr;
        double range = (double)short.MaxValue - (double)short.MinValue;
        double val = sample * range / 2.0;
        *buf = (short)val;
    }

    static unsafe void write_sample_s32ne(IntPtr ptr, double sample)
    {
        int* buf = (int*)ptr;
        double range = (double)int.MaxValue - (double)int.MinValue;
        double val = sample * range / 2.0;
        *buf = (int)val;
    }

    static unsafe void write_sample_float32ne(IntPtr ptr, double sample)
    {
        float* buf = (float*)ptr;
        *buf = (float)sample;
    }

    static unsafe void write_sample_float64ne(IntPtr ptr, double sample)
    {
        double* buf = (double*)ptr;
        *buf = sample;
    }


}