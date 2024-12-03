using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelForge.Tools;

namespace PixelForge
{
    public partial class Workspace : Window
    {
        private readonly Dictionary<int, string> _modes = new Dictionary<int, string>()
        {
            { 0, "SELECT"},
            { 1, "BRUSH"},
            { 2, "PEN"},
            { 3, ""},
            { 4, ""},
            { 5, ""},
            { 6, ""},
            { 7, ""},
        };

        private double _currentScale = 1.0;
        private TemplateData _data;

        private WriteableBitmap _bitmap; // Растровое изображение
        private bool _isDrawing = false; // Флаг рисования

        public string selectedMode;

        private Point _lastPaintedPixel = new Point(-1, -1);
        public PaintSettings paintSettings = new PaintSettings();
        public Color currentPaintColor; // Текущий цвет кисти
        private int _currentPaintRotation; // Текущий поворот кисти
        private int _currentPaintSize; // Текущий размер кисти
        public Workspace(TemplateData data)
        {
            InitializeComponent();

            selectedMode = _modes[1];

            var modeSettings = paintSettings.GetToolSettings(selectedMode.ToLower());
            currentPaintColor = (Color)ColorConverter.ConvertFromString(modeSettings["color"].ToString());
            _currentPaintSize = int.Parse(modeSettings["size"].ToString());
            
            _data = data;
            EditorCanvas.Width = _data.Width;
            EditorCanvas.Height = _data.Height;
            sizeText.Text = $"Size: {_data.Width}x{_data.Height} |";
            UpdateScaleText();
            CreatePixelGrid(_data.Width, _data.Height);

            BrushRotationTextBox.Text = _currentPaintRotation.ToString();
            BrushSizeTextBox.Text = _currentPaintSize.ToString();
            BrushRotationSlider.Value = _currentPaintRotation;
            BrushSizeSlider.Value = _currentPaintSize;
            BrushColorButton.Background = new SolidColorBrush(currentPaintColor);
        }

        // Создание сетки пикселей
        private void CreatePixelGrid(int width, int height)
        {
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            EditorCanvas.Source = _bitmap;

            // Заливка цветом
            ClearCanvas((Color)ColorConverter.ConvertFromString(_data.Background));
        }

        // Очистка холста
        private void ClearCanvas(Color color)
        {
            byte[] pixels = new byte[_bitmap.PixelWidth * _bitmap.PixelHeight * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = color.B;     // Blue
                pixels[i + 1] = color.G; // Green
                pixels[i + 2] = color.R; // Red
                pixels[i + 3] = 255;     // Alpha
            }

            _bitmap.WritePixels(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight), pixels, _bitmap.BackBufferStride, 0);
        }

