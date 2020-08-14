namespace Unosquare.FFME.Engine
{
    using Common;
    using Container;
    using Primitives;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Contains all the status properties of the stream being handled by the media engine.
    /// </summary>
    public sealed class MediaEngineState
    {
        #region Property Backing and Private State

        private static readonly IReadOnlyDictionary<string, string> EmptyDictionary = new Dictionary<string, string>(0);

        private readonly AtomicInteger m_MediaState = new AtomicInteger((int)MediaPlaybackState.Close);
        private readonly AtomicBoolean m_HasMediaEnded = new AtomicBoolean(default);

        private readonly AtomicBoolean m_IsBuffering = new AtomicBoolean(default);
        private readonly AtomicLong m_DecodingBitRate = new AtomicLong(default);
        private readonly AtomicDouble m_BufferingProgress = new AtomicDouble(default);
        private readonly AtomicDouble m_DownloadProgress = new AtomicDouble(default);
        private readonly AtomicLong m_PacketBufferLength = new AtomicLong(default);
        private readonly AtomicTimeSpan m_PacketBufferDuration = new AtomicTimeSpan(TimeSpan.MinValue);
        private readonly AtomicInteger m_PacketBufferCount = new AtomicInteger(default);

        private readonly AtomicTimeSpan m_FramePosition = new AtomicTimeSpan(default);
        private readonly AtomicTimeSpan m_Position = new AtomicTimeSpan(default);
        private readonly AtomicDouble m_SpeedRatio = new AtomicDouble(Constants.DefaultSpeedRatio);
        private readonly AtomicDouble m_Volume = new AtomicDouble(Constants.DefaultVolume);
        private readonly AtomicDouble m_Balance = new AtomicDouble(Constants.DefaultBalance);
        private readonly AtomicBoolean m_IsMuted = new AtomicBoolean(false);
        private readonly AtomicBoolean m_ScrubbingEnabled = new AtomicBoolean(true);
        private readonly AtomicBoolean m_VerticalSyncEnabled = new AtomicBoolean(true);

        private Uri m_Source;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaEngineState" /> class.
        /// </summary>
        /// <param name="mediaCore">The associated media core.</param>
        public MediaEngineState(MediaEngine mediaCore)
        {
            ResetAll();
        }

        #endregion

        #region Controller Properties

        /// <inheritdoc />
        public Uri Source
        {
            get => m_Source;
            private set => m_Source = value;
        }

        /// <inheritdoc />
        public double SpeedRatio
        {
            get => m_SpeedRatio.Value;
            set => m_SpeedRatio.Value = value;
        }

        /// <inheritdoc />
        public double Volume
        {
            get => m_Volume.Value;
            set => m_Volume.Value = value;
        }

        #endregion

        #region Renderer Update Driven Properties

        /// <inheritdoc />
        public MediaPlaybackState MediaState
        {
            get => (MediaPlaybackState)m_MediaState.Value;
            set
            {
                var oldState = (MediaPlaybackState)m_MediaState.Value;
                m_MediaState.Value = (int)value;
            }
        }

        /// <inheritdoc />
        public TimeSpan Position
        {
            get => m_Position.Value;
            private set => m_Position.Value = value;
        }

        /// <inheritdoc />
        public TimeSpan FramePosition
        {
            get => m_FramePosition.Value;
            private set => m_FramePosition.Value = value;
        }

        /// <inheritdoc />
        public bool HasMediaEnded
        {
            get => m_HasMediaEnded.Value;
            set
            {
                m_HasMediaEnded.Value = value;
            }
        }
       
        #endregion



        #region State Method Managed Media Properties

        /// <inheritdoc />
        public bool IsBuffering
        {
            get => m_IsBuffering.Value;
            private set => m_IsBuffering.Value = value;
        }

        /// <inheritdoc />
        public long DecodingBitRate
        {
            get => m_DecodingBitRate.Value;
            private set => m_DecodingBitRate.Value = value;
        }

        /// <inheritdoc />
        public double BufferingProgress
        {
            get => m_BufferingProgress.Value;
            private set => m_BufferingProgress.Value = value; 
        }

        /// <inheritdoc />
        public double DownloadProgress
        {
            get => m_DownloadProgress.Value;
            private set => m_DownloadProgress.Value = value;
        }

        /// <inheritdoc />
        public long PacketBufferLength
        {
            get => m_PacketBufferLength.Value;
            private set => m_PacketBufferLength.Value = value; 
        }

        /// <inheritdoc />
        public TimeSpan PacketBufferDuration
        {
            get => m_PacketBufferDuration.Value;
            private set => m_PacketBufferDuration.Value = value; 
        }

        /// <inheritdoc />
        public int PacketBufferCount
        {
            get => m_PacketBufferCount.Value;
            private set
            {
                m_PacketBufferCount.Value = value;
            }
        }

        #endregion

        #region State Management Methods

        /// <summary>
        /// Updates the <see cref="Source"/> property.
        /// </summary>
        /// <param name="newSource">The new source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSource(Uri newSource) => Source = newSource;


        /// <summary>
        /// Resets all media state properties.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetAll()
        {
            ResetMediaProperties();
            InitializeBufferingStatistics();
        }

        /// <summary>
        /// Resets the controller properties.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetMediaProperties()
        {
            // Reset Method-controlled properties
            Position = default;
            FramePosition = default;
            HasMediaEnded = default;


            // Reset controller properties
            SpeedRatio = Constants.DefaultSpeedRatio;

            MediaState = MediaPlaybackState.Close;
        }

        /// <summary>
        /// Resets all the buffering properties to their defaults.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InitializeBufferingStatistics()
        {
            
        }

        /// <summary>
        /// Updates the decoding bit rate and duration of the reference timing component.
        /// </summary>
        /// <param name="bitRate">The bit rate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateDecodingStats(long bitRate)
        {
            DecodingBitRate = bitRate;
        }


        #endregion
    }
}
