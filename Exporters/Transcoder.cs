using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace BeatmapExporter.Exporters
{
    public class Transcoder
    {
        readonly bool available;

        public bool Available
        {
            get => available;
        }

        public Transcoder()
        {
            try
            {
                Console.WriteLine("FFmpeg successfully loaded! .mp3 export for beatmaps that use other audio formats will be available.");
                available = true;
            }
            catch (Exception)
            {
                available = false;
            }
        }

        public bool TranscodeMP3(FileStream sourceFile, string destFile)
        {
            if (!available) throw new InvalidOperationException("Audio transcoder is not available within this runtime.");

            return FFMpegArguments
                .FromPipeInput(new StreamPipeSource(sourceFile))
                .OutputToFile(destFile, overwrite: false, o => o.WithAudioCodec(AudioCodec.LibMp3Lame))
                .ProcessSynchronously();
        }
    }
}
