using System;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace Composer
{
    static class Program
    {
        private static readonly string RootFolder = "../../../";

        static void Main(string[] args)
        {
            System.IO.Directory.CreateDirectory($"{RootFolder}/output");

            var paddingAmount = 10;
            var oneDimensionSize = 1080;
            var totalPaddingInOneDimension = paddingAmount * 3;
            var thumbnailOneDimensionSize = Convert.ToInt32((oneDimensionSize / 2) - paddingAmount * 2);

            using (var outputImage = new Image<Rgba32>(Configuration.Default, oneDimensionSize, oneDimensionSize, Rgba32.DarkBlue))
            {
                using (var topLeftImage = _generateThumbnail($"{RootFolder}/input/ss.jpg", thumbnailOneDimensionSize, "100"))
                {
                    outputImage.Mutate(x => x.DrawImage(topLeftImage, new Point(paddingAmount, paddingAmount), 1f));
                }

                using (var topRightImage = _generateThumbnail($"{RootFolder}/input/f9h.jpg", thumbnailOneDimensionSize, "5"))
                {
                    outputImage.Mutate(x => x.DrawImage(topRightImage, new Point(thumbnailOneDimensionSize + totalPaddingInOneDimension, paddingAmount), 1f));
                }

                using (var bottomLeftImage = _generateThumbnail($"{RootFolder}/input/moon.jpg", thumbnailOneDimensionSize, "25"))
                {
                    outputImage.Mutate(x => x.DrawImage(bottomLeftImage, new Point(paddingAmount, thumbnailOneDimensionSize + totalPaddingInOneDimension), 1f));
                }

                using (var bottomRightImage = _generateThumbnail($"{RootFolder}/input/iss.jpg", thumbnailOneDimensionSize, "40"))
                {
                    outputImage.Mutate(x => x.DrawImage(bottomRightImage, new Point(thumbnailOneDimensionSize + totalPaddingInOneDimension, thumbnailOneDimensionSize + totalPaddingInOneDimension), 1f));
                }

                using (var watermark = Image.Load($"{RootFolder}/input/nasa.png"))
                {
                    outputImage.Mutate(x => x.DrawImage(watermark, new Point(thumbnailOneDimensionSize * 2 - (watermark.Width) + totalPaddingInOneDimension, paddingAmount - 5), 1f));
                }

                outputImage.Save($"{RootFolder}/output/final.png");
            }
        }

        private static Image _generateThumbnail(string path, int thumbnailOneDimensionSize, string price)
        {
            using (var img = Image.Load(path))
            {
                // as generate returns a new IImage make sure we dispose of it
                var thumbnail = img.Clone(x =>
                    x.Crop(new Size(thumbnailOneDimensionSize, thumbnailOneDimensionSize))
                        .ApplyRoundedCorners(15)
                );

                var fonts = new FontCollection();
                var bebasNeueFontFamily = fonts.Install($"{RootFolder}/input/BebasNeue-Regular.ttf");

                var font = new Font(bebasNeueFontFamily, 85);

                using (var priceBg = Image.Load($"{RootFolder}/input/pricebg.png"))
                {
                    thumbnail.Mutate(x => x.DrawImage(priceBg, new Point(0, thumbnailOneDimensionSize - 120), 1f));
                }

                var finalPrice = $"${price}";

                if (finalPrice.Length < 4)
                {
                    for (var i = 0; i <= 4 - finalPrice.Length; i++)
                    {
                        finalPrice = $" {finalPrice}";
                    }
                }

                thumbnail.Mutate(x => x.DrawText(
                    finalPrice,
                    font,
                    Rgba32.White,
                    new Point(15, thumbnailOneDimensionSize - 105))
                );

                return thumbnail;
            }
        }

        private static IImageProcessingContext Crop(this IImageProcessingContext processingContext, Size size)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            });
        }

        private static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
        {
            var size = ctx.GetCurrentSize();
            var corners = BuildCorners(size.Width, size.Height, cornerRadius);

            var graphicOptions = new GraphicsOptions(true)
            {
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
            };

            return ctx.Fill(graphicOptions, Rgba32.LimeGreen, corners);
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            var cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the original around the center of the image

            var rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            var bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            // move it across the width of the image - the width of the shape
            var cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            var cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            var cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
    }
}