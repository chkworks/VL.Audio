﻿
namespace VL.Audio
{
    public class AudioToValueSignal : SinkSignal
    {
        public AudioToValueSignal(AudioSignal input)
        {
            InputSignal.Value = input;
        }
        
        public float Value;

        protected override void FillBuffer(float[] buffer, int offset, int count)
        {
            if (InputSignal.Value != null) 
            {
                InputSignal.Read(buffer, offset, count);
                
                //just output the latest value
                Value = buffer[count - 1];
            }
        }
    }
}




