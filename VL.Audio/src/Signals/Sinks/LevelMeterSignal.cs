﻿#region usings
using System;
#endregion
namespace VL.Audio
{
    public class LevelMeterSignal : SinkSignal
    {
        public LevelMeterSignal(AudioSignal input)
        {
            InputSignal.Value = input;
        }
        
        public float Max;

        protected override void FillBuffer(float[] buffer, int offset, int count)
        {
            if (InputSignal.Value != null) 
            {
                InputSignal.Read(buffer, offset, count);
                var max = 0.0f;
                for (int i = offset; i < count; i++) 
                {
                    max = Math.Max(max, Math.Abs(buffer[i]));
                }
                Max = max;
            }
        }
    }
}




