using System;
using System.Collections.Generic;
using System.Text;

namespace FilterIIRTestApp
{
    public class TransferFcnCoeffs
    {
        public float[] A;
        public float[] B;

        public float[] samplingPeriod;

        public int Order { get { return A.Length == B.Length ? A.Length - 1 : 0; } }

        public TransferFcnCoeffs(int len)
        {
            A = new float[len];
            B = new float[len];
            samplingPeriod = new float[len];
            return;
        }

        public TransferFcnCoeffs(TransferFcnCoeffs copy)
        {
            A = new float[copy.A.Length];
            B = new float[copy.B.Length];
            samplingPeriod = new float[copy.samplingPeriod.Length];

            Array.Copy(copy.A, A, copy.A.Length);
            Array.Copy(copy.B, B, copy.B.Length);
            Array.Copy(copy.samplingPeriod, samplingPeriod, copy.samplingPeriod.Length);
        }

        public static void CalculateCoeffsButterworth(TransferFcnCoeffs coeffs, float tSampling, float fCuttOff)
        {
            float wc = 2f * (float)Math.PI * fCuttOff * tSampling;  //cutt-off freq
            float K = (float)Math.Tan(wc * 0.5f);                   //cutt-off freq corrected for bilinear transform
            float K2 = K * K;
            switch (coeffs.Order)
            {
                case 0:
                    coeffs.A[0] = 1f;
                    coeffs.B[0] = 1f;
                    break;
                case 1:
                    float alpha = 1 + K;
                    coeffs.A[0] = 1.0f;
                    coeffs.A[1] = -(1f - K) / alpha;
                    coeffs.B[0] = K / alpha;
                    coeffs.B[1] = K / alpha;
                    break;
                case 2:
                    float _K = 1f / (1f + 3f * K2);
                    coeffs.A[0] = K2 * _K;
                    coeffs.A[1] = 2f * K2 * _K;
                    coeffs.A[2] = coeffs.A[0];
                    coeffs.B[0] = 1f;
                    coeffs.B[1] = (2f * K2 - 2f) * _K;
                    coeffs.B[2] = (1 - K2) * _K;
                    break;
                default:
                    break;
            }
            


        }

        public static void CalculateCoeffs(TransferFcnCoeffs coeffs, float tSampling, float fCuttOff, float damping)
        {
            float wc = 0f;
            float K = 0f;

            float[] Ks = new float[3];

            switch (coeffs.Order)
            {
                case 0:
                    coeffs.A[0] = 1f;
                    coeffs.B[0] = 1f;
                    break;
                case 1:
                    wc = 2f * (float)Math.PI * fCuttOff * tSampling;  //cutt-off freq
                    K = (float)Math.Tan(wc * 0.5f);                   //cutt-off freq corrected for bilinear transform
                    float alpha = 1 + K;
                    coeffs.A[0] = 1.0f;
                    coeffs.A[1] = -(1f - K) / alpha;
                    coeffs.B[0] = K / alpha;
                    coeffs.B[1] = K / alpha;
                    break;
                case 2:
                    wc = 2f * (float)Math.PI * fCuttOff;
                    

                    float b0 = 0;
                    float b1 = 0;
                    float b2 = wc * wc;

                    float a0 = 1;
                    float a1 = 2 * damping * wc;
                    float a2 = wc * wc;

                    K = wc / (float)Math.Tan(wc * tSampling * 0.5f);

                    float K2 = K * K;
                    float Norm = 1f / (a0 * K2 + a1 * K + a2);

                    coeffs.A[0] = 1f;
                    coeffs.A[1] = (2f*a2 - 2f*a0*K2) *Norm;
                    coeffs.A[2] = (a0 * K2 - a1 * K + a2) * Norm;
                    coeffs.B[0] = (b0* K2 + b1*K + b2) *Norm;
                    coeffs.B[1] = (2f*b2 - 2*b0* K2) *Norm;
                    coeffs.B[2] = (b0 * K2 - b1* K + b2) * Norm;

                    coeffs.samplingPeriod[2] = coeffs.samplingPeriod[1];
                    coeffs.samplingPeriod[1] = coeffs.samplingPeriod[0];
                    break;
                default:
                    break;
            }



        }

    }

    public class TransferFcn
    {
        public TransferFcnCoeffs Coeffs;
        public int Order { get { return Coeffs.Order; } }

        private float[] xk;
        private float[] yk;

        public TransferFcn()
        {

        }

        public TransferFcn(TransferFcnCoeffs coeffs)
        {
            Coeffs = new TransferFcnCoeffs(coeffs);
            xk = new float[coeffs.B.Length];
            yk = new float[coeffs.A.Length];

            Init();
        }

        public TransferFcn(TransferFcn copy)
        {
            Coeffs = new TransferFcnCoeffs(copy.Coeffs);
            xk = new float[copy.Coeffs.B.Length];
            yk = new float[copy.Coeffs.A.Length];

            Init();
        }

        public void Init()
        {
            for (int i = 0; i < xk.Length; i++)
            {
                xk[i] = 0f;
            }

            for (int i = 0; i < yk.Length; i++)
            {
                yk[i] = 0f;
            }
        }

        public float Step(float timeSpan, float x)
        {
            yk[0] = 0;
            xk[0] = x;

            switch(Order)
            {
                case 0:
                    yk[0] = Coeffs.B[0] / Coeffs.A[0] * xk[0];
                    break;

                case 1:
                    yk[0] = yk[0] + Coeffs.B[0] * xk[0] + Coeffs.B[1] * xk[1] - Coeffs.A[1] * yk[1];
                    yk[1] = yk[0];
                    xk[1] = xk[0];
                    break;

                case 2:
                    yk[0] = Coeffs.B[0] * xk[0] + Coeffs.B[1] * xk[1] + Coeffs.B[2] * xk[2] - Coeffs.A[1] * yk[1] - Coeffs.A[2] * yk[2];
                    yk[2] = yk[1];
                    yk[1] = yk[0];
                    xk[2] = xk[1];
                    xk[1] = xk[0];
                    break;

                default:
                    break;

            }

            return yk[0];
        }

        public void Set(float x, float y)
        {
            for (int i = 0; i < xk.Length; i++)
            {
                xk[i] = x;
            }

            for (int i = 0; i < yk.Length; i++)
            {
                yk[i] = y;
            } 
            return;
        }   
    }
}
