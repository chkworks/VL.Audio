﻿namespace VL.Audio
{
    /// <summary>
    /// Base class for nodes which work with buffers
    /// </summary>
    public abstract class BufferAudioSignal : AudioSignalInput
    {
        public BufferAudioSignal(string bufferKey)
        {
            FBufferKey = bufferKey;
            AudioService.BufferStorage.BufferSet += BufferStorage_BufferSet;
            AudioService.BufferStorage.BufferRemoved += BufferStorage_BufferRemoved;
            if (AudioService.BufferStorage.ContainsKey(FBufferKey))
            {
                SetBuffer(AudioService.BufferStorage[FBufferKey]);
            }
            else
            {
                SetBuffer(new float[AudioService.Engine.Settings.BufferSize]);
            }
        }

        void BufferStorage_BufferRemoved(object sender, BufferEventArgs e)
        {
            if (e.BufferName == FBufferKey)
            {
                SetBuffer(new float[AudioService.Engine.Settings.BufferSize]);
            }
        }

        void BufferStorage_BufferSet(object sender, BufferEventArgs e)
        {
            if (e.BufferName == FBufferKey)
            {
                SetBuffer(e.Buffer);
            }
        }

        protected void SetBuffer(float[] buffer)
        {
            FBuffer = buffer;
            FBufferSize = FBuffer.Length;
        }

        protected string FBufferKey;

        public string BufferKey
        {
            get
            {
                return FBufferKey;
            }
            set
            {
                if (FBufferKey != value)
                {
                    FBufferKey = value;
                    SetBuffer(AudioService.BufferStorage[FBufferKey]);
                }
            }
        }

        protected int FBufferSize;

        protected float[] FBuffer;

        protected override void FillBuffer(float[] buffer, int offset, int count)
        {
        }

        public override void Dispose()
        {
            AudioService.BufferStorage.BufferSet -= BufferStorage_BufferSet;
            AudioService.BufferStorage.BufferRemoved -= BufferStorage_BufferRemoved;
            base.Dispose();
        }
    }
}




