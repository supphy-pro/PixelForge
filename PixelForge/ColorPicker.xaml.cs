using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelForge.Tools;

namespace PixelForge
{
    public partial class ColorPicker : Window
    {
        private Color _selectedColor;
        private WriteableBitmap _colorBitmap;
        private Template _template;
        private Workspace _workspace;
        private SolidColorBrush _newBackground;

        public ColorPicker(Template template, Workspace workspace)
        {
            InitializeComponent();
            GenerateGradient(0);
            _template = template;
            _workspace = workspace;
        }

        private void GenerateGradient(double hue)
        {
            int width = 300;
            int height = 300;
            _colorBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            byte[] pixels = new byte[width * height * 4];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double saturation = (double)x / width;
                    double lightness = 1.0 - (double)y / height;

                    Color color = HslToRgb(hue, saturation, lightness);

                    int index = (y * width + x) * 4;
                    pixels[index] = color.B;
                    pixels[index + 1] = color.G;
                    pixels[index + 2] = color.R;
                    pixels[index + 3] = 255;
                }
            }

            _colorBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
            GradientBrush.ImageSource = _colorBitmap;
        }

        private Color HslToRgb(double h, double s, double l)
        {
            h = h % 360;
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double r = 0, g = 0, b = 0;
            if (0 <= h && h < 60) { r = c; g = x; b = 0; }
            else if (60 <= h && h < 120) { r = x; g = c; b = 0; }
            else if (120 <= h && h < 180) { r = 0; g = c; b = x; }
            else if (180 <= h && h < 240) { r = 0; g = x; b = c; }
            else if (240 <= h && h < 300) { r = x; g = 0; b = c; }
            else if (300 <= h && h < 360) { r = c; g = 0; b = x; }

            byte R = (byte)((r + m) * 255);
            byte G = (byte)((g + m) * 255);
            byte B = (byte)((b + m) * 255);

            return Color.FromRgb(R, G, B);
        }

        private void ColorCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(ColorCanvas);
            SetSelectedColor(pos);
        }

        private void ColorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(ColorCanvas);
                SetSelectedColor(pos);
            }
        }

        private void SetSelectedColor(Point position)
        {
            if (position.X < 0 || position.Y < 0 || position.X >= _colorBitmap.PixelWidth || position.Y >= _colorBitmap.PixelHeight)
                return;

            _selectedColor = GetColorFromBitmap((int)position.X, (int)position.Y);

            SelectionIndicator.Visibility = Visibility.Visible;
            Canvas.SetLeft(SelectionIndicator, position.X - SelectionIndicator.Width / 2);
            Canvas.SetTop(SelectionIndicator, position.Y - SelectionIndicator.Height / 2);

            SelectionIndicator.Fill = new SolidColorBrush(_selectedColor);

            UpdateSelectionIndicatorBorder(_selectedColor);

            UpdateHexInput();

            UpdateExampleColor();
        }

        private void UpdateExampleColor()
        {
            exampleColor.Fill = _newBackground;
        }

        private Color GetColorFromBitmap(int x, int y)
        {
            byte[] pixels = new byte[4];
            _colorBitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);

            return Color.FromRgb(pixels[2], pixels[1], pixels[0]);
        }

        private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GenerateGradient(e.NewValue);
        }

        private void UpdateHexInput()
        {
            HexInput.Text = $"{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
        }

        private void HexInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ColorConverterFromHex(HexInput.Text, out Color color))
            {
                _selectedColor = color;
                SelectionIndicator.Fill = new SolidColorBrush(color);
                UpdateSelectionIndicatorBorder(color);
                UpdateWindowBackground();
                if (exampleColor != null)
                {
                    UpdateExampleColor();
                }
            }
        }

        private void ApplyHexColor(object sender, RoutedEventArgs e)
        {
            if (ColorConverterFromHex(HexInput.Text, out Color color))
            {
                _selectedColor = color;
                UpdateWindowBackground();
                UpdateSelectionIndicatorBorder(color);
            }
        }

        private bool ColorConverterFromHex(string hex, out Color color)
        {
            if (hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, null, out int rgb))
            {
                color = Color.FromRgb((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));
                return true;
            }
            color = Colors.Transparent;
            return false;
        }

        private void UpdateSelectionIndicatorBorder(Color color)
        {
            double brightness = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            SelectionIndicator.Stroke = new SolidColorBrush(brightness > 0.5 ? Colors.Black : Colors.White);
        }

        private void UpdateWindowBackground()
        {
            _newBackground = new SolidColorBrush(Color.FromArgb(_selectedColor.A, _selectedColor.R, _selectedColor.G, _selectedColor.B));
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindowBackground();
            if (_template == null && _workspace != null)
            {
                _workspace.currentPaintColor = _selectedColor;
                _workspace.paintSettings.SetToolParameter(_workspace.selectedMode, "color", _selectedColor.ToString());
                _workspace.BrushColorButton.Background = _newBackground;
            }
            else
                _template.templateBackground.Background = _newBackground;
            Close();
        }

        private void CancelPickerClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
