using System;
using NUnit.Framework;

namespace YoutubeExtractor.Tests
{
    /// <summary>
    /// Small series of unit tests for DownloadUrlResolver. Run these with NUnit.
    /// </summary>
    [TestFixture]
    public class DownloadUrlResolverTest
    {        
        [Test]
        public void TryNormalizedUrlForStandardYouTubeUrlShouldReturnSame()
        {
            var url = "http://youtube.com/watch?v=12345";            
            
            var normalizedUrl = String.Empty;

            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.AreEqual(url, normalizedUrl);
        }
        
        [Test]
        public void TryNormalizedrlForYouTuDotBeUrlShouldReturnNormalizedUrl()
        {
            var url = "http://youtu.be/12345";
            
            var normalizedUrl = String.Empty;
            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.AreEqual("http://youtube.com/watch?v=12345", normalizedUrl);
        }
        
        [Test]
        public void TryNormalizedUrlForMobileLinkShouldReturnNormalizedUrl()
        {
            var url = "http://m.youtube.com/?v=12345";
            
            var normalizedUrl = String.Empty;
            Assert.IsTrue(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));

            Assert.AreEqual("http://youtube.com/watch?v=12345", normalizedUrl);
        }
        
        [Test]
        public void GetNormalizedYouTubeUrlForBadLinkShouldReturnNull()
        {
            var url = "http://notAYouTubeUrl.com";
           
            var normalizedUrl = String.Empty;
            Assert.IsFalse(DownloadUrlResolver.TryNormalizeYoutubeUrl(url, out normalizedUrl));
            Assert.IsNull(normalizedUrl);
        }
    }
}
