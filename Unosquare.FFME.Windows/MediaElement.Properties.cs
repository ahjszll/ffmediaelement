#pragma warning disable SA1201 // Elements must appear in the correct order
#pragma warning disable SA1117 // Parameters must be on same line or separate lines
using System;
using System.ComponentModel;

namespace Unosquare.FFME
{
    public partial class MediaElement
    {
        #region Position Dependency Property

        /// <summary>
        /// Gets/Sets the Position property on the MediaElement.
        /// </summary>
        [Category(nameof(MediaElement))]
        [Description("Specifies the position of the underlying media. Set this property to seek though the media stream.")]
        public TimeSpan Position { get; set; }


        #endregion


    }
}
