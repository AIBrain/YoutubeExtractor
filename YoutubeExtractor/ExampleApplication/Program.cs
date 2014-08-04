using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeExtractor;

namespace ExampleApplication {
    using System.Threading.Tasks;

    internal class Program {

        private static void DownloadAudio( IEnumerable<VideoInfo> videoInfos ) {
            /*
             * We want the first extractable video with the highest audio quality.
             */
            var video = videoInfos
                .Where( info => info.CanExtractAudio )
                .OrderByDescending( info => info.AudioBitrate )
                .First();

            /*
             * Create the audio downloader.
             * The first argument is the video where the audio should be extracted from.
             * The second argument is the path to save the audio file.
             */

            var audioDownloader = new AudioDownloader( video,
                                                      Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), video.Title + video.AudioExtension ) );

            // Register the progress events. We treat the download progress as 85% of the progress
            // and the extraction progress only as 15% of the progress, because the download will
            // take much longer than the audio extraction.
            audioDownloader.DownloadProgressChanged += ( sender, args ) => Console.WriteLine( args.ProgressPercentage * 0.85 );
            audioDownloader.AudioExtractionProgressChanged += ( sender, args ) => Console.WriteLine( 85 + args.ProgressPercentage * 0.15 );

            /*
             * Execute the audio downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            audioDownloader.Execute();
        }

        private static async Task< Boolean > DownloadVideo( IEnumerable<VideoInfo> videoInfos ) {


            //var video = videoInfos.First( info => info.VideoType == VideoType.Mp4 && info.Resolution == 360 );  //Select the first .mp4 video with 360p resolution
            var video = videoInfos.Where( info => info.VideoType == VideoType.Mp4 ).OrderByDescending( info => info.Resolution ).FirstOrDefault();  //Select the first .mp4 video with highest resolution
            if ( default(VideoInfo) == video ) {
                return false;
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            var videoDownloader = new VideoDownloader( video: video, savePath: Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), string.Format( "{0}{1}", video.Title, video.VideoExtension ) ) );

            // Register the ProgressChanged event and print the current progress
            videoDownloader.DownloadProgressChanged += ( sender, args ) => Console.WriteLine( args.ProgressPercentage );

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            await Task.Run( () => videoDownloader.Execute() );

            return true;
        }

        private static void Main() {
            // Our test youtube link
            DoaDownload();

            Console.ReadLine();
        }

        private static async void DoaDownload() {
            const string link = "https://www.youtube.com/watch?v=Q6omsDyFNlk";

            /*
             * Get the available video formats.
             * We'll work with them in the video and audio download examples.
             */
            var videoInfos = DownloadUrlResolver.GetDownloadUrls( link );

            //DownloadAudio(videoInfos);
            var result = await DownloadVideo( videoInfos );

            Console.WriteLine( result );
        }
    }
}