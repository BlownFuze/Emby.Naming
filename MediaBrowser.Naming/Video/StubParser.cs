﻿using MediaBrowser.Naming.Logging;
using System;
using System.IO;
using System.Linq;

namespace MediaBrowser.Naming.Video
{
    public class StubParser
    {
        private readonly VideoOptions _options;
        private readonly ILogger _logger;

        public StubParser(VideoOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }

        public StubResult ParseFile(string path)
        {
            var result = new StubResult();
            var extension = Path.GetExtension(path) ?? string.Empty;
            
            if (_options.StubFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                result.IsStub = true;

                path = Path.GetFileNameWithoutExtension(path);

                var token = (Path.GetExtension(path) ?? string.Empty).TrimStart('.');

                foreach (var rule in _options.StubTypes)
                {
                    if (string.Equals(rule.Token, token, StringComparison.OrdinalIgnoreCase))
                    {
                        result.StubType = rule.StubType;
                    }
                }
            }

            return result;
        }
    }
}
