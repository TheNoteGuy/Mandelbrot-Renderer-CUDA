using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedCuda;
using ManagedCuda.VectorTypes;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;


namespace ClaudeFractal
{
    public partial class Form1 : Form
    {
        public Decimal XMin { get; set; } = -2;
        public Decimal XMax { get; set; } = 2;
        public Decimal YMin { get; set; } = -2;
        public Decimal YMax { get; set; } = 2;
        public int MaxIterations { get; set; } = 100;

        public int redPart = 255;
        public int greenPart = 0;
        public int bluePart = 255;
        public double brightness = 200;

        public Form1()
        {
            InitializeComponent();
            ShowControlForm();
            ShowColorForm();
            this.DoubleBuffered = true; // Enable double buffering for smooth drawing
            RenderMandelbrot(this.ClientSize.Width, this.ClientSize.Height);
        }

        private void ShowControlForm()
        {
            ControlForm controlForm = new ControlForm(this);
            controlForm.ControlForm_Load();
            controlForm.Show();
        }

        private void ShowColorForm()
        {
            ColorForm colorform = new ColorForm(this);
            colorform.ColorForm_Load();
            colorform.Show();
        }

        public unsafe void RenderMandelbrot(int width, int height)
        {
            decimal xScale = (decimal)(XMax - XMin) / (decimal)width;
            decimal yScale = (decimal)(YMax - YMin) / (decimal)height;

            // Calculate the zoom level based on the current fractal dimensions
            decimal widthToRangeRatio = Decimal.Divide((decimal)width, (decimal)(XMax - XMin));
            decimal heightToRangeRatio = Decimal.Divide((decimal)height, (decimal)(YMax - YMin));
            decimal zoomLevel = Math.Max(widthToRangeRatio, heightToRangeRatio);

            // Determine the scaling factor based on the zoom level
            int scaleFactor = (int)(1 + Math.Log((double)zoomLevel));

            // Calculate the adjusted dimensions based on the scaling factor
            int adjustedWidth = width * scaleFactor;
            int adjustedHeight = height * scaleFactor;

            // Check if the adjusted dimensions exceed the maximum allowed size
            long maxSize = 8192; // Adjust this value based on your system's memory constraints
            if ((long)adjustedWidth * adjustedHeight > maxSize * maxSize)
            {
                // Scale down the dimensions to fit within the maximum size
                decimal scalingRatio =
                    (decimal)Math.Sqrt((double)(maxSize * maxSize) / (adjustedWidth * adjustedHeight));
                adjustedWidth = (int)((decimal)adjustedWidth * scalingRatio);
                adjustedHeight = (int)((decimal)adjustedHeight * scalingRatio);
            }

            // Check if the adjusted dimensions are within a valid range
            if (adjustedWidth <= 0 || adjustedHeight <= 0)
            {
                // Fallback to the original dimensions if adjusted dimensions are invalid
                adjustedWidth = width;
                adjustedHeight = height;
            }

            try
            {
                using (CudaContext ctx = new CudaContext())
                {
                    CudaDeviceVariable<int> devResult = new CudaDeviceVariable<int>(adjustedWidth * adjustedHeight);

                    var kernel = ctx.LoadKernel("Mandelbrot.ptx", "mandelbrotKernel");

                    kernel.BlockDimensions = new dim3(32, 32);
                    kernel.GridDimensions = new dim3(
                        (uint)(width + kernel.BlockDimensions.x - 1) / kernel.BlockDimensions.x,
                        (uint)(height + kernel.BlockDimensions.y - 1) / kernel.BlockDimensions.y);

                    kernel.Run(devResult.DevicePointer, width, height, (double)XMin, (double)YMin, (double)xScale,
                        (double)yScale, MaxIterations,
                        (double)brightness, redPart, greenPart, bluePart);

                    int[] result = new int[width * height];
                    devResult.CopyToHost(result);

                    byte[] imageData = new byte[width * height * 4];
                    Parallel.For(0, height, y =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int iterations = result[y * width + x];

                            if (iterations == MaxIterations)
                            {
                                // Black pixel
                                imageData[4 * (y * width + x)] = 0;
                                imageData[4 * (y * width + x) + 1] = 0;
                                imageData[4 * (y * width + x) + 2] = 0;
                                imageData[4 * (y * width + x) + 3] = 255;
                            }
                            else
                            {
                                int redComponent =
                                    Convert.ToInt32(Math.Min(iterations * (decimal)(brightness / 100), redPart));
                                int greenComponent =
                                    Convert.ToInt32(Math.Min(iterations * (decimal)(brightness / 100), greenPart));
                                int blueComponent =
                                    Convert.ToInt32(Math.Min(iterations * (decimal)(brightness / 100), bluePart));

                                imageData[4 * (y * width + x)] = (byte)blueComponent;
                                imageData[4 * (y * width + x) + 1] = (byte)greenComponent;
                                imageData[4 * (y * width + x) + 2] = (byte)redComponent;
                                imageData[4 * (y * width + x) + 3] = 255;
                            }
                        }
                    });

                    fixed (byte* ptr = imageData)
                    {
                        using (Bitmap bitmap = new Bitmap(width, height, width * 4,
                                   PixelFormat.Format32bppArgb, new IntPtr(ptr)))
                        {
                            using (Graphics formGraphics = this.CreateGraphics())
                            {
                                formGraphics.DrawImage(bitmap, 0, 0);
                            }
                        }
                    }
                }
            }
            catch (CudaException ex)
            {
                // Handle the CUDA exception
                Console.WriteLine("CUDA Exception: " + ex.Message);

                // Fallback to rendering the fractal without CUDA
                RenderMandelbrotFallback(width, height);
            }
        }

        private void RenderMandelbrotFallback(int width, int height)
        {
            ControlForm controlForm = new ControlForm(this);

            Decimal xScale = (XMax - XMin) / width;
            Decimal yScale = (YMax - YMin) / height;

            int taskCount = Environment.ProcessorCount;
            int chunkHeight = height / taskCount;

            Task<Bitmap>[] tasks = new Task<Bitmap>[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                int startY = i * chunkHeight;
                int endY = (i == taskCount - 1) ? height : (i + 1) * chunkHeight;

                tasks[i] = Task.Factory.StartNew(() =>
                {
                    Bitmap chunkBitmap = new Bitmap(width, endY - startY);
                    using (Graphics chunkGraphics = Graphics.FromImage(chunkBitmap))
                    {
                        for (int y = 0; y < endY - startY; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                Decimal zx = 0.0m, zy = 0.0m;
                                Decimal cx = XMin + x * xScale;
                                Decimal cy = YMin + (startY + y) * yScale;
                                int iterations = 0;

                                while (iterations < MaxIterations && zx * zx + zy * zy < 4.0m)
                                {
                                    Decimal temp = zx * zx - zy * zy + cx;
                                    zy = 2.0m * zx * zy + cy;
                                    zx = temp;
                                    iterations++;
                                }

                                if (iterations == MaxIterations)
                                    chunkGraphics.FillRectangle(Brushes.Black, x, y, 1, 1);
                                else
                                {
                                    int redComponent =
                                        Convert.ToInt32(Math.Min(iterations * (brightness / 100), redPart));
                                    int greenComponent =
                                        Convert.ToInt32(Math.Min(iterations * (brightness / 100), greenPart));
                                    int blueComponent =
                                        Convert.ToInt32(Math.Min(iterations * (brightness / 100), bluePart));

                                    chunkGraphics.FillRectangle(
                                        new SolidBrush(Color.FromArgb(redComponent, greenComponent, blueComponent)), x,
                                        y, 1, 1);
                                }
                            }
                        }
                    }

                    return chunkBitmap;
                });
            }

            Task.WaitAll(tasks);

            using (Graphics formGraphics = CreateGraphics())
            {
                using (Bitmap finalBitmap = new Bitmap(width, height))
                {
                    using (Graphics finalGraphics = Graphics.FromImage(finalBitmap))
                    {
                        for (int i = 0; i < taskCount; i++)
                        {
                            Bitmap chunkBitmap = tasks[i].Result;
                            int startY = i * chunkHeight;
                            finalGraphics.DrawImage(chunkBitmap, 0, startY);
                        }
                    }

                    formGraphics.DrawImage(finalBitmap, 0, 0);
                }
            }
        }

        public void Movement(int m)
        {
            Decimal xRange = (XMax - XMin) / 8;
            Decimal yRange = (YMax - YMin) / 8;

            if (m == 0)
            {
                YMin += yRange;
                YMax += yRange;
            }

            else if (m == 1)
            {
                YMin -= yRange;
                YMax -= yRange;
            }

            else if (m == 2)
            {
                XMin += xRange;
                XMax += xRange;
            }

            else if (m == 3)
            {
                XMin -= xRange;
                XMax -= xRange;
            }

            RenderMandelbrot(this.ClientSize.Width, this.ClientSize.Height);
        }

        public void Zoom(int z)
        {
            Decimal zoomFactor = 1.6m;
            Decimal xRange = XMax - XMin;
            Decimal yRange = YMax - YMin;
            Decimal xMid = XMin + xRange / 2;
            Decimal yMid = YMin + yRange / 2;

            if (z > 0) // Zoom in
            {
                xRange /= zoomFactor;
                yRange /= zoomFactor;
            }
            else // Zoom out
            {
                xRange *= zoomFactor;
                yRange *= zoomFactor;
            }

            //double xOffset = (this.ClientSize.Width / 10) / (double)this.ClientSize.Width * xRange;
            //double yOffset = (this.ClientSize.Height / 10) / (double)this.ClientSize.Height * yRange;

            XMin = xMid - xRange / 2;
            XMax = xMid + xRange / 2;
            YMin = yMid - yRange / 2;
            YMax = yMid + yRange / 2;

            RenderMandelbrot(this.ClientSize.Width, this.ClientSize.Height);
        }
    }
}