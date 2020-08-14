namespace Unosquare.FFME.Engine
{
    using Common;
    using Container;
    using Diagnostics;
    using Primitives;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implement frame decoding worker logic.
    /// </summary>
    /// <seealso cref="IMediaWorker" />
    public sealed class FrameDecodingWorker : IntervalWorkerBase, IMediaWorker, ILoggingSource
    {
        private readonly Action<IEnumerable<MediaType>, CancellationToken> SerialDecodeBlocks;

        /// <summary>
        /// The decoded frame count for a cycle. This is used to detect end of decoding scenarios.
        /// </summary>
        private int DecodedFrameCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameDecodingWorker"/> class.
        /// </summary>
        /// <param name="mediaCore">The media core.</param>
        public FrameDecodingWorker(MediaEngine mediaCore)
            : base(nameof(FrameDecodingWorker))
        {
            MediaCore = mediaCore;
            Container = mediaCore.Container;

            SerialDecodeBlocks = (all, ct) =>
            {
                foreach (var t in Container.Components.MediaTypes)
                    DecodedFrameCount += DecodeComponentBlocks(t, ct);
            };

            Container.Components.OnFrameDecoded = (frame, type) =>
            {
                unsafe
                {
                    
                }
            };
        }

        /// <inheritdoc />
        public MediaEngine MediaCore { get; }

        /// <inheritdoc />
        ILoggingHandler ILoggingSource.LoggingHandler => MediaCore;

        /// <summary>
        /// Gets the Media Engine's Container.
        /// </summary>
        private MediaContainer Container { get; }

        /// <summary>
        /// Gets the Media Engine's State.
        /// </summary>
        private MediaEngineState State { get; }


        /// <inheritdoc />
        protected override void ExecuteCycleLogic(CancellationToken ct)
        {
            try
            {
                if (MediaCore.HasDecodingEnded || ct.IsCancellationRequested)
                    return;

                // Call the frame decoding logic
                DecodedFrameCount = 0;
                SerialDecodeBlocks.Invoke(Container.Components.MediaTypes, ct);
            }
            finally
            {
                // Detect End of Decoding Scenarios
                // The Rendering will check for end of media when this condition is set.
                MediaCore.HasDecodingEnded = DetectHasDecodingEnded();
            }
        }

        /// <inheritdoc />
        protected override void OnCycleException(Exception ex) =>
            this.LogError(Aspects.DecodingWorker, "Worker Cycle exception thrown", ex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int DecodeComponentBlocks(MediaType t, CancellationToken ct)
        {
            var decoderFrames = MediaCore.Frames[t]; // the blocks reference
            var addedFrames = 0; // the number of blocks that have been added
            var maxAddedBlocks = decoderFrames.Capacity; // the max blocks to add for this cycle

            while (addedFrames < maxAddedBlocks)
            {
                // We break decoding if we have a full set of blocks and if the
                // clock is not past the first half of the available block range
                if (decoderFrames.IsFull)
                    break;

                // Try adding the next block. Stop decoding upon failure or cancellation
                if (ct.IsCancellationRequested || AddNextFrame(t) == false)
                    break;

                // At this point we notify that we have added the block
                addedFrames++;
            }

            return addedFrames;

        }

        /// <summary>
        /// Tries to receive the next frame from the decoder by decoding queued
        /// Packets and converting the decoded frame into a Media Block which gets
        /// queued into the playback block buffer.
        /// </summary>
        /// <param name="t">The MediaType.</param>
        /// <returns>True if a block could be added. False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AddNextFrame(MediaType t)
        {
            //Container.Components[t].ReceiveNextFrame();
            //Decode the frames
            var block = MediaCore.Frames[t].Add(Container.Components[t].ReceiveNextFrame());
            return block != null;
        }

        /// <summary>
        /// Detects the end of media in the decoding worker.
        /// </summary>
        /// <returns>True if media docding has ended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DetectHasDecodingEnded() =>
            DecodedFrameCount <= 0 &&
            CanReadMoreFramesOf(Container.Components.SeekableMediaType) == false;

        /// <summary>
        /// Gets a value indicating whether more frames can be decoded into blocks of the given type.
        /// </summary>
        /// <param name="t">The media type.</param>
        /// <returns>
        ///   <c>true</c> if more frames can be decoded; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanReadMoreFramesOf(MediaType t)
        {
            return
                Container.Components[t].BufferLength > 0 ||
                Container.Components[t].HasPacketsInCodec ||
                MediaCore.ShouldReadMorePackets;
        }
    }
}
