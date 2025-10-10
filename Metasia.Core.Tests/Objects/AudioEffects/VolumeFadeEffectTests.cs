using NUnit.Framework;
using Moq;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Sounds;
using Metasia.Core.Objects;

namespace Metasia.Core.Tests.Objects.AudioEffects
{
    [TestFixture]
    public class VolumeFadeEffectTests
    {
        private VolumeFadeEffect _effect;
        private AudioFormat _stereoFormat;
        private AudioFormat _monoFormat;

        [SetUp]
        public void Setup()
        {
            _effect = new VolumeFadeEffect();
            _stereoFormat = new AudioFormat(44100, 2);
            _monoFormat = new AudioFormat(48000, 1);
        }

        /// <summary>
        /// 入力がnullの場合、元の入力を返すことを確認するテスト
        /// 入力チェックの命令網羅テスト
        /// </summary>
        [Test]
        public void Apply_InputIsNull_ReturnsOriginalInput()
        {
            // Arrange
            IAudioChunk input = null;
            var mockContext = new Mock<AudioEffectContext>(MockBehavior.Strict,
                new Mock<IAudible>().Object,
                new GetAudioContext(_stereoFormat, 0, 100, 60.0, 1.0));

            // Act
            var result = _effect.Apply(input, mockContext.Object);

            // Assert
            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// 入力サンプルがnullの場合、元の入力を返すことを確認するテスト
        /// 入力チェックの命令網羅テスト
        /// </summary>
        [Test]
        public void Apply_InputSamplesIsNull_ReturnsOriginalInput()
        {
            // Arrange
            var mockChunk = new Mock<IAudioChunk>();
            mockChunk.Setup(c => c.Samples).Returns((double[])null);
            mockChunk.Setup(c => c.Format).Returns(_stereoFormat);

            var mockContext = new Mock<AudioEffectContext>(MockBehavior.Strict,
                new Mock<IAudible>().Object,
                new GetAudioContext(_stereoFormat, 0, 100, 60.0, 1.0));

            // Act
            var result = _effect.Apply(mockChunk.Object, mockContext.Object);

            // Assert
            Assert.That(result, Is.SameAs(mockChunk.Object));
        }

        /// <summary>
        /// 入力サンプルが空の場合、元の入力を返すことを確認するテスト
        /// 入力チェックの命令網羅テスト
        /// </summary>
        [Test]
        public void Apply_InputSamplesIsEmpty_ReturnsOriginalInput()
        {
            // Arrange
            var mockChunk = new Mock<IAudioChunk>();
            mockChunk.Setup(c => c.Samples).Returns(new double[0]);
            mockChunk.Setup(c => c.Format).Returns(_stereoFormat);
            mockChunk.Setup(c => c.Length).Returns(0);

            var mockContext = new Mock<AudioEffectContext>(MockBehavior.Strict,
                new Mock<IAudible>().Object,
                new GetAudioContext(_stereoFormat, 0, 100, 60.0, 1.0));

            // Act
            var result = _effect.Apply(mockChunk.Object, mockContext.Object);

            // Assert
            Assert.That(result, Is.SameAs(mockChunk.Object));
        }

        /// <summary>
        /// フェード時間が両方0の場合、元の入力を返すことを確認するテスト
        /// フェード時間チェックの命令網羅テスト
        /// </summary>
        [Test]
        public void Apply_BothFadeTimesAreZero_ReturnsOriginalInput()
        {
            // Arrange
            _effect.In = 0f;
            _effect.Out = 0f;

            var samples = new double[] { 0.5, 0.5, 0.5, 0.5 }; // 2 samples, 2 channels
            var chunk = new AudioChunk(_stereoFormat, samples);

            var mockContext = new Mock<AudioEffectContext>(MockBehavior.Strict,
                new Mock<IAudible>().Object,
                new GetAudioContext(_stereoFormat, 0, 100, 60.0, 1.0));

            // Act
            var result = _effect.Apply(chunk, mockContext.Object);

            // Assert
            Assert.That(result, Is.SameAs(chunk));
        }

        /// <summary>
        /// 正常な入力でフェードインのみ、正しくフェードインが適用されるか確認するテスト
        /// フェードイン処理の命令・分岐網羅テスト
        /// </summary>
        [Test]
        public void Apply_NormalInputWithFadeInOnly_CorrectlyAppliesFadeIn()
        {
            // Arrange
            _effect.In = 0.1f; // 0.1秒のフェードイン
            _effect.Out = 0f;  // フェードアウトなし

            // 44100Hz, 2chで0.2秒分のサンプルを作成
            int sampleRate = 44100;
            int channels = 2;
            int totalSamples = sampleRate * 2 / 10; // 0.2秒分
            var samples = new double[totalSamples * channels];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 1.0; // 全て1.0の信号
            }

            var format = new AudioFormat(sampleRate, channels);
            var chunk = new AudioChunk(format, samples);

            var getAudioContext = new GetAudioContext(format, 0, totalSamples, 60.0, 0.2);
            var context = new AudioEffectContext(new Mock<IAudible>().Object, getAudioContext);

            // Act
            var result = _effect.Apply(chunk, context);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.SameAs(chunk));
            Assert.That(result.Samples, Is.Not.Null);

