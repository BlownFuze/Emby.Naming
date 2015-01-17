﻿using MediaBrowser.Naming.Common;
using MediaBrowser.Naming.IO;
using MediaBrowser.Naming.TV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Patterns.Logging;

namespace MediaBrowser.Naming.Tests.TV
{
    [TestClass]
    public class EpisodeWithoutSeasonTests
    {
        [TestMethod]
        public void TestWithoutSeason1()
        {
            Test(@"\\server\anything_ep02.mp4", "anything", null, 2);
        }

        [TestMethod]
        public void TestWithoutSeason2()
        {
            Test(@"\\server\anything_ep_02.mp4", "anything", null, 2);
        }

        [TestMethod]
        public void TestWithoutSeason3()
        {
            Test(@"\\server\anything_part.II.mp4", "anything", null, 2);
        }

        [TestMethod]
        public void TestWithoutSeason4()
        {
            Test(@"\\server\anything_pt.II.mp4", "anything", null, 2);
        }

        [TestMethod]
        public void TestWithoutSeason5()
        {
            Test(@"\\server\anything_pt_II.mp4", "anything", null, 2);
        }

        private void Test(string path, string seriesName, int? seasonNumber, int? episodeNumber)
        {
            var options = new ExtendedNamingOptions();

            var result = new EpisodeResolver(options, new NullLogger(), new RegexProvider())
                .Resolve(path, FileInfoType.File);

            Assert.AreEqual(seasonNumber, result.SeasonNumber);
            Assert.AreEqual(episodeNumber, result.EpisodeNumber);
            //Assert.AreEqual(seriesName, result.SeriesName, true, CultureInfo.InvariantCulture);
        }
    }
}
