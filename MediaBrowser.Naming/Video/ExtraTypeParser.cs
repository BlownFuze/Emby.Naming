﻿using MediaBrowser.Naming.Audio;
using MediaBrowser.Naming.Common;
using MediaBrowser.Naming.Logging;
using System;
using System.IO;
using System.Linq;

namespace MediaBrowser.Naming.Video
{
    public class ExtraTypeParser
    {
        private readonly ILogger _logger;
        private readonly VideoOptions _videoOptions;
        private readonly AudioOptions _audioOptions;

        public ExtraTypeParser(VideoOptions videoOptions, AudioOptions audioOptions, ILogger logger)
        {
            _videoOptions = videoOptions;
            _audioOptions = audioOptions;
            _logger = logger;
        }

        public ExtraResult GetExtraInfo(string path)
        {
            return _videoOptions.ExtraRules
                .Select(i => GetExtraInfo(path, i))
                .FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.ExtraType)) ?? new ExtraResult();
        }

        private ExtraResult GetExtraInfo(string path, ExtraRule rule)
        {
            var result = new ExtraResult();

            if (rule.MediaType == MediaType.Audio)
            {
                if (!new AudioFileParser(_audioOptions).IsAudioFile(path))
                {
                    return result;
                }
            }
            else if (rule.MediaType == MediaType.Video)
            {
                if (!new VideoResolver(_videoOptions, _audioOptions, _logger).IsVideoFile(path))
                {
                    return result;
                }
            }
            else
            {
                return result;
            }

            var filename = Path.GetFileNameWithoutExtension(path);

            if (rule.RuleType == ExtraRuleType.Filename)
            {
                if (string.Equals(filename, rule.Token, StringComparison.OrdinalIgnoreCase))
                {
                    result.ExtraType = rule.ExtraType;
                    result.Tokens.Add(rule.Token);
                }
            }

            else if (rule.RuleType == ExtraRuleType.Suffix)
            {
                if (filename.EndsWith(rule.Token, StringComparison.OrdinalIgnoreCase))
                {
                    result.ExtraType = rule.ExtraType;
                    result.Tokens.Add(rule.Token);
                }
            }

            return result;
        }
    }
}
