using iText.IO.Image;
using iText.Kernel.Geom;
using SkiaSharp;
using Swan.Logging;

namespace DocvTools
{
    internal class Util
    {
        public static ImageData GenerateSignatureImage(string imgPath)
        {
            SKBitmap bitmap;

            if (File.Exists(imgPath))
            {
                "Найден шаблон бланка для генерации визуального отображения.".Debug();
                bitmap = SKBitmap.Decode(imgPath);
            }
            else
            {
                "Не найден шаблон бланка для генерации визуального отображения.".Debug();
                bitmap = GenerateTemplate();
            }

            SKData encodedData = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return ImageDataFactory.Create(encodedData.ToArray());
        }

        public static ImageData GenerateSignatureImage(string imgPath, string text, string fontName, int fontSize, Offset? offset)
        {

            SKBitmap bitmap;

            if (File.Exists(imgPath))
            {
                "Найден шаблон бланка для генерации визуального отображения.".Debug();
                bitmap = SKBitmap.Decode(imgPath);
            }
            else
            {
                "Не найден шаблон бланка для генерации визуального отображения.".Debug();
                bitmap = GenerateTemplate();
            }


            using (SKCanvas canvas = new(bitmap))
            {
                // 3. Define the text style using an SKPaint object
                using (SKPaint paint = new())
                {
                    paint.Color = SKColors.Black;
                    paint.IsAntialias = true;

                    var typeface = SKTypeface.FromFamilyName(fontName);

                    // 2. Create the SKFont object with size 24
                    var font = new SKFont(typeface, fontSize);

                    if (offset == null) offset = new Offset();

                    float x = offset.X; // Initial X position
                    float y = offset.Y; // Initial Y position
                                        

                    // Get text height
                    float lineHeight = font.Spacing;

                    // 3. Draw each line
                    foreach (string line in text.Split('\n'))
                    {
                        canvas.DrawText(line, x, y, SKTextAlign.Left, font, paint);
                        y += lineHeight; // Move down for the next line
                    }
                }
            }

            SKData encodedData = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return ImageDataFactory.Create(encodedData.ToArray());
        }

        private static SKBitmap GenerateTemplate() {
            SKBitmap bitmap = new SKBitmap(1250, 500);

            using (SKCanvas canvas = new(bitmap))
            {
                // 3. Define the text style using an SKPaint object
                canvas.Clear();
                using (SKPaint paint = new())
                {
                    paint.Color = SKColors.Black;
                    paint.IsAntialias = true;

                    var typeface = SKTypeface.FromFamilyName("Times New Roman", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

                    // 2. Create the SKFont object with size
                    var font = new SKFont(typeface, 58);

                    float x = bitmap.Width / 2;  // Initial X position
                    float y = bitmap.Height / 6; // Initial Y position
                                                 
                    // 3. Draw each line
                    canvas.DrawText("ДОКУМЕНТ ПОДПИСАН", x, y, SKTextAlign.Center, font, paint);
                    canvas.DrawText("ЭЛЕКТРОННОЙ ПОДПИСЬЮ", x, y + font.Spacing, SKTextAlign.Center, font, paint);
                    canvas.DrawText("СВЕДЕНИЯ О СЕРТИФИКАТЕ ЭП", x, y + font.Spacing * 2.5f, SKTextAlign.Center, font, paint);
                    
                    SKRect borderRect = SKRect.Create(8, 8, bitmap.Width-16, bitmap.Height-16);
                    float cornerRadius = 50;

                    paint.Style = SKPaintStyle.Stroke; // Set to stroke for an outline
                    paint.StrokeWidth = 12;
                    canvas.DrawRoundRect(borderRect, cornerRadius, cornerRadius, paint);
                }
            }

            return bitmap;
        }

        public static void CalculateRect(Rectangle sourceRect, float imgRatio, float ratio)
        {
            float centerX = sourceRect.GetX() + sourceRect.GetWidth() / 2;
            float centerY = sourceRect.GetY() + sourceRect.GetHeight() / 2;

            sourceRect.SetWidth(sourceRect.GetWidth() * ratio)
                      .SetX(centerX - sourceRect.GetWidth() / 2)
                      .SetHeight(sourceRect.GetWidth() / imgRatio)
                      .SetY(centerY - sourceRect.GetHeight() / 2);
        }

        public static bool IsByteArrayPdf(byte[] byteArray)
        {
            // PDF файл должен иметь первые 4 байта сигнатуры
            if (byteArray == null || byteArray.Length < 4)
            {
                return false;
            }

            // Проверка первых 4 байт на соответствие "%PDF"
            // % = 0x25
            // P = 0x50
            // D = 0x44
            // F = 0x46
            if (byteArray[0] == 0x25 &&
                byteArray[1] == 0x50 &&
                byteArray[2] == 0x44 &&
                byteArray[3] == 0x46)
            {
                return true;
            }

            return false;
        }
    }
}
