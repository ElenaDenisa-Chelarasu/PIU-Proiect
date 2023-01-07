using SFAudioCore.DataTypes;

namespace SFAudioTests
{
    public class AudioEngineTests
    {
        [Fact]
        public void RenderInto_WithInvalidInterval_ThrowsException()
        {
            var engine = new AudioEngine();

            var test = () =>
            {
                engine.RenderInto(100, 0, Array.Empty<float>());
            };

            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Contains("Invalid args", exception.Message);
        }

        [Fact]
        public void RenderInto_WithInvalidArray_ThrowsException()
        {
            var engine = new AudioEngine();

            var test = () =>
            {
                engine.RenderInto(0, 100, Array.Empty<float>());
            };

            var exception = Assert.Throws<InvalidOperationException>(test);
            Assert.Contains("Badly sized float data", exception.Message);
        }

        [Fact]
        public void RenderInto_WithValidArguments_WithNoAudio_ReturnsZeroedArray()
        {
            var engine = new AudioEngine();

            var floatData = new float[100 * 2];
            engine.RenderInto(0, 100, floatData);

            foreach (float sample in floatData)
                Assert.Equal(0f, sample);
        }

        [Fact]
        public void RenderInto_WithValidArguments_RenderingAStereoTrack_ReturnsCorrectMixedData()
        {
            var engine = new AudioEngine();

            var audioFile = new AudioInstance(Enumerable.Repeat(0.78f, 50).ToArray(), 2, 44100);
            engine.AddAudio(audioFile);

            var floatData = new float[100 * 2];
            engine.RenderInto(0, 100, floatData);

            for (int i = 0; i < 50; i++)
                Assert.Equal(0.78f, floatData[i]);

            for (int i = 50; i < 200; i++)
                Assert.Equal(0f, floatData[i]);
        }

        [Fact]
        public void RenderInto_WithValidArguments_RenderingAMonoTrack_ReturnsCorrectMixedData()
        {
            var engine = new AudioEngine();

            var audioFile = new AudioInstance(Enumerable.Repeat(0.78f, 25).ToArray(), 1, 44100);
            engine.AddAudio(audioFile);

            var floatData = new float[100 * 2];
            engine.RenderInto(0, 100, floatData);

            for (int i = 0; i < 50; i++)
                Assert.Equal(0.78f, floatData[i]);

            for (int i = 50; i < 200; i++)
                Assert.Equal(0f, floatData[i]);
        }

        [Fact]
        public void RenderInto_WithValidArguments_RenderingMultipleMixedChannelTracks_V1_ReturnsCorrectMixedData()
        {
            var engine = new AudioEngine();

            var audioFile = new AudioInstance(Enumerable.Repeat(0.78f, 50).ToArray(), 2, 44100);
            engine.AddAudio(audioFile);

            audioFile = new AudioInstance(Enumerable.Repeat(0.11f, 30).ToArray(), 1, 44100);
            engine.AddAudio(audioFile);

            var floatData = new float[100 * 2];
            engine.RenderInto(0, 100, floatData);

            for (int i = 0; i < 50; i++)
                Assert.Equal(0.78f + 0.11f, floatData[i]);

            for (int i = 50; i < 60; i++)
                Assert.Equal(0.11f, floatData[i]);

            for (int i = 60; i < 200; i++)
                Assert.Equal(0f, floatData[i]);
        }

        [Fact]
        public void RenderInto_WithValidArguments_RenderingMultipleMixedChannelTracks_WithChannelMuting_ReturnsCorrectMixedData()
        {
            var engine = new AudioEngine();

            var audioFile = new AudioInstance(Enumerable.Repeat(0.78f, 200).ToArray(), 2, 44100);
            audioFile.LeftMuted = true;
            engine.AddAudio(audioFile);

            audioFile = new AudioInstance(Enumerable.Repeat(0.11f, 100).ToArray(), 1, 44100);
            audioFile.RightMuted = true;
            engine.AddAudio(audioFile);

            var floatData = new float[100 * 2];
            engine.RenderInto(0, 100, floatData);

            for (int i = 0; i < 200; i += 2)
                Assert.Equal(0.11f, floatData[i]);

            for (int i = 1; i < 200; i += 2)
                Assert.Equal(0.78f, floatData[i]);
        }

        [Fact]
        public void RenderInto_WithValidArguments_RenderingMultipleMixedChannelTracks_WithChannelVolumes_ReturnsCorrectMixedData()
        {
            var engine = new AudioEngine();

            var audioFile = new AudioInstance(Enumerable.Repeat(0.78f, 200).ToArray(), 2, 44100);
            audioFile.LeftVolume = 0.25f;
            engine.AddAudio(audioFile);

            audioFile = new AudioInstance(Enumerable.Repeat(0.11f, 100).ToArray(), 1, 44100);
            audioFile.RightVolume = 0.45f;
            engine.AddAudio(audioFile);

            var floatData = new float[100 * 2];
            engine.RenderInto(0, 100, floatData);

            for (int i = 0; i < 200; i += 2)
                Assert.Equal(0.78f * 0.25f + 0.11f, floatData[i]);

            for (int i = 1; i < 200; i += 2)
                Assert.Equal(0.78f + 0.11f * 0.45f, floatData[i]);
        }

        [Fact]
        public void RenderInto_WithValidArguments_RenderingPastPlayRegion_ReturnsZeroedArray()
        {
            var engine = new AudioEngine();

            var audioFile = new AudioInstance(Enumerable.Repeat(0.78f, 50).ToArray(), 2, 44100);
            engine.AddAudio(audioFile);

            audioFile = new AudioInstance(Enumerable.Repeat(0.11f, 30).ToArray(), 1, 44100);
            engine.AddAudio(audioFile);

            var floatData = new float[100 * 2];
            engine.RenderInto(1337420, 1337420 + 100, floatData);

            for (int i = 0; i < 200; i++)
                Assert.Equal(0f, floatData[i]);
        }
    }
}