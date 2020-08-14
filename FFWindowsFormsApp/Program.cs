using FFmpeg.AutoGen;
using System;
using System.Windows.Forms;
using Unosquare.FFME;

namespace Unosquare.FFWindowsFormsApp
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Change the default location of the ffmpeg binaries(same directory as application)
            // You can get the 32-bit binaries here: https://ffmpeg.zeranoe.com/builds/win32/shared/ffmpeg-4.2.1-win32-shared.zip
            // You can get the 64-bit binaries here: https://ffmpeg.zeranoe.com/builds/win64/shared/ffmpeg-4.2.1-win64-shared.zip
            Library.FFmpegDirectory = @"c:\ffmpeg" + (Environment.Is64BitProcess ? @"\x64" : string.Empty);

            // You can pick which FFmpeg binaries are loaded. See issue #28
            // For more specific control (issue #414) you can set Library.FFmpegLoadModeFlags to:
            // FFmpegLoadMode.LibraryFlags["avcodec"] | FFmpegLoadMode.LibraryFlags["avfilter"] | ... etc.
            // Full Features is already the default.
            Library.FFmpegLogLevel = ffmpeg.AV_LOG_DEBUG;

            Library.FFmpegLoadModeFlags = FFmpegLoadMode.FullFeatures;
            Library.LoadFFmpeg();

            //SetupLogging();

            //ConfigureHWDecoder(out var deviceType);

            //Console.WriteLine("Decoding...");
            //DecodeAllFramesToImages(deviceType);

            //Console.WriteLine("Encoding...");
            //EncodeImagesToH264();

            Application.Run(new Form1());
        }

    }
}
