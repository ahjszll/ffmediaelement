namespace Unosquare.FFME.Engine
{
    using Common;
    using Diagnostics;
    using Primitives;
    using System;
    using System.Runtime.CompilerServices;

    public partial class MediaEngine
    {
        #region State Management

        /// <summary>
        /// Gets the buffer length maximum.
        /// port of MAX_QUEUE_SIZE (ffplay.c).
        /// </summary>
        public const long BufferLengthMax = 16 * 1024 * 1024;

        private readonly AtomicBoolean m_IsSyncBuffering = new AtomicBoolean(false);
        private readonly AtomicBoolean m_HasDecodingEnded = new AtomicBoolean(false);


        /// <summary>
        /// Gets the worker collection.
        /// </summary>
        public MediaWorkerSet Workers { get; set; }


        /// <summary>
        /// Gets a value indicating whether the decoder worker is sync-buffering.
        /// Sync-buffering is entered when there are no main blocks for the current clock.
        /// This in turn pauses the clock (without changing the media state).
        /// The decoder exits this condition when buffering is no longer needed and updates the clock position to what is available in the main block buffer.
        /// </summary>
        public bool IsSyncBuffering
        {
            get => m_IsSyncBuffering.Value;
            private set => m_IsSyncBuffering.Value = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the decoder worker has decoded all frames.
        /// This is an indication that the rendering worker should probe for end of media scenarios.
        /// </summary>
        public bool HasDecodingEnded
        {
            get => m_HasDecodingEnded.Value;
            set => m_HasDecodingEnded.Value = value;
        }

        /// <summary>
        /// Gets a value indicating whether packets can be read and room is available in the download cache.
        /// </summary>
        public bool ShouldReadMorePackets
        {
            get
            {
                if (Container?.Components == null)
                    return false;

                if (Container.IsReadAborted || Container.IsAtEndOfStream)
                    return false;

                // If it's a live stream always continue reading, regardless
                if (Container.IsLiveStream)
                    return true;

                // For network streams always expect a minimum buffer length
                if (Container.IsNetworkStream && Container.Components.BufferLength < BufferLengthMax)
                    return true;

                // if we don't have enough packets queued we should read
                return Container.Components.HasEnoughPackets == false;
            }
        }

        #endregion

    }
}
