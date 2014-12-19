﻿using MediaBrowser.Naming.Common;
using MediaBrowser.Naming.IO;
using MediaBrowser.Naming.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MediaBrowser.Naming.Video
{
    public class VideoListResolver
    {
        private readonly ILogger _logger;
        private readonly NamingOptions _options;
        private readonly IRegexProvider _regexProvider;

        public VideoListResolver(NamingOptions options, ILogger logger)
            : this(options, logger, new RegexProvider())
        {
        }

        public VideoListResolver(NamingOptions options, ILogger logger, IRegexProvider regexProvider)
        {
            _options = options;
            _logger = logger;
            _regexProvider = regexProvider;
        }

        public IEnumerable<VideoInfo> Resolve(List<PortableFileInfo> files, bool supportMultiVersion = true)
        {
            var videoResolver = new VideoResolver(_options, _logger, _regexProvider);

            var videoInfos = files
                .Select(i => videoResolver.Resolve(i.FullName, i.Type))
                .Where(i => i != null)
                .ToList();

            // Filter out all extras, otherwise they could cause stacks to not be resolved
            // See the unit test TestStackedWithTrailer
            var nonExtras = videoInfos
                .Where(i => string.IsNullOrWhiteSpace(i.ExtraType))
                .Select(i => new PortableFileInfo
                {
                    FullName = i.Path,
                    Type = i.FileInfoType
                });

            var stackResult = new StackResolver(_options, _logger, _regexProvider)
                .Resolve(nonExtras);

            var remainingFiles = videoInfos
                .Where(i => !stackResult.Stacks.Any(s => s.ContainsFile(i.Path, i.FileInfoType)))
                .ToList();

            var list = new List<VideoInfo>();

            foreach (var stack in stackResult.Stacks)
            {
                var info = new VideoInfo
                {
                    Files = stack.Files.Select(i => videoResolver.Resolve(i, stack.Type)).ToList(),
                    Name = stack.Name
                };

                info.Year = info.Files.First().Year;

                var extraBaseNames = new List<string> 
                {
                    stack.Name, 
                    Path.GetFileNameWithoutExtension(stack.Files[0])
                };

                var extras = GetExtras(remainingFiles, extraBaseNames);

                if (extras.Count > 0)
                {
                    remainingFiles = remainingFiles
                        .Except(extras)
                        .ToList();

                    info.Extras = extras;
                }

                list.Add(info);
            }

            var standaloneMedia = remainingFiles
                .Where(i => string.IsNullOrWhiteSpace(i.ExtraType))
                .ToList();

            foreach (var media in standaloneMedia)
            {
                var info = new VideoInfo
                {
                    Files = new List<VideoFileInfo> { media },
                    Name = media.Name
                };

                info.Year = info.Files.First().Year;

                var extras = GetExtras(remainingFiles, new List<string> { media.FileNameWithoutExtension, media.Name });

                remainingFiles = remainingFiles
                    .Except(extras.Concat(new[] { media }))
                    .ToList();

                info.Extras = extras;

                list.Add(info);
            }

            // If there's only one resolved video, use the folder name as well to find extras
            if (list.Count == 1)
            {
                var info = list[0];
                var videoPath = list[0].Files[0].Path;
                var parentPath = Path.GetDirectoryName(videoPath);

                if (!string.IsNullOrWhiteSpace(parentPath))
                {
                    var folderName = Path.GetFileName(Path.GetDirectoryName(videoPath));
                    if (!string.IsNullOrWhiteSpace(folderName))
                    {
                        var extras = GetExtras(remainingFiles, new List<string> { folderName });

                        remainingFiles = remainingFiles
                            .Except(extras)
                            .ToList();

                        info.Extras.AddRange(extras);
                    }
                }

                // Add the extras that are just based on file name as well
                var extrasByFileName = remainingFiles
                    .Where(i => i.ExtraRule != null && i.ExtraRule.RuleType == ExtraRuleType.Filename)
                    .ToList();

                remainingFiles = remainingFiles
                    .Except(extrasByFileName)
                    .ToList();

                info.Extras.AddRange(extrasByFileName);
            }

            // Whatever files are left, just add them

            list.AddRange(remainingFiles.Select(i => new VideoInfo
            {
                Files = new List<VideoFileInfo> { i },
                Name = i.Name,
                Year = i.Year
            }));

            var orderedList = list.OrderBy(i => i.Name);

            if (supportMultiVersion)
            {
                return GetVideosGroupedByVersion(orderedList);
            }

            return orderedList;
        }

        private IEnumerable<VideoInfo> GetVideosGroupedByVersion(IEnumerable<VideoInfo> videos)
        {
            var list = new List<VideoInfo>();

            foreach (var video in videos)
            {
                var match = list
                    .FirstOrDefault(i => string.Equals(i.Name, video.Name, StringComparison.OrdinalIgnoreCase));

                if (match != null && video.Files.Count == 1 && match.Files.Count == 1)
                {
                    match.AlternateVersions.Add(video.Files[0]);
                    match.Extras.AddRange(video.Extras);
                }
                else
                {
                    list.Add(video);
                }
            }

            return list;
        }

        private List<VideoFileInfo> GetExtras(IEnumerable<VideoFileInfo> remainingFiles, List<string> baseNames)
        {
            foreach (var name in baseNames.ToList())
            {
                var trimmedName = name.TrimEnd().TrimEnd(_options.VideoFlagDelimiters).TrimEnd();
                baseNames.Add(trimmedName);
            }

            return remainingFiles
                .Where(i => !string.IsNullOrWhiteSpace(i.ExtraType))
                .Where(i => baseNames.Any(b => i.FileNameWithoutExtension.StartsWith(b, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
