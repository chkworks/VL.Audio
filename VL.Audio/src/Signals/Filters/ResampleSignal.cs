﻿#region usings
using System;
#endregion
namespace VL.Audio
{
    public class ResamplerPullBuffer : CircularPullBuffer
    {
        R8BrainSampleRateConverter FConverter;
        
        public ResamplerPullBuffer(R8BrainSampleRateConverter converter)
            : base(4096)
        {
            FConverter = converter;
            
            //calculate needed input samples per buffer
            var ratio = FConverter.SourcRate / FConverter.DestinationRate;
            PullCount = (int)(AudioService.Engine.Settings.BufferSize * ratio * 2) ;
            
            //fill the buffer with the first pull
            Pull(FConverter.Latency + PullCount);
        }
        
        double[] FInBuffer = new double[1];
        double[] FOutBuffer = new double[1];
        float[] FOutFloats = new float[1];

        private AudioSignal FInput;

        public new AudioSignal Input
        {
            get 
            { 
                return FInput; 
            }
            set 
            {
                if (FInput != value)
                {
                    if(FInput != null)
                        FInput.ReleaseOwnership(this);
                    
                    FInput = value;

                    if(FInput != null)
                        FInput.TakeOwnership(this);
                }
            }
        }

        
        public override void Pull(int count)
        {
            if(FTmpBuffer.Length != count)
            {
                FTmpBuffer = new float[count];
                FInBuffer = new double[count];
            }
            
            if(Input != null)
            {
                Input.Reset(this);
                Input.Read(FTmpBuffer, 0, count);
            }
            else
            {
                FTmpBuffer.ReadSilence(0, count);
            }
            
            FTmpBuffer.ReadDouble(FInBuffer, 0, count);
            
            var samples = FConverter.Process(FInBuffer, ref FOutBuffer);
            
            if(FOutFloats.Length < samples)
            {
                FOutFloats = new float[samples];
            }
            
            FOutFloats.WriteDouble(FOutBuffer, 0, samples);
            
            Write(FOutFloats, 0, samples);
        }

    }
    
    public class ResampleSignal : AudioSignalInput
    {
        ResamplerPullBuffer FPullBuffer;

        R8BrainSampleRateConverter FConverter;

        public ResampleSignal(double srcRate, double dstRate, AudioSignal input, double reqTransBand = 3)
        {
            InputSignal.Value = input;
            InputSignal.ValueChanged = InputWasSet;
            SetupConverter(srcRate, dstRate, reqTransBand);
        }

        public void SetupConverter(double srcRate, double dstRate, double reqTransBand = 3)
        {
            if (DestinationRateIsEngineRate)
                dstRate = WaveFormat.SampleRate;
            if (FConverter == null || FConverter.SourcRate != srcRate || FConverter.DestinationRate != dstRate) {
                FConverter = new R8BrainSampleRateConverter(srcRate, dstRate, 4096, reqTransBand, R8BrainResamplerResolution.R8Brain24);
                FPullBuffer = new ResamplerPullBuffer(FConverter);
                FPullBuffer.Input = InputSignal.Value;
            }
        }

        public bool DestinationRateIsEngineRate {
            get;
            set;
        }

        protected override void Engine_SampleRateChanged(object sender, EventArgs e)
        {
            base.Engine_SampleRateChanged(sender, e);
            if (DestinationRateIsEngineRate) {
                SetupConverter(FConverter.SourcRate, WaveFormat.SampleRate, FConverter.RequiredTransitionBand);
            }
        }

        /// <summary>
        /// The converter input latency
        /// </summary>
        public int Latency {
            get {
                return FConverter.Latency;
            }
        }

        //set new input for the pull buffer
        protected void InputWasSet(AudioSignal newInput)
        {
            FPullBuffer.Input = newInput;
        }

        //just read from the pull buffer
        protected override void FillBuffer(float[] buffer, int offset, int count)
        {
            FPullBuffer.Read(buffer, offset, count);
        }
    }
}




