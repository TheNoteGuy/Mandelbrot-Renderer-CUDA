extern "C" {
    __global__ void mandelbrotKernel(int* result, int width, int height, decimal XMin, decimal YMin, decimal xScale, decimal yScale, int MaxIterations, decimal brightness, int redPart, int greenPart, int bluePart) {
        int x = blockIdx.x * blockDim.x + threadIdx.x;
        int y = blockIdx.y * blockDim.y + threadIdx.y;

        if (x < width && y < height) {
            decimal zx = (decimal)0.0, zy = (decimal)0.0;
            decimal cx = XMin + x * xScale;
            decimal cy = YMin + y * yScale;
            int iterations = 0;

            while (iterations < MaxIterations && zx * zx + zy * zy < (decimal)4.0) {
                decimal temp = zx * zx - zy * zy + cx;
                zy = (decimal)2.0 * zx * zy + cy;
                zx = temp;
                iterations++;
            }

            if (iterations == MaxIterations)
                result[y * width + x] = MaxIterations;
            else
                result[y * width + x] = iterations;
        }
    }
}