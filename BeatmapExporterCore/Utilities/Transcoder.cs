using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace BeatmapExporterCore.Utilities
{
    public class Transcoder
    {
        public Transcoder()
        {
            try
            {
                var _ = FFMpeg.GetAudioCodecs();
                Console.WriteLine("FFmpeg successfully loaded! .mp3 export for beatmaps that use other audio formats will be available.");
                Available = true;
            }
            catch (Exception)
            {
                Console.WriteLine("FFmpeg not found. Conversion to .mp3 for beatmap audio export will not be available.");
                Available = false;
            }
        }

        /// <summary>
        /// If the FFMpeg runtime was successfully when this Transcoder was initalized.
        /// </summary>
        public bool Available { get; }

        /// <summary>
        /// Performs a transcode on an audio file into MP3 format, saving it to disk.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the FFMpeg runtime was not found on application launch. This condition can be checked before attempting the transcode, <see cref="Available" /></exception>
        public bool TranscodeMP3(FileStream sourceFile, string destFile)
        {
            if (!Available) throw new InvalidOperationException("Audio transcoder is not available within this runtime.");

            return FFMpegArguments
                .FromPipeInput(new StreamPipeSource(sourceFile))
                .OutputToFile(destFile, overwrite: false, o => o.WithAudioCodec(AudioCodec.LibMp3Lame))
                .ProcessSynchronously();
        }
    }
}
