#include "cgbn.h"
#include "gpu_support.h"

extern "C" {
    __global__ void mandelbrotKernel(int* result, int width, int height, double XMin, double YMin, double xScale, double yScale, int MaxIterations, double brightness, int redPart, int greenPart, int bluePart) {
        int x = blockIdx.x * blockDim.x + threadIdx.x;
        int y = blockIdx.y * blockDim.y + threadIdx.y;

        if (x < width && y < height) {
            double zx = 0.0, zy = 0.0;
            double cx = XMin + x * xScale;
            double cy = YMin + y * yScale;
            int iterations = 0;

            while (iterations < MaxIterations && zx * zx + zy * zy < 4.0) {
                double temp = zx * zx - zy * zy + cx;
                zy = 2.0 * zx * zy + cy;
                zx = temp;
                iterations++;
            }

            if (iterations == MaxIterations)
                result[y * width + x] = 0; // Change this value based on your color scheme
            else
                result[y * width + x] = iterations;
        }
    }
}