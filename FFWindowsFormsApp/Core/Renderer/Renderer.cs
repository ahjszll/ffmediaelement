using FFmpeg.AutoGen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Unosquare.FFME.Common;
using Unosquare.FFME.Container;
using Unosquare.FFME.Diagnostics;

namespace FFWindowsFormsApp.Core.Package
{
    public unsafe class Renderer:MediaNode
    {
        private int MaxBuffer = 26;

        protected ConcurrentQueue<AudioBlock> PlaybackAudioBlock = new ConcurrentQueue<AudioBlock>();//播放帧集合
        protected ConcurrentQueue<VideoBlock> PlaybackVideoBlock = new ConcurrentQueue<VideoBlock>();

        protected ConcurrentQueue<AudioBlock> PoolAudioBlock = new ConcurrentQueue<AudioBlock>();//缓冲集合，提前申请内存，避免内存重复申请
        protected ConcurrentQueue<VideoBlock> PoolVideoBlock = new ConcurrentQueue<VideoBlock>();

        private SwsContext* Scaler = null;

        public Renderer()
        {
            for (var i = 0; i < MaxBuffer; i++)
            {
                PoolAudioBlock.Enqueue(new AudioBlock());
                PoolVideoBlock.Enqueue(new VideoBlock());
            }
        }

        public virtual void AddVideoFrame(MediaFrame mediaFrame)
        {
            if (PoolVideoBlock.Count == 0)
                return;

            VideoBlock target = null;
            if (PoolVideoBlock.TryDequeue(out target)) 
            {
                if (MaterializeVideoFrame(mediaFrame, ref target)) 
                {
                    PlaybackVideoBlock.Enqueue(target);
                }
            }
        }

        public virtual void AddAudioFrame(MediaFrame mediaFrame)
        {

        }

        public bool MaterializeVideoFrame(MediaFrame input, ref VideoBlock output)
        {
            if (output == null) output = new VideoBlock();
            if (input is VideoFrame == false || output is VideoBlock == false)
                throw new ArgumentNullException($"{nameof(input)} and {nameof(output)} are either null");

            var source = (VideoFrame)input;
            var target = (VideoBlock)output;

            // Retrieve a suitable scaler or create it on the fly
            var newScaler = ffmpeg.sws_getCachedContext(
                Scaler,
                source.Pointer->width,
                source.Pointer->height,
                NormalizePixelFormat(source.Pointer),
                source.Pointer->width,
                source.Pointer->height,
                AVPixelFormat.AV_PIX_FMT_BGRA,
                ffmpeg.SWS_POINT,
                null,
                null,
                null);

            // if it's the first time we set the scaler, simply assign it.
            if (Scaler == null)
            {
                Scaler = newScaler;
                RC.Current.Add(Scaler);
            }

            // Reassign to the new scaler and remove the reference to the existing one
            // The get cached context function automatically frees the existing scaler.
            if (Scaler != newScaler)
            {
                RC.Current.Remove(Scaler);
                Scaler = newScaler;
            }

            // Perform scaling and save the data to our unmanaged buffer pointer
            if (target.Allocate(source, AVPixelFormat.AV_PIX_FMT_BGRA)
                && target.TryAcquireWriterLock(out var writeLock))
            {
                using (writeLock)
                {
                    var targetStride = new[] { target.PictureBufferStride };
                    var targetScan = default(byte_ptrArray8);
                    targetScan[0] = (byte*)target.Buffer;

                    // The scaling is done here
                    var outputHeight = ffmpeg.sws_scale(
                        Scaler,
                        source.Pointer->data,
                        source.Pointer->linesize,
                        0,
                        source.Pointer->height,
                        targetScan,
                        targetStride);

                    if (outputHeight <= 0)
                        return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private AVPixelFormat NormalizePixelFormat(AVFrame* frame)
        {
            var currentFormat = (AVPixelFormat)frame->format;
            switch (currentFormat)
            {
                case AVPixelFormat.AV_PIX_FMT_YUVJ411P: return AVPixelFormat.AV_PIX_FMT_YUV411P;
                case AVPixelFormat.AV_PIX_FMT_YUVJ420P: return AVPixelFormat.AV_PIX_FMT_YUV420P;
                case AVPixelFormat.AV_PIX_FMT_YUVJ422P: return AVPixelFormat.AV_PIX_FMT_YUV422P;
                case AVPixelFormat.AV_PIX_FMT_YUVJ440P: return AVPixelFormat.AV_PIX_FMT_YUV440P;
                case AVPixelFormat.AV_PIX_FMT_YUVJ444P: return AVPixelFormat.AV_PIX_FMT_YUV444P;
                default: return currentFormat;
            }
        }

    }
}
