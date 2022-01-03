using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_Mandelbrot.Models
{
    
    internal class Logic : ILogic
    {
        //Formula: Z^2 + C
        //Z = 0 as start
        //C = start location
        private readonly double StartConvergence = 3.0;
        

        /*
         * Calculation for mandelbrot fractal after each transformaton the point is checked on convergence. Once it does (greater than 2) it will return the iterationCount
         * each this is repeated as long the iterationCount is smaller than the maximum iterable number.
        */
        public int CalculateMandelbrotComplex(int MaxIterabeleNr, double x, double y)
        {
            Complex c = new Complex(x, y);
            int iterationCount = 0;
            Complex z = new Complex(0, 0);
            do
            {
                iterationCount++;
                z = (z * z) + c;
                double absValue = Complex.Abs(z);//absolute point / magnitude of point
                if (absValue > StartConvergence) return iterationCount;
            } while (iterationCount <= MaxIterabeleNr);
            return iterationCount;
        }

        public int CalculateMandelbrot(int MaxIterabeleNr, double a, double b)
        {
            int iterationCount = 0;
            double x = 0;
            double y = 0;
            double X = 0;
            double Y = 0;
            do
            {
                X = x * x - y * y + a;
                Y = 2 * x * y + b;
                x = X;
                y = Y;
                iterationCount++;
            } while (x * x + y * y < 4 && iterationCount < MaxIterabeleNr);
            return iterationCount++;
        }


        public int[] GenerateMandelbrot(Rect crdts, int bytesPerPxl, byte[] byteOfPxls, int bytesPerPxlRow, double xscale, double yscale, double offsetX, double offsetY, int maxIterabeleNr, double zoomFactor)
        {
            int[] count = new int[byteOfPxls.Length];
            Parallel.For(0, byteOfPxls.Length, i =>
           {
                //calculate positions on bitmap
                int pxly = i / bytesPerPxlRow;
               int pxlx = i % bytesPerPxlRow / bytesPerPxl;
                //Convert bitmap positions to mandelbrot fractal positions
                double x = (crdts.Left + pxlx * xscale) * zoomFactor - offsetX;//
                double y = (crdts.Top - pxly * yscale) * zoomFactor + offsetY;//
                                                                              //Calculate if position (point) is fractal 
                count[i] = CalculateMandelbrot(maxIterabeleNr, x, y);

           });
            return count;
        }
    }
    }

