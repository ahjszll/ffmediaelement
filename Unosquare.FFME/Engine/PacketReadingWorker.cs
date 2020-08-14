﻿namespace Unosquare.FFME.Engine
{
    using Common;
    using Container;
    using Diagnostics;
    using Primitives;
    using System;
    using System.Threading;

    /// <summary>
    /// Implement packet reading worker logic.
    /// </summary>
    /// <seealso cref="IMediaWorker" />
    public sealed class PacketReadingWorker : IntervalWorkerBase, IMediaWorker, ILoggingSource
    {
        public PacketReadingWorker(MediaEngine mediaCore)
            : base(nameof(PacketReadingWorker))
        {
            MediaCore = mediaCore;
            Container = mediaCore.Container;

            Container.Data.OnDataPacketReceived = (dataPacket, stream) =>
            {
                try
                {
                    var dataFrame = new DataFrame(dataPacket, stream, MediaCore);
                }
                catch
                {
                    // ignore
                }
            };
        }

        /// <inheritdoc />
        public MediaEngine MediaCore { get; }

        /// <inheritdoc />
        ILoggingHandler ILoggingSource.LoggingHandler => MediaCore;

        /// <summary>
        /// Gets the Media Engine's container.
        /// </summary>
        private MediaContainer Container { get; }

        /// <inheritdoc />
        protected override void ExecuteCycleLogic(CancellationToken ct)
        {
            while (MediaCore.ShouldReadMorePackets)
            {
                if (Container.IsReadAborted || Container.IsAtEndOfStream || ct.IsCancellationRequested ||
                    WorkerState != WantedWorkerState)
                {
                    break;
                }

                try { Container.Read(); }
                catch (MediaContainerException) { /* ignore */ }
            }
        }

        /// <inheritdoc />
        protected override void OnCycleException(Exception ex) =>
            this.LogError(Aspects.ReadingWorker, "Worker Cycle exception thrown", ex);
    }
}
