using SFML.Audio;
using SFML.System;

namespace pongus
{
    class SoundAnalyzer
    {
        SoundBuffer buff;
        public Sound OpenedAudio { get; }
        public Time Duration
        {
            get => buff.Duration;
        }

        public SoundAnalyzer(string audioFilePath)
        {
            this.buff = new SoundBuffer(audioFilePath);
            this.OpenedAudio = new Sound(buff);

            this.OpenedAudio.Loop = true;
        }

        public short FetchSample(float seconds)
        {
            int sampleInd = (int)(this.buff.SampleRate * seconds) % this.buff.Samples.Length;
            return this.buff.Samples[sampleInd];
        }
    }
}
