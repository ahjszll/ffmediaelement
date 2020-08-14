namespace Unosquare.FFME.Container
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// </summary>
    public sealed class MediaFrameBuffer : IDisposable
    {
        #region Private Declarations

        /// <summary>
        /// The blocks that are available to be filled.
        /// </summary>
        private readonly Queue<MediaFrame> PoolFrame;

        /// <summary>
        /// Controls multiple reads and exclusive writes.
        /// </summary>
        private readonly object SyncLock = new object();

        private int m_Count;
        private bool m_IsFull;
        private bool m_IsDisposed;


        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaBlockBuffer"/> class.
        /// </summary>
        public MediaFrameBuffer(int capacity, MediaType mediaType)
        {
            Capacity = capacity;
            MediaType = mediaType;
            PoolFrame = new Queue<MediaFrame>(capacity + 1); // +1 to be safe and not degrade performance
        }

        #endregion

        #region Regular Properties

        /// <summary>
        /// Gets the media type of the block buffer.
        /// </summary>
        public MediaType MediaType { get; }

        /// <summary>
        /// Gets the maximum count of this buffer.
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed { get { lock (SyncLock) return m_IsDisposed; } }

        #endregion

        #region Collection Discrete Properties

        /// <summary>
        /// Gets a value indicating whether the playback blocks are all allocated.
        /// </summary>
        public bool IsFull { get { lock (SyncLock) return m_IsFull; } }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Dispose()
        {
            lock (SyncLock)
            {
                if (m_IsDisposed) return;
                m_IsDisposed = true;

                while (PoolFrame.Count > 0)
                {
                    var frame = PoolFrame.Dequeue();
                    frame.Dispose();
                }
                UpdateCollectionProperties();
            }
        }

        /// <summary>
        /// Adds a block to the playback blocks by converting the given frame.
        /// If there are no more blocks in the pool, the oldest block is returned to the pool
        /// and reused for the new block. The source frame is automatically disposed.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="container">The container.</param>
        /// <returns>The filled block.</returns>
        public MediaFrame Add(MediaFrame source)
        {
            if (source == null) 
                return null;

            lock (SyncLock)
            {
                try
                {
                    if (PoolFrame.Count >= Capacity) 
                    {
                        source.Dispose();
                        return null;
                    }
                    PoolFrame.Enqueue(source);
                    return source;
                }
                finally
                {
                    // update collection-wide properties
                    UpdateCollectionProperties();
                }
            }
        }

        public List<MediaFrame> ReadAll()
        {
            List<MediaFrame> frameList = new List<MediaFrame>();
            lock (SyncLock)
            {
                try
                {
                    while (PoolFrame.Count>0) 
                    {
                        var frmae = PoolFrame.Dequeue();
                        frameList.Add(frmae);
                    }
                }
                finally
                {
                    // update collection-wide properties
                    UpdateCollectionProperties();
                }
            }
            return frameList;
        }


        /// <summary>
        /// Block factory method.
        /// </summary>
        /// <param name="mediaType">Type of the media.</param>
        /// <exception cref="InvalidCastException">MediaBlock does not have a valid type.</exception>
        /// <returns>An instance of the block of the specified type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MediaBlock CreateBlock(MediaType mediaType)
        {
            if (mediaType == MediaType.Video) return new VideoBlock();
            if (mediaType == MediaType.Audio) return new AudioBlock();

            throw new InvalidCastException($"No {nameof(MediaBlock)} constructor for {nameof(MediaType)} '{mediaType}'");
        }

        /// <summary>
        /// Updates the <see cref="PlaybackBlocks"/> collection properties.
        /// This method must be called whenever the collection is modified.
        /// The reason this exists is to avoid computing and iterating over these values every time they are read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCollectionProperties()
        {
            m_Count = PoolFrame.Count; 
            m_IsFull = m_Count >= Capacity;
        }

        #endregion
    }
}
