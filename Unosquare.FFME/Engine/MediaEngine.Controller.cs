namespace Unosquare.FFME.Engine
{
    using Common;
    using Container;
    using System;

    public partial class MediaEngine
    {
        /// <summary>
        /// The underlying media container that provides access to
        /// individual media component streams.
        /// </summary>
        public MediaContainer Container { get; set; }


        #region Public API

        public void Open(IMediaInputStream stream, Uri streamUri)
        {
            if (stream != null || streamUri != null)
            {
                Close();

                try
                {
                    // TODO: Sometimes when the stream can't be read, the sample player stays as if it were trying to open
                    // until the interrupt timeout occurs but and the Real-Time Clock continues. Strange behavior. Investigate more.

                    // Signal the initial state
                    var source = stream != null ? stream.StreamUri : streamUri;
                    State.ResetAll();
                    State.UpdateSource(source);

                    // Register FFmpeg libraries if not already done
                    //if (Library.LoadFFmpeg())
                    //{
                    //    // Log an init message
                    //    this.LogInfo(Aspects.EngineCommand,
                    //        $"{nameof(FFInterop)}.{nameof(FFInterop.Initialize)}: FFmpeg v{Library.FFmpegVersionInfo}");
                    //}

                    // Create a default stream container configuration object
                    var containerConfig = new ContainerConfiguration();

                    // Convert the URI object to something the Media Container understands (Uri to String)
                    var mediaSource = source.IsWellFormedOriginalString()
                        ? source.OriginalString
                        : Uri.EscapeUriString(source.OriginalString);

                    // When opening via URL (and not via custom input stream), fix up the protocols and stuff
                    if (stream == null)
                    {
                        try
                        {
                            // the async protocol prefix allows for increased performance for local files.
                            // or anything that is file-system related
                            if (source.IsFile || source.IsUnc)
                            {
                                // Set the default protocol Prefix
                                // The async protocol prefix by default does not ssem to provide
                                // any performance improvements. Just leaving it for future reference below.
                                // containerConfig.ProtocolPrefix = "async"
                                mediaSource = source.LocalPath;
                            }
                        }
                        catch { /* Ignore exception and continue */ }

                        // Support device URLs
                        // GDI GRAB: Example URI: device://gdigrab?desktop
                        if (string.IsNullOrWhiteSpace(source.Scheme) == false
                            && (source.Scheme == "format" || source.Scheme == "device")
                            && string.IsNullOrWhiteSpace(source.Host) == false
                            && string.IsNullOrWhiteSpace(containerConfig.ForcedInputFormat)
                            && string.IsNullOrWhiteSpace(source.Query) == false)
                        {
                            // Update the Input format and container input URL
                            // It is also possible to set some input options as follows:
                            // Example: streamOptions.PrivateOptions["framerate"] = "20"
                            containerConfig.ForcedInputFormat = source.Host;
                            mediaSource = Uri.UnescapeDataString(source.Query).TrimStart('?');
                            //this.LogInfo(Aspects.EngineCommand,
                            //    $"Media URI will be updated. Input Format: {source.Host}, Input Argument: {mediaSource}");
                        }
                    }

                    // Instantiate the public container using either a URL (default) or a custom input stream.
                    Container = stream == null ?
                        new MediaContainer(mediaSource, containerConfig, this) :
                        new MediaContainer(stream, containerConfig, this);

                    // Initialize the container
                    Container.Initialize();

                    // Side-load subtitles if requested

                    // Get the main container open
                    Container.Open();

                    State.InitializeBufferingStatistics();

                    Workers = new MediaWorkerSet(this);
                    Workers.Start();
                }
                catch
                {
                    try { Workers?.Dispose(); } catch { /* Ignore any exceptions and continue */ }
                    try { Container?.Dispose(); } catch { /* Ignore any exceptions and continue */ }
                    Container = null;
                    throw;
                }
            }
            else
            {
                Close();
            }
        }

        public bool Close()
        {
            Container?.SignalAbortReads(false);
            Workers?.Dispose();
            Container?.Dispose();
            Container = null;
            return true;
        }

        public bool Play()
        {
            Workers.Start();
            return true;
        }

        public bool Pause()
        {
            Workers.PauseAll();
            return true;
        }


        public bool Stop()
        {
            return true;
        }

        public bool Seek(TimeSpan position)
        {

            return true;//
            // TODO: Handle Cancellation token ct
            //var result = false;
            //var hasDecoderSeeked = false;
            //var startTime = DateTime.UtcNow;
            //var targetSeekMode = seekOperation.Mode;
            //var targetPosition = seekOperation.Position;
            //var hasSeekBlocks = false;

            //try
            //{
            //    var seekableType = MediaCore.Container.Components.SeekableMediaType;
            //    var all = MediaCore.Container.Components.MediaTypes;
            //    var initialPosition = MediaCore.PlaybackPosition;

            //    if (targetSeekMode == SeekMode.StepBackward || targetSeekMode == SeekMode.StepForward)
            //    {
            //        targetPosition = ComputeStepTargetPosition(targetSeekMode, mainBlocks, initialPosition);
            //    }
            //    else if (targetSeekMode == SeekMode.Stop)
            //    {
            //        targetPosition = TimeSpan.MinValue;
            //    }

            //    // Check if we already have the block. If we do, simply set the clock position to the target position
            //    // we don't need anything else. This implements frame-by frame seeking and we need to snap to a discrete
            //    // position of the main component so it sticks on it.
            //    if (mainBlocks.IsInRange(targetPosition))
            //    {
            //        MediaCore.ChangePlaybackPosition(targetPosition);
            //        return true;
            //    }

            //    // Let consumers know main blocks are not available
            //    hasDecoderSeeked = true;

            //    // wait for the current reading and decoding cycles
            //    // to finish. We don't want to interfere with reading in progress
            //    // or decoding in progress.
            //    MediaCore.Workers.PauseReadDecode();
            //    SeekBlocksAvailable.Reset();

            //    // Signal the starting state clearing the packet buffer cache
            //    // TODO: this may not be necessary because the container does this for us.
            //    // explore the possibility of removing this line
            //    MediaCore.Container.Components.ClearQueuedPackets(flushBuffers: true);

            //    // Capture seek target adjustment
            //    var adjustedSeekTarget = targetPosition;
            //    if (targetPosition != TimeSpan.MinValue && mainBlocks.IsMonotonic)
            //    {
            //        var targetSkewTicks = Convert.ToInt64(
            //            mainBlocks.MonotonicDuration.Ticks * (mainBlocks.Capacity / 2d));

            //        if (adjustedSeekTarget.Ticks >= targetSkewTicks)
            //            adjustedSeekTarget = TimeSpan.FromTicks(adjustedSeekTarget.Ticks - targetSkewTicks);
            //    }

            //    // Populate frame queues with after-seek operation
            //    var firstFrame = MediaCore.Container.Seek(adjustedSeekTarget);
            //    if (firstFrame != null)
            //    {
            //        // if we seeked to minvalue we really meant the first frame start time
            //        if (targetPosition == TimeSpan.MinValue)
            //            targetPosition = firstFrame.StartTime;

            //        // Ensure we signal media has not ended
            //        State.HasMediaEnded = false;

            //        // Clear Blocks and frames (This does not clear the preloaded subtitles)
            //        foreach (var mt in all)
            //            MediaCore.Blocks[mt].Clear();

            //        // reset the render times
            //        MediaCore.InvalidateRenderers();

            //        // Create the blocks from the obtained seek frames
            //        MediaCore.Blocks[firstFrame.MediaType]?.Add(firstFrame, MediaCore.Container);
            //        hasSeekBlocks = TrySignalBlocksAvailable(targetSeekMode, mainBlocks, targetPosition, hasSeekBlocks);

            //        // Decode all available queued packets into the media component blocks
            //        foreach (var mt in all)
            //        {
            //            while (ct.IsCancellationRequested == false)
            //            {
            //                var frame = MediaCore.Container.Components[mt].ReceiveNextFrame();
            //                if (frame == null) break;
            //            }
            //        }

            //        // Align to the exact requested position on the main component
            //        while (MediaCore.ShouldReadMorePackets && ct.IsCancellationRequested == false && hasSeekBlocks == false)
            //        {
            //            // Check if we are already in range
            //            hasSeekBlocks = TrySignalBlocksAvailable(targetSeekMode, mainBlocks, targetPosition, hasSeekBlocks);

            //            // Read the next packet
            //            var packetType = MediaCore.Container.Read();
            //            var blocks = MediaCore.Blocks[packetType];
            //            if (blocks == null) continue;

            //            // Get the next frame
            //            if (blocks.RangeEndTime.Ticks < targetPosition.Ticks || blocks.IsFull == false)
            //            {
            //                blocks.Add(MediaCore.Container.Components[packetType].ReceiveNextFrame(), MediaCore.Container);
            //                hasSeekBlocks = TrySignalBlocksAvailable(targetSeekMode, mainBlocks, targetPosition, hasSeekBlocks);
            //            }
            //        }
            //    }

            //    // Find out what the final, best-effort position was
            //    TimeSpan resultPosition;
            //    if (mainBlocks.IsInRange(targetPosition) == false)
            //    {
            //        // We don't have a a valid main range
            //        var minStartTimeTicks = mainBlocks.RangeStartTime.Ticks;
            //        var maxStartTimeTicks = mainBlocks.RangeEndTime.Ticks;

            //        this.LogWarning(Aspects.EngineCommand,
            //            $"SEEK TP: Target Pos {targetPosition.Format()} not between {mainBlocks.RangeStartTime.TotalSeconds:0.000} " +
            //            $"and {mainBlocks.RangeEndTime.TotalSeconds:0.000}");

            //        resultPosition = TimeSpan.FromTicks(targetPosition.Ticks.Clamp(minStartTimeTicks, maxStartTimeTicks));
            //    }
            //    else
            //    {
            //        resultPosition = mainBlocks.Count == 0 && targetPosition != TimeSpan.Zero ?
            //            initialPosition : // Unsuccessful. This initial position is simply what the clock was :(
            //            targetPosition; // Successful seek with main blocks in range
            //    }

            //    // Write a new Real-time clock position now.
            //    if (hasSeekBlocks == false)
            //        MediaCore.ChangePlaybackPosition(resultPosition);
            //}
            //catch (Exception ex)
            //{
            //    // Log the exception
            //    this.LogError(Aspects.EngineCommand, "SEEK ERROR", ex);
            //}
            //finally
            //{
            //    if (hasDecoderSeeked)
            //    {
            //        this.LogTrace(Aspects.EngineCommand,
            //            $"SEEK D: Elapsed: {startTime.FormatElapsed()} | Target: {targetPosition.Format()}");
            //    }

            //    SeekBlocksAvailable.Set();
            //    MediaCore.InvalidateRenderers();
            //    seekOperation.Dispose();
            //}
        }


        public bool StepForward()
        {
            return true;
        }


        public bool StepBackward()
        {
            return true;
        }
        #endregion
    }
}