using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPF_Mandelbrot.Models
{
    public interface ILogic
    {
        int CalculateMandelbrot(int MaxIterabeleNr, double x, double y);
        int[] GenerateMandelbrot(Rect crdts, int bytesPerPxl, byte[] byteOfPxls, int bytesPerPxlRow, double xscale, double yscale, double offsetX, double offsetY, int maxIterabeleNr, double zoomFactor);
    }
}
