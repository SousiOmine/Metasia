using Metasia.Core.Sounds;

namespace Metasia.Core.Objects.AudioEffects
{
	public class VolumeFadeEffect : AudioEffectBase
	{
        /// <summary>
        /// フェードイン時の時間
        /// </summary>
        public float In { get; set; } = 0.5f;
        
        /// <summary>
        /// フェードアウト時の時間
        /// </summary>
        public float Out { get; set; } = 0.5f;
		public override AudioChunk Apply(AudioChunk input, AudioEffectContext context)
		{
			// 入力チェック
			if (input == null || input.Samples == null || input.Samples.Length == 0)
			{
				return input;
			}

			// フェード時間が0の場合は元の音声を返す
			if (In <= 0 && Out <= 0)
			{
				return input;
			}

			// サンプルレートとチャネル数を取得
			int sampleRate = context.Format.SampleRate;
			int channelCount = context.Format.ChannelCount;
			
			// オブジェクト全体の長さをサンプル数に変換
			long totalObjectSamples = (long)(context.ObjectDurationInSeconds * sampleRate);
			
			// 現在のチャンクの長さ（サンプル数）
			long chunkLength = input.Length;

			// フェードイン・フェードアウトのサンプル数を計算
			long fadeInSamples = (long)(In * sampleRate);
			long fadeOutSamples = (long)(Out * sampleRate);

			// 出力用のサンプル配列を作成（元のデータをコピー）
			double[] outputSamples = new double[input.Samples.Length];
			Array.Copy(input.Samples, outputSamples, input.Samples.Length);

			// 各サンプルを処理
			for (long i = 0; i < chunkLength; i++)
			{
				double fadeMultiplier = 1.0;

				// オブジェクト全体での現在のサンプル位置を計算
				long globalSamplePosition = context.CurrentSamplePosition + i;

				// フェードイン処理（オブジェクトの開始からfadeInSamplesの範囲）
				if (In > 0 && globalSamplePosition < fadeInSamples)
				{
					double fadeInMultiplier = (double)globalSamplePosition / fadeInSamples;
					fadeMultiplier *= fadeInMultiplier;
				}

				// フェードアウト処理（オブジェクトの終端からfadeOutSamplesの範囲）
				if (Out > 0 && globalSamplePosition >= totalObjectSamples - fadeOutSamples)
				{
					long samplesFromEnd = totalObjectSamples - globalSamplePosition;
					double fadeOutMultiplier = (double)samplesFromEnd / fadeOutSamples;
					fadeMultiplier *= fadeOutMultiplier;
				}

				// すべてのチャネルに同じフェード係数を適用
				for (int channel = 0; channel < channelCount; channel++)
				{
					long sampleIndex = i * channelCount + channel;
					if (sampleIndex < outputSamples.Length)
					{
						outputSamples[sampleIndex] *= fadeMultiplier;
					}
				}
			}

			// 新しいAudioChunkを作成して返す
			return new AudioChunk(input.Format, outputSamples);
		}
	}
}
