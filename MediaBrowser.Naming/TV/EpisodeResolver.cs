﻿using MediaBrowser.Naming.Common;
using MediaBrowser.Naming.IO;
using MediaBrowser.Naming.Logging;
using MediaBrowser.Naming.Video;
using System;
using System.IO;
using System.Linq;

namespace MediaBrowser.Naming.TV
{
    public class EpisodeResolver
    {
        private readonly NamingOptions _options;
        private readonly ILogger _logger;
        private readonly IRegexProvider _iRegexProvider;

        public EpisodeResolver(NamingOptions options, ILogger logger)
            : this(options, logger, new RegexProvider())
        {
        }

        public EpisodeResolver(NamingOptions options, ILogger logger, IRegexProvider iRegexProvider)
        {
            _options = options;
            _logger = logger;
            _iRegexProvider = iRegexProvider;
        }

        public EpisodeInfo ParseFile(string path)
        {
            return Resolve(path, FileInfoType.File);
        }

        public EpisodeInfo ParseDirectory(string path)
        {
            return Resolve(path, FileInfoType.Directory);
        }

        public EpisodeInfo Resolve(string path, FileInfoType type, bool fillExtendedInfo = true)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            var isStub = false;
            string container = null;
            string stubType = null;

            if (type == FileInfoType.File)
            {
                var extension = Path.GetExtension(path) ?? string.Empty;
                // Check supported extensions
                if (!_options.VideoFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    var stubResult = new StubResolver(_options, _logger).ResolveFile(path);

                    isStub = stubResult.IsStub;

                    // It's not supported. Check stub extensions
                    if (!isStub)
                    {
                        return null;
                    }

                    stubType = stubResult.StubType;
                }

                container = extension.TrimStart('.');
            }

            var flags = new FlagParser(_options).GetFlags(path);
            var format3DResult = new Format3DParser(_options, _logger).Parse(flags);

            var parsingResult = new EpisodePathParser(_options, _iRegexProvider)
                .Parse(path, type, fillExtendedInfo);
            
            return new EpisodeInfo
            {
                Path = path,
                Container = container,
                IsStub = isStub,
                EndingEpsiodeNumber = parsingResult.EndingEpsiodeNumber,
                EpisodeNumber = parsingResult.EpisodeNumber,
                SeasonNumber = parsingResult.SeasonNumber,
                SeriesName = parsingResult.SeriesName,
                StubType = stubType,
                Is3D = format3DResult.Is3D,
                Format3D = format3DResult.Format3D,
                IsByDate = parsingResult.IsByDate,
                Day = parsingResult.Day,
                Month = parsingResult.Month,
                Year = parsingResult.Year
            };
        }
    }
}