        private void UpdateScaleText()
        {
            if (_currentScale > 10)
            {
                scaleText.Text = $"Scale: {(_currentScale * 100).ToString().Substring(0, 4)}%";
            }
            else if (_currentScale >= 1)
            {
                scaleText.Text = $"Scale: {(_currentScale * 100).ToString().Substring(0, 3)}%";
            }
            else if (_currentScale >= 0.1)
            {
                try
                {
                    scaleText.Text = $"Scale: {(_currentScale * 100).ToString().Substring(0, 4)}%";
                }
                catch
                {
                    scaleText.Text = $"Scale: {(_currentScale * 100).ToString().Substring(0, 3)}%";
                }
            }
            else
            {
                scaleText.Text = $"Scale: {(_currentScale * 100).ToString().Substring(0, 3)}%";
            }
        }
        private void MainCanvasScrolling(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                if (e.Delta > 0 && _currentScale <= 30)
                {
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        _currentScale *= 1.01; // Увеличить масштаб
                    }
                    else
                    {
                        _currentScale *= 1.1; // Увеличить масштаб
                    }
                }
                else if (e.Delta < 0 && _currentScale >= 0.1)
                {
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        _currentScale /= 1.01; // Уменьшить масштаб
                    }
                    else
                    {
                        _currentScale /= 1.1; // Уменьшить масштаб
                    }
                }
                ScaleTransform scale = new ScaleTransform(_currentScale, _currentScale);
                EditorCanvas.LayoutTransform = scale;
                UpdateScaleText();
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - e.Delta / 9.0);
                }
                else
                {
                    MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - e.Delta / 3.0);
                }
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset - e.Delta / 9.0);
                }
                else
                {
                    MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset - e.Delta / 3.0);
                }
            }
        }

        private void EditorCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            PaintPixel(e.GetPosition(EditorCanvas));
        }

        private void EditorCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
        }

        private void PixelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                PaintPixel(e.GetPosition(EditorCanvas));
            }
        }

        // Рисование пикселя
        private void PaintPixel(Point position)
        {
            if (selectedMode == "BRUSH")
            {
                int radius = _currentPaintSize / 2; // Радиус кисти

                // Центр кисти
                int centerX = (int)position.X;
                int centerY = (int)position.Y;

                if (_lastPaintedPixel.X == centerX && _lastPaintedPixel.Y == centerY)
                {
                    return; // Если пиксель тот же, пропускаем рисование
                }

                // Ограничиваем область рисования
                int startX = Math.Max(centerX - radius, 0);
                int startY = Math.Max(centerY - radius, 0);
                int endX = Math.Min(centerX + radius, _bitmap.PixelWidth - 1);
                int endY = Math.Min(centerY + radius, _bitmap.PixelHeight - 1);

                // Буфер для чтения пикселей
                int stride = _bitmap.PixelWidth * 4;
                byte[] pixels = new byte[_bitmap.PixelHeight * stride];
                _bitmap.CopyPixels(pixels, stride, 0);

                // Рисуем круг
                for (int y = startY; y <= endY; y++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        double dx = x - centerX;
                        double dy = y - centerY;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= radius)
                        {
                            // Вычисляем прозрачность (градиент)
                            double alphaFactor = 1.0 - (distance / radius);
                            byte brushAlpha = (byte)(currentPaintColor.A * alphaFactor);

                            // Если прозрачность равна 0, пропускаем
                            if (brushAlpha == 0)
                                continue;

                            int pixelIndex = (y * stride) + (x * 4);

                            // Смешиваем цвета кисти и фона
                            byte bgB = pixels[pixelIndex];
                            byte bgG = pixels[pixelIndex + 1];
                            byte bgR = pixels[pixelIndex + 2];
                            byte bgA = pixels[pixelIndex + 3];

                            byte blendedR = (byte)((currentPaintColor.R * brushAlpha + bgR * (255 - brushAlpha)) / 255);
                            byte blendedG = (byte)((currentPaintColor.G * brushAlpha + bgG * (255 - brushAlpha)) / 255);
                            byte blendedB = (byte)((currentPaintColor.B * brushAlpha + bgB * (255 - brushAlpha)) / 255);
                            byte blendedA = (byte)(brushAlpha + bgA * (255 - brushAlpha) / 255);

                            // Записываем результат обратно
                            pixels[pixelIndex] = blendedB;
                            pixels[pixelIndex + 1] = blendedG;
                            pixels[pixelIndex + 2] = blendedR;
                            pixels[pixelIndex + 3] = blendedA;
                        }
                    }
                }

                // Обновляем изображение
                _bitmap.WritePixels(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight), pixels, stride, 0);
                _lastPaintedPixel = new Point(centerX, centerY);
            }
            else if (selectedMode == "PEN")
            {
                int x = (int)position.X - _currentPaintSize / 2;
                int y = (int)position.Y - _currentPaintSize / 2;

                // Проверяем, чтобы координаты оставались в пределах изображения
                if (x >= 0 && y >= 0 && x + _currentPaintSize <= _bitmap.PixelWidth && y + _currentPaintSize <= _bitmap.PixelHeight)
                {
                    // Создаём массив данных для всех пикселей в области (ARGB для каждого пикселя)
                    int bytesPerPixel = 4;
                    int stride = _currentPaintSize * bytesPerPixel; // Количество байтов в одной строке пикселей
                    byte[] colorData = new byte[_currentPaintSize * stride];

                    // Заполняем массив данными цвета
                    for (int i = 0; i < colorData.Length; i += bytesPerPixel)
                    {
                        colorData[i] = currentPaintColor.B; // Blue
                        colorData[i + 1] = currentPaintColor.G; // Green
                        colorData[i + 2] = currentPaintColor.R; // Red
                        colorData[i + 3] = currentPaintColor.A; // Alpha
                    }

                    // Указываем прямоугольник для заливки
                    var rect = new Int32Rect(x, y, _currentPaintSize, _currentPaintSize);
                    _bitmap.WritePixels(rect, colorData, stride, 0);
                }
            }
        }

        private void OpenCanvasContextMenu(object sender, MouseButtonEventArgs e)
        {
            BrushSettingsPopup.HorizontalOffset = 5;
            BrushSettingsPopup.VerticalOffset = -3;
            BrushSettingsPopup.IsOpen = true;
        }

        private void BrushSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обновляем значение размера кисти
            if (BrushSizeTextBox != null)
            {
                BrushSizeTextBox.Text = ((int)((Slider)sender).Value).ToString();
            }
        }

        private void BrushRotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обновляем значение угла поворота кисти
            if (BrushSizeTextBox != null)
            {
                BrushRotationTextBox.Text = ((int)((Slider)sender).Value).ToString();
            }
        }

        private void BrushSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(BrushSizeTextBox.Text, out _))
            {
                _currentPaintSize = int.Parse(BrushSizeTextBox.Text);
                BrushSizeSlider.Value = _currentPaintSize;
                paintSettings.SetToolParameter(selectedMode, "size", int.Parse(BrushSizeTextBox.Text));
            }
            else
                BrushSizeTextBox.Text = _currentPaintSize.ToString();
        }

        private void BrushRotationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(BrushRotationTextBox.Text, out _))
            {
                _currentPaintRotation = int.Parse(BrushRotationTextBox.Text);
                BrushRotationSlider.Value = _currentPaintRotation;
                paintSettings.SetToolParameter(selectedMode, "rotation", int.Parse(BrushRotationTextBox.Text));
            }
            else
                BrushSizeTextBox.Text = _currentPaintRotation.ToString();
        }

        private void BrushColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker picker = new ColorPicker(null, this);
            picker.ShowDialog();
        }
    }
}
