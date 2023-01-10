﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchDetector
{
    public class PeriodFit
    {
        public bool IsValid;
        public int Period;
        public int Sample;
        public double SumS1, SumS2;
        public int CountSameSign;
        public double SumAbsDeltaDiff;
        public double PrevPeriodRatio;
        
        public int Count;
        public int SampleRate;
        public float Pitch;
        public float AvgPower;

        public double RelativeIncrease => SumS2 > 0 ? SumS1 / SumS2 : 1;
        public double SameSignRatio => Count > 0 ? CountSameSign / (double)Count : int.MaxValue;
        public double RatioSameSign => Count > 0 ? CountSameSign / (double)Count : int.MaxValue;
        public double Deviation => Count > 0 ? SumAbsDeltaDiff / RatioSameSign / Count: double.MaxValue;

        public double SomeMeasure => SumS1 > 0 ? SumAbsDeltaDiff / SumS1 : 0;

        public PeriodFit(int period, int sample, int sampleRate)
        {
            Period = period;
            Sample = sample;
            SampleRate = sampleRate;
            Pitch = SampleRate / (float)period;
        }

        public override string ToString()
        {
            return "Freq: "+Pitch.ToString("N1") +", Sample:"+ Sample
                +" Period: "+Period + ", Measure: "+ Deviation.ToString("N5") + ",SS:"+SameSignRatio.ToString("N2")+
                " SomeMeasure: "+ SomeMeasure.ToString("N2");
        }
    }
}
