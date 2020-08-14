﻿namespace Unosquare.FFME
{
    using Common;
    using Diagnostics;
    using Engine;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public partial class MediaElement : ILoggingHandler, ILoggingSource, INotifyPropertyChanged
    {
        /// <inheritdoc />
        ILoggingHandler ILoggingSource.LoggingHandler => this;

        /// <summary>
        /// Provides access to the underlying media engine driving this control.
        /// This property is intended for advanced usages only.
        /// </summary>
        public MediaEngine MediaCore { get; }

        #region Public API

        /// <summary>
        /// Requests new media options to be applied, including stream component selection.
        /// Handle the <see cref="MediaChanging"/> event to set new <see cref="MediaOptions"/> based on
        /// <see cref="MediaInfo"/> properties.
        /// </summary>
        /// <returns>The awaitable command.</returns>
        public ConfiguredTaskAwaitable<bool> ChangeMedia() => Task.Run(async () =>
        {
            try { return await MediaCore.ChangeMedia(); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }
            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Begins or resumes playback of the currently loaded media.
        /// </summary>
        /// <returns>The awaitable command.</returns>
        public ConfiguredTaskAwaitable<bool> Play() => Task.Run(async () =>
        {
            try { return await MediaCore.Play(); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }
            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Pauses playback of the currently loaded media.
        /// </summary>
        /// <returns>The awaitable command.</returns>
        public ConfiguredTaskAwaitable<bool> Pause() => Task.Run(async () =>
        {
            try { return await MediaCore.Pause(); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }
            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Pauses and rewinds the currently loaded media.
        /// </summary>
        /// <returns>The awaitable command.</returns>
        public ConfiguredTaskAwaitable<bool> Stop() => Task.Run(async () =>
        {
            try { return await MediaCore.Stop(); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }
            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Seeks to the specified target position.
        /// This is an alternative to using the <see cref="Position"/> dependency property.
        /// </summary>
        /// <param name="target">The target time to seek to.</param>
        /// <returns>The awaitable command.</returns>
        public ConfiguredTaskAwaitable<bool> Seek(TimeSpan target) => Task.Run(async () =>
        {
            try { return await MediaCore.Seek(target); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }
            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Seeks a single frame forward.
        /// </summary>
        /// <returns>The awaitable command.</returns>
        public ConfiguredTaskAwaitable<bool> StepForward() => Task.Run(async () =>
        {
            try { return await MediaCore.StepForward(); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }
            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Seeks a single frame backward.
        /// </summary>
        /// <returns>The awaitable command.</returns>
        public ConfiguredTaskAwaitable<bool> StepBackward() => Task.Run(async () =>
        {
            try { return await MediaCore.StepBackward(); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }
            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Opens the specified URI.
        /// This is an alternative method of opening media vs using the
        /// <see cref="Source"/> Dependency Property.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The awaitable task.</returns>
        public ConfiguredTaskAwaitable<bool> Open(Uri uri) => Task.Run(async () =>
        {
            try { return await MediaCore.Open(uri); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }

            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Opens the specified custom input stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The awaitable task.</returns>
        public ConfiguredTaskAwaitable<bool> Open(IMediaInputStream stream) => Task.Run(async () =>
        {
            try { return await MediaCore.Open(stream); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }

            return false;
        }).ConfigureAwait(true);

        /// <summary>
        /// Closes the currently loaded media.
        /// </summary>
        /// <returns>The awaitable command.</returns>
        public ConfiguredTaskAwaitable<bool> Close() => Task.Run(async () =>
        {
            try { return await MediaCore.Close(); }
            catch (Exception ex) { PostMediaFailedEvent(ex); }

            return false;
        }).ConfigureAwait(true);

        #endregion

        /// <inheritdoc />
        void ILoggingHandler.HandleLogMessage(LoggingMessage message) =>
            RaiseMessageLoggedEvent(message);
    }


    [DefaultProperty(nameof(Source))]
    public sealed partial class MediaElement : IDisposable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaElement" /> class.
        /// </summary>
        public MediaElement()
        {
            try
            {
                MediaCore = new MediaEngine();
            }
            finally
            {
            }
        }

        public void Dispose() => Dispose(true);


        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="alsoManaged"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool alsoManaged)
        {
            MediaCore.Dispose();
        }
    }

}
