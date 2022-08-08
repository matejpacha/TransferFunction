using System;
using System.IO;

namespace FilterIIRTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            float y = 0;
            float x = 1;
            float preY = 0;
            float timeSpanBase = 0.03f;
            float timeSpan = timeSpanBase;

            float fps_base = 30;
            float fps = fps_base;

            float damping = 0.45f;
            float cutoff = 1.5f;

            var rand = new Random();

            TransferFcnCoeffs coeffs = new TransferFcnCoeffs(3);
            TransferFcnCoeffs.CalculateCoeffs(coeffs, timeSpanBase, cutoff, damping);


            TransferFcn filter = new TransferFcn(coeffs);

            TransferFcnCoeffs.CalculateCoeffs(coeffs, timeSpanBase, cutoff, damping);
            TransferFcn prefilter = new TransferFcn(coeffs);

            prefilter.Init();
            filter.Init();  //Not needed - included in the constructor;

            using StreamWriter file = new StreamWriter("data.csv");

            for(float t = 0f; t < 10f; t += timeSpan)
            {
                fps = fps_base + 0.5f * fps_base * (float)rand.NextDouble() * (float)rand.Next(-1, 3);
                fps = 30;
                timeSpan = 1/fps;

                TransferFcnCoeffs.CalculateCoeffs(prefilter.Coeffs, timeSpanBase, cutoff, damping);
                preY = prefilter.Step(timeSpan, x);

                TransferFcnCoeffs.CalculateCoeffs(filter.Coeffs, timeSpan, cutoff, damping);
                y = filter.Step(timeSpan, preY);


                if(t > (5.0 - timeSpan/2f) && t < (5.0 + timeSpan/2f))
                {
                    //filter.Set(0.5f, 0.5f);
                    x = 0.0f;
                }

                Console.WriteLine("dt = " + timeSpan.ToString() + "  t = " + t.ToString("0.0") + "; y = " + y.ToString("0.00000"));
                file.WriteLine(t.ToString("0.00000") + ";" + y.ToString("0.00000") + ";" + timeSpan.ToString());
                
            }
            file.Close();
        }
    }
}