            // フェードイン期間中のサンプルが徐々に増加していることを確認
            int fadeInSamples = (int)(0.1f * sampleRate);
            for (int i = 0; i < fadeInSamples; i++)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    int index = i * channels + ch;
                    double expectedMultiplier = (double)i / fadeInSamples;
                    Assert.That(result.Samples[index], Is.EqualTo(1.0 * expectedMultiplier).Within(0.001));
                }
            }

            // フェードイン終了後のサンプルは1.0のまま
            for (int i = fadeInSamples; i < totalSamples; i++)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    int index = i * channels + ch;
                    Assert.That(result.Samples[index], Is.EqualTo(1.0).Within(0.001));
                }
            }
        }

        /// <summary>
        /// 正常な入力でフェードアウトのみ、正しくフェードアウトが適用されるか確認するテスト
        /// フェードアウト処理の命令・分岐網羅テスト
        /// </summary>
        [Test]
        public void Apply_NormalInputWithFadeOutOnly_CorrectlyAppliesFadeOut()
        {
            // Arrange
            _effect.In = 0f;   // フェードインなし
            _effect.Out = 0.1f; // 0.1秒のフェードアウト

            // 44100Hz, 2chで0.2秒分のサンプルを作成
            int sampleRate = 44100;
            int channels = 2;
            int totalSamples = sampleRate * 2 / 10; // 0.2秒分
            var samples = new double[totalSamples * channels];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 1.0; // 全て1.0の信号
            }

            var format = new AudioFormat(sampleRate, channels);
            var chunk = new AudioChunk(format, samples);

            var getAudioContext = new GetAudioContext(format, 0, totalSamples, 60.0, 0.2);
            var context = new AudioEffectContext(new Mock<IAudible>().Object, getAudioContext);

            // Act
            var result = _effect.Apply(chunk, context);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.SameAs(chunk));
            Assert.That(result.Samples, Is.Not.Null);

            // フェードアウト期間中のサンプルが徐々に減少していることを確認
            int fadeOutSamples = (int)(0.1f * sampleRate);
            long totalObjectSamples = (long)(0.2 * sampleRate);
            long fadeOutStart = totalObjectSamples - fadeOutSamples;

            for (long i = 0; i < fadeOutStart; i++)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    int index = (int)(i * channels + ch);
                    Assert.That(result.Samples[index], Is.EqualTo(1.0).Within(0.001));
                }
            }

            for (long i = fadeOutStart; i < totalObjectSamples; i++)
            {
                double samplesFromEnd = totalObjectSamples - i;
                double expectedMultiplier = samplesFromEnd / fadeOutSamples;

                for (int ch = 0; ch < channels; ch++)
                {
                    int index = (int)(i * channels + ch);
                    Assert.That(result.Samples[index], Is.EqualTo(1.0 * expectedMultiplier).Within(0.001));
                }
            }
        }

        /// <summary>
        /// 正常な入力でフェードインとフェードアウト両方、正しく両方のフェードが適用されるか確認するテスト
        /// 両方の処理の命令・分岐網羅テスト
        /// </summary>
        [Test]
        public void Apply_NormalInputWithBothFadeInAndFadeOut_CorrectlyAppliesBothFades()
        {
            // Arrange
            _effect.In = 0.05f;  // 0.05秒のフェードイン
            _effect.Out = 0.05f; // 0.05秒のフェードアウト

            // 44100Hz, 2chで0.2秒分のサンプルを作成
            int sampleRate = 44100;
            int channels = 2;
            int totalSamples = sampleRate * 2 / 10; // 0.2秒分
            var samples = new double[totalSamples * channels];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 1.0; // 全て1.0の信号
            }

            var format = new AudioFormat(sampleRate, channels);
            var chunk = new AudioChunk(format, samples);

            var getAudioContext = new GetAudioContext(format, 0, totalSamples, 60.0, 0.2);
            var context = new AudioEffectContext(new Mock<IAudible>().Object, getAudioContext);

            // Act
            var result = _effect.Apply(chunk, context);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.SameAs(chunk));
            Assert.That(result.Samples, Is.Not.Null);

            // フェードイン期間の確認
            int fadeInSamples = (int)(0.05f * sampleRate);
            for (int i = 0; i < fadeInSamples; i++)
            {
                double expectedMultiplier = (double)i / fadeInSamples;
                for (int ch = 0; ch < channels; ch++)
                {
                    int index = i * channels + ch;
                    Assert.That(result.Samples[index], Is.EqualTo(1.0 * expectedMultiplier).Within(0.001));
                }
            }

            // フェードアウト期間の確認
            int fadeOutSamples = (int)(0.05f * sampleRate);
            long totalObjectSamples = (long)(0.2 * sampleRate);
            long fadeOutStart = totalObjectSamples - fadeOutSamples;

            for (long i = fadeOutStart; i < totalObjectSamples; i++)
            {
                double samplesFromEnd = totalObjectSamples - i;
                double expectedMultiplier = samplesFromEnd / fadeOutSamples;

                for (int ch = 0; ch < channels; ch++)
                {
                    int index = (int)(i * channels + ch);
                    Assert.That(result.Samples[index], Is.EqualTo(1.0 * expectedMultiplier).Within(0.001));
                }
            }
        }

        /// <summary>
        /// フェードイン境界条件、正確な位置でフェードが適用されるか確認するテスト
        /// フェードインの分岐網羅テスト
        /// </summary>
        [Test]
        public void Apply_FadeInBoundaryCondition_FadeAppliedAtExactPosition()
        {
            // Arrange
            _effect.In = 0.1f;
            _effect.Out = 0f;

            int sampleRate = 44100;
            int channels = 1;
            int totalSamples = sampleRate / 10; // 0.1秒分（フェードイン時間と同じ）
            var samples = new double[totalSamples];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 0.8; // 0.8の信号
            }

            var format = new AudioFormat(sampleRate, channels);
            var chunk = new AudioChunk(format, samples);

            var getAudioContext = new GetAudioContext(format, 0, totalSamples, 60.0, 0.1);
            var context = new AudioEffectContext(new Mock<IAudible>().Object, getAudioContext);

            // Act
            var result = _effect.Apply(chunk, context);

            // Assert
            Assert.That(result, Is.Not.SameAs(chunk));

            // 最後のサンプル（フェードイン終了位置）がほぼ1.0倍になっていることを確認
            int lastSampleIndex = totalSamples - 1;
            Assert.That(result.Samples[lastSampleIndex], Is.EqualTo(0.8).Within(0.01));

            // 最初のサンプルが0に近いことを確認
            Assert.That(result.Samples[0], Is.EqualTo(0.0).Within(0.01));
        }

        /// <summary>
        /// フェードアウト境界条件、正確な位置でフェードが適用されるか確認するテスト
        /// フェードアウトの分岐網羅テスト
        /// </summary>
        [Test]
        public void Apply_FadeOutBoundaryCondition_FadeAppliedAtExactPosition()
        {
            // Arrange
            _effect.In = 0f;
            _effect.Out = 0.1f;

            int sampleRate = 44100;
            int channels = 1;
            int totalSamples = sampleRate / 10; // 0.1秒分（フェードアウト時間と同じ）
            var samples = new double[totalSamples];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 0.7; // 0.7の信号
            }

            var format = new AudioFormat(sampleRate, channels);
            var chunk = new AudioChunk(format, samples);

            var getAudioContext = new GetAudioContext(format, 0, totalSamples, 60.0, 0.1);
            var context = new AudioEffectContext(new Mock<IAudible>().Object, getAudioContext);

            // Act
            var result = _effect.Apply(chunk, context);

            // Assert
            Assert.That(result, Is.Not.SameAs(chunk));

            // 最後のサンプル（フェードアウト終了位置）が0に近いことを確認
            int lastSampleIndex = totalSamples - 1;
            Assert.That(result.Samples[lastSampleIndex], Is.EqualTo(0.0).Within(0.01));

            // 最初のサンプルがほぼ元の値であることを確認
            Assert.That(result.Samples[0], Is.EqualTo(0.7).Within(0.01));
        }

        /// <summary>
        /// マルチチャンネル音声、すべてのチャンネルに同じフェードが適用されるか確認するテスト
        /// チャンネル処理の網羅テスト
        /// </summary>
        [Test]
        public void Apply_MultiChannelAudio_SameFadeAppliedToAllChannels()
        {
            // Arrange
            _effect.In = 0.05f;
            _effect.Out = 0.05f;

            int sampleRate = 44100;
            int channels = 4; // 4チャンネル
            int totalSamples = sampleRate / 10; // 0.1秒分
            var samples = new double[totalSamples * channels];

            // 異なる値で各チャンネルを初期化
            for (int i = 0; i < totalSamples; i++)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    samples[i * channels + ch] = (ch + 1) * 0.2; // 0.2, 0.4, 0.6, 0.8
                }
            }

            var format = new AudioFormat(sampleRate, channels);
            var chunk = new AudioChunk(format, samples);

            var getAudioContext = new GetAudioContext(format, 0, totalSamples, 60.0, 0.1);
            var context = new AudioEffectContext(new Mock<IAudible>().Object, getAudioContext);

            // Act
            var result = _effect.Apply(chunk, context);

            // Assert
            Assert.That(result, Is.Not.SameAs(chunk));

            // 同じ位置の全チャンネルが同じフェード係数で処理されていることを確認
            int testSampleIndex = 10; // テスト用の適当なサンプル位置
            double firstChannelValue = result.Samples[testSampleIndex * channels + 0];
            double multiplier = firstChannelValue / 0.2; // 元の値0.2に対する倍率

            for (int ch = 1; ch < channels; ch++)
            {
                double expectedValue = (ch + 1) * 0.2 * multiplier;
                double actualValue = result.Samples[testSampleIndex * channels + ch];
                Assert.That(actualValue, Is.EqualTo(expectedValue).Within(0.001));
            }
        }
    }
}
