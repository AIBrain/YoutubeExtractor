using System;
using YoutubeExtractor;

namespace ExampleApplication.Portable
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Run();

            Console.ReadLine();
        }

        private static async void Run()
        {
            var videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("http://www.youtube.com/watch?v=6bMmhKz6KXg");

            foreach (var videoInfo in videoInfos)
            {
                Console.WriteLine(videoInfo.DownloadUrl);
                Console.WriteLine();
            }
        }
    }
}