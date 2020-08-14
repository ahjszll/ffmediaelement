namespace Unosquare.FFME.Engine
{
    using Common;
    using Diagnostics;
    using Primitives;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unosquare.FFME.Container;

    /// <summary>
    /// Represents a Media Engine that contains underlying streams of audio and/or video.
    /// It uses the fantastic FFmpeg library to perform reading and decoding of media streams.
    /// </summary>
    public sealed partial class MediaEngine : IDisposable, ILoggingSource, ILoggingHandler
    {
        private readonly AtomicBoolean m_IsDisposed = new AtomicBoolean(false);
        public Dictionary<MediaType, MediaFrameBuffer> Frames = new Dictionary<MediaType, MediaFrameBuffer>();

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaEngine" /> class.
        /// </summary>
        /// <param name="parent">The associated parent object.</param>
        /// <param name="connector">The parent implementing connector methods.</param>
        /// <exception cref="InvalidOperationException">Thrown when the static Initialize method has not been called.</exception>
        public MediaEngine()
        {
            State = new MediaEngineState(this);
            Frames[MediaType.Audio] = new MediaFrameBuffer(25, MediaType.Audio);
            Frames[MediaType.Video] = new MediaFrameBuffer(25, MediaType.Video);
        }

        #endregion

        #region Properties

        /// <summary>
        /// An event that is raised whenever a global FFmpeg message is logged.
        /// </summary>
        public static event EventHandler<LoggingMessage> FFmpegMessageLogged;

        /// <inheritdoc />
        ILoggingHandler ILoggingSource.LoggingHandler => this;

        /// <summary>
        /// Contains the Media Status.
        /// </summary>
        public MediaEngineState State { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed => m_IsDisposed.Value;


        #endregion

        #region Methods

        /// <inheritdoc />
        void ILoggingHandler.HandleLogMessage(LoggingMessage message) 
        { 
        
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (m_IsDisposed == true) 
                return;
            m_IsDisposed.Value = true;
        }

        /// <summary>
        /// Raises the FFmpeg message logged.
        /// </summary>
        /// <param name="message">The <see cref="LoggingMessage"/> instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RaiseFFmpegMessageLogged(LoggingMessage message) =>
            FFmpegMessageLogged?.Invoke(null, message);

        #endregion
    }
}
