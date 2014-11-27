﻿using MediaBrowser.Naming.Common;
using MediaBrowser.Naming.Logging;
using MediaBrowser.Naming.Video;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace MediaBrowser.Naming.Tests.Video
{
    /// <summary>
    /// Summary description for ExtraTests
    /// </summary>
    [TestClass]
    public class ExtraTests : BaseVideoTest
    {
        // Requirements
        // movie-deleted = ExtraType deletedscene

        // All of the above rules should be configurable through the options objects (ideally, even the ExtraTypes)

        [TestMethod]
        public void TestKodiExtras()
        {
            var videoOptions = new NamingOptions();

            Test("trailer.mp4", "trailer", videoOptions);
            Test("trailer.mp3", null, videoOptions);
            Test("300-trailer.mp4", "trailer", videoOptions);

            Test("theme.mp3", "themesong", videoOptions);
            Test("theme.mkv", null, videoOptions);

            Test("300-sample.mp4", null, videoOptions);
            Test("300-scene.mp4", null, videoOptions);
            Test("300-clip.mp4", null, videoOptions);

            Test("300-deleted.mp4", null, videoOptions);
            Test("300-deletedscene.mp4", null, videoOptions);
            Test("300-interview.mp4", null, videoOptions);
            Test("300-behindthescenes.mp4", null, videoOptions);
        }

        [TestMethod]
        public void TestExpandedExtras()
        {
            var videoOptions = new ExtendedNamingOptions();

            Test("trailer.mp4", "trailer", videoOptions);
            Test("trailer.mp3", null, videoOptions);
            Test("300-trailer.mp4", "trailer", videoOptions);

            Test("theme.mp3", "themesong", videoOptions);
            Test("theme.mkv", null, videoOptions);

            Test("300-sample.mp4", "sample", videoOptions);
            Test("300-scene.mp4", "scene", videoOptions);
            Test("300-clip.mp4", "clip", videoOptions);

            Test("300-deleted.mp4", "deletedscene", videoOptions);
            Test("300-deletedscene.mp4", null, videoOptions);
            Test("300-interview.mp4", "interview", videoOptions);
            Test("300-behindthescenes.mp4", "behindthescenes", videoOptions);
        }

        private void Test(string input, string expectedType, NamingOptions videoOptions)
        {
            var parser = GetExtraTypeParser(videoOptions);

            var extraType = parser.GetExtraInfo(input).ExtraType;

            if (expectedType == null)
            {
                Assert.IsNull(extraType);
            }
            else
            {
                Assert.AreEqual(expectedType, extraType, true, CultureInfo.InvariantCulture);
            }
        }

        private ExtraTypeParser GetExtraTypeParser(NamingOptions videoOptions)
        {
            return new ExtraTypeParser(videoOptions, new NullLogger());
        }
    }
}
