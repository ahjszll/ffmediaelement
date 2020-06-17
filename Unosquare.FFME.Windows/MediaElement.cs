namespace Unosquare.FFME
{
    using Common;
    using Engine;
    using Platform;
    using Primitives;
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    /// <summary>
    /// Represents a control that contains audio and/or video.
    /// In contrast with System.Windows.Controls.MediaElement, this version uses
    /// the FFmpeg library to perform reading and decoding of media streams.
    /// </summary>
    /// <seealso cref="UserControl" />
    /// <seealso cref="IUriContext" />
    [Localizability(LocalizationCategory.NeverLocalize)]
    [DefaultProperty(nameof(Source))]
    public sealed partial class MediaElement : IDisposable
    {
        #region Fields and Property Backing


        private readonly ConcurrentBag<string> PropertyUpdates = new ConcurrentBag<string>();
        private readonly AtomicBoolean m_IsStateUpdating = new AtomicBoolean(false);

        private bool m_IsDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaElement" /> class.
        /// </summary>
        public MediaElement()
        {
            try
            {

                if (!Library.IsInDesignMode)
                {
                    // Setup the media engine and property updates timer
                    MediaCore = new MediaEngine(this, new MediaConnector(this));
                    MediaCore.State.PropertyChanged += (s, e) => PropertyUpdates.Add(e.PropertyName);
                }
            }
            finally
            {
            }
        }

        #endregion

        #region Properties


        /// <summary>
        /// Provides access to various internal media renderer options.
        /// The default options are optimal to work for most media streams.
        /// This is an advanced feature and it is not recommended to change these
        /// options without careful consideration.
        /// </summary>
        public RendererOptions RendererOptions { get; } = new RendererOptions();



        #endregion

        #region Public API


        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        #endregion

        #region Methods

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="alsoManaged"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool alsoManaged)
        {
            if (!m_IsDisposed)
            {
                if (alsoManaged)
                {
                    MediaCore.Dispose();
                }

                m_IsDisposed = true;
            }
        }

        #endregion
    }
}