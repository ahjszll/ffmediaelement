namespace Unosquare.FFME.Container
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// 表示一组预先分配的相同媒体类型的媒体块
    /// A block buffer contains playback and pool blocks. Pool blocks are blocks that
    /// can be reused. Playback blocks are blocks that have been filled.
    /// This class is thread safe.
    /// </summary>
    public sealed class MediaBlockBuffer : IDisposable
    {
        #region Private Declarations

        /// <summary>
        /// The blocks that are available to be filled.
        /// </summary>
        private readonly Queue<MediaBlock> PoolBlocks;

        /// <summary>
        /// The blocks that are available for rendering.
        /// </summary>
        private readonly List<MediaBlock> PlaybackBlocks;

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
        public MediaBlockBuffer(int capacity, MediaType mediaType)
        {
            Capacity = capacity;
            MediaType = mediaType;
            PoolBlocks = new Queue<MediaBlock>(capacity + 1); // +1 to be safe and not degrade performance
            PlaybackBlocks = new List<MediaBlock>(capacity + 1); // +1 to be safe and not degrade performance

            // allocate the blocks
            for (var i = 0; i < capacity; i++)
                PoolBlocks.Enqueue(CreateBlock(mediaType));
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

        #region Indexer Properties

        /// <summary>
        /// Gets the <see cref="MediaBlock" /> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="MediaBlock"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The media block.</returns>
        public MediaBlock this[int index]
        {
            get { lock (SyncLock) return PlaybackBlocks[index]; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Dispose()
        {
            lock (SyncLock)
            {
                if (m_IsDisposed) return;
                m_IsDisposed = true;

                while (PoolBlocks.Count > 0)
                {
                    var block = PoolBlocks.Dequeue();
                    block.Dispose();
                }

                for (var i = PlaybackBlocks.Count - 1; i >= 0; i--)
                {
                    var block = PlaybackBlocks[i];
                    PlaybackBlocks.RemoveAt(i);
                    block.Dispose();
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
        public MediaBlock Add(MediaFrame source, MediaContainer container)
        {
            if (source == null) return null;

            lock (SyncLock)
            {
                try
                {
                    // if there are no available blocks, make room!
                    if (PoolBlocks.Count <= 0)
                    {
                        // Remove the first block from playback
                        var firstBlock = PlaybackBlocks[0];
                        PlaybackBlocks.RemoveAt(0);
                        PoolBlocks.Enqueue(firstBlock);
                    }

                    // Get a block reference from the pool and convert it!
                    var targetBlock = PoolBlocks.Dequeue();
                    var lastBlock = PlaybackBlocks.Count > 0 ? PlaybackBlocks[PlaybackBlocks.Count - 1] : null;

                    if (container.Convert(source, ref targetBlock) == false)
                    {
                        // return the converted block to the pool
                        PoolBlocks.Enqueue(targetBlock);
                        return null;
                    }

                    // Add the target block to the playback blocks
                    PlaybackBlocks.Add(targetBlock);

                    // return the new target block
                    return targetBlock;
                }
                finally
                {
                    // update collection-wide properties
                    UpdateCollectionProperties();
                }
            }
        }

        public void UseMediaBlock(Action<MediaBlock> act)
        {
            lock (SyncLock)
            {
                if (PlaybackBlocks.Count > 0)
                    act(this[0]);
            }
        }

        /// <summary>
        /// Clears all the playback blocks returning them to the
        /// block pool.
        /// </summary>
        public void Clear()
        {
            lock (SyncLock)
            {
                // return all the blocks to the block pool
                foreach (var block in PlaybackBlocks)
                    PoolBlocks.Enqueue(block);

                PlaybackBlocks.Clear();
                UpdateCollectionProperties();
            }
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
            // Update the playback blocks sorting
            if (PlaybackBlocks.Count > 0)
            {
                var maxBlockIndex = PlaybackBlocks.Count - 1;

                // Perform the sorting and assignment of Previous and Next blocks
                PlaybackBlocks.Sort();
                PlaybackBlocks[0].Index = 0;
                PlaybackBlocks[0].Previous = null;
                PlaybackBlocks[0].Next = maxBlockIndex > 0 ? PlaybackBlocks[1] : null;

                for (var blockIndex = 1; blockIndex <= maxBlockIndex; blockIndex++)
                {
                    PlaybackBlocks[blockIndex].Index = blockIndex;
                    PlaybackBlocks[blockIndex].Previous = PlaybackBlocks[blockIndex - 1];
                    PlaybackBlocks[blockIndex].Next = blockIndex + 1 <= maxBlockIndex ? PlaybackBlocks[blockIndex + 1] : null;
                }
            }

            m_Count = PlaybackBlocks.Count;
            m_IsFull = m_Count >= Capacity;
        }

        #endregion
    }
}
