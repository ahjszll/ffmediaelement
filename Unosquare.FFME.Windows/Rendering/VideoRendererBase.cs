namespace Unosquare.FFME.Rendering
{
    using Container;
    using Diagnostics;
    using Engine;
    using Platform;
    using Primitives;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows;

    /// <summary>
    /// Provides basic infrastructure for video rendering.
    /// </summary>
    /// <seealso cref="IMediaRenderer" />
    /// <seealso cref="ILoggingSource" />
    internal abstract class VideoRendererBase : IMediaRenderer, ILoggingSource
    {
        /// <summary>
        /// The default dpi.
        /// </summary>
        protected const double DefaultDpi = 96.0;

        /// <summary>
        /// Set when a bitmap is being written to the target bitmap.
        /// </summary>
        private readonly AtomicBoolean m_IsRenderingInProgress = new AtomicBoolean(false);

        /// <summary>
        /// Keeps track of the elapsed time since the last frame was displayed.
        /// for frame limiting purposes.
        /// </summary>
        private readonly Stopwatch RenderStopwatch = new Stopwatch();

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoRendererBase"/> class.
        /// </summary>
        /// <param name="mediaCore">The media core.</param>
        protected VideoRendererBase(MediaEngine mediaCore)
        {
            MediaCore = mediaCore;
        }

        /// <inheritdoc />
        ILoggingHandler ILoggingSource.LoggingHandler => MediaCore;

        /// <summary>
        /// Gets the parent media element (platform specific).
        /// </summary>
        public MediaElement MediaElement => MediaCore?.Parent as MediaElement;

        /// <inheritdoc />
        public MediaEngine MediaCore { get; }

        /// <summary>
        /// Gets the DPI along the X axis.
        /// </summary>
        public double DpiX { get; private set; } = DefaultDpi;

        /// <summary>
        /// Gets the DPI along the Y axis.
        /// </summary>
        public double DpiY { get; private set; } = DefaultDpi;

        /// <summary>
        /// Gets or sets a value indicating whether rendering is in progress.
        /// </summary>
        protected bool IsRenderingInProgress
        {
            get => m_IsRenderingInProgress.Value;
            set => m_IsRenderingInProgress.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether it is time to render after applying frame rate limiter.
        /// </summary>
        protected bool IsRenderTime
        {
            get
            {
                // Apply frame rate limiter (if active)
                var frameRateLimit = MediaElement.RendererOptions.VideoRefreshRateLimit;
                var result = frameRateLimit <= 0 || !RenderStopwatch.IsRunning || RenderStopwatch.ElapsedMilliseconds >= 1000d / frameRateLimit;
                return result;
            }
        }

        /// <inheritdoc />
        public virtual void OnPause()
        {
            // placeholder
        }

        /// <inheritdoc />
        public virtual void OnPlay()
        {
            // placeholder
        }

        /// <inheritdoc />
        public virtual void OnClose() { }

        /// <inheritdoc />
        public virtual void OnSeek() { }

        /// <inheritdoc />
        public virtual void OnStarting()
        {
            // placeholder
        }

        /// <inheritdoc />
        public virtual void Update(TimeSpan clockPosition)
        {
            // placeholder
        }

        /// <inheritdoc />
        public abstract void Render(MediaBlock mediaBlock, TimeSpan clockPosition);

        /// <inheritdoc />
        public virtual void OnStop() { }


        /// <summary>
        /// Begins the rendering cycle.
        /// </summary>
        /// <param name="mediaBlock">The media block.</param>
        /// <returns>The block for rendering. Returns null of not ready.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected VideoBlock BeginRenderingCycle(MediaBlock mediaBlock)
        {
            if (mediaBlock is VideoBlock == false) return null;

            var block = (VideoBlock)mediaBlock;
            if (IsRenderingInProgress)
            {
                if (MediaCore?.State.IsPlaying ?? false)
                    this.LogDebug(Aspects.VideoRenderer, $"{nameof(VideoRenderer)} frame skipped at {mediaBlock.StartTime}");

                return null;
            }

            // Flag the start of a rendering cycle
            IsRenderingInProgress = true;

            // VerticalSyncContext.Flush();
            // Send the packets to the CC renderer

            if (!IsRenderTime)
                return null;
            else
                RenderStopwatch.Restart();

            // Return block for rendering
            return block;
        }

        /// <summary>
        /// Finishes the rendering cycle.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="clockPosition">The clock position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FinishRenderingCycle(VideoBlock block, TimeSpan clockPosition)
        {
            // Alwasy set the progress to false to allow for next cycle.
            IsRenderingInProgress = false;

        }

        /// <summary>
        /// Updates the layout.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="clockPosition">The clock position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateLayout(VideoBlock block, TimeSpan clockPosition)
        {
            try
            {
            }
            catch (Exception ex)
            {
                this.LogError(Aspects.VideoRenderer, $"{nameof(VideoRenderer)}.{nameof(Render)} layout/CC failed.", ex);
            }
        }
    }
}
