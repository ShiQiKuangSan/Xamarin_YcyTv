﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YcyTv.Renderers;
using YcyTv.Renderers.Interfaces;

namespace YcyTv.Droid.SourceHandlers
{
    public sealed class FileVideoSourceHandler : IVideoSourceHandler
    {
        /// <summary>
        /// Loads the video from the specified source.
        /// </summary>
        /// <param name="source">The source of the video file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The path to the video file.</returns>
        public Task<string> LoadVideoAsync(VideoSource source, CancellationToken cancellationToken = default(CancellationToken))
        {
            string path = null;
            var fileVideoSource = source as FileVideoSource;

            if (!string.IsNullOrEmpty(fileVideoSource?.File) && File.Exists(fileVideoSource.File))
            {
                path = fileVideoSource.File;
            }

            return Task.FromResult(path);
        }
    }
}