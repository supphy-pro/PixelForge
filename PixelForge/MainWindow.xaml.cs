using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PixelForge.Tools;
namespace PixelForge
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdatesTemplates();
            new PaintSettings();
        }

        public void UpdatesTemplates()
        {
            templatesPanel.Children.Clear();
            var grid = new Grid
            {
                Width = 150,
                Height = 100,
                Margin = new Thickness(0, 10, 10, 0),
                Cursor = Cursors.Hand,
            };
            grid.SetResourceReference(StyleProperty, "NewTemplateStyle");
            grid.MouseLeftButtonUp += OpenTemplateWindow;

            TextBlock sizeTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ADB5")),
                FontSize = 26,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 2, 10),
                Text = "+"
            };

            Border border = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#707070")),
                BorderThickness = new Thickness(1)
            };

            grid.Children.Add(sizeTextBlock);
            grid.Children.Add(border);

            templatesPanel.Children.Add(grid);

            TemplateManager _templateManager = new TemplateManager();

            foreach (var template in _templateManager.Templates)
            {
                grid = new Grid
                {
                    Width = 150,
                    Height = 100,
                    Margin = new Thickness(0, 10, 10, 0),
                    Cursor = Cursors.Hand,
                    Tag = new TemplateData
                    {
                        Title = template.Title,
                        Width = template.Width,
                        Height = template.Height,
                        DPI = template.DPI,
                        Background = template.BackgroundColor
                    }
                };
                grid.SetResourceReference(StyleProperty, "TemplateStyle");
                grid.MouseLeftButtonUp += OpenTemplateWorkspace;

                TextBlock titleTextBlock = new TextBlock
                {
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ADB5")),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 5, 0, 0),
                    Text = template.Title
                };

                FontFamily fontFamily = new FontFamily("Segoe UI");
                FontStyle fontStyle = FontStyles.Normal;
                FontWeight fontWeight = FontWeights.Normal;
                FontStretch fontStretch = FontStretches.Normal;

                if (GetTextWidth(titleTextBlock.Text, fontFamily, 14, fontStyle, fontWeight, fontStretch) > 130)
                {
                    while (GetTextWidth(titleTextBlock.Text, fontFamily, 14, fontStyle, fontWeight, fontStretch) >= 130)
                    {
                        titleTextBlock.Text = titleTextBlock.Text.Substring(0, titleTextBlock.Text.Length - 3);
                    }
                    titleTextBlock.Text += "...";
                }

                sizeTextBlock = new TextBlock
                {
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ADB5")),
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = $"{template.Width}x{template.Height}"
                };

                Rectangle rectangle = new Rectangle
                {
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"{template.BackgroundColor}")),
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Height = 7
                };

                border = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#707070")),
                    BorderThickness = new Thickness(1)
                };

                grid.Children.Add(titleTextBlock);
                grid.Children.Add(sizeTextBlock);
                grid.Children.Add(rectangle);
                grid.Children.Add(border);
                templatesPanel.Children.Add(grid);
            }
            GC.Collect();
        }

        public double GetTextWidth(string text, FontFamily fontFamily, double fontSize, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);

            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return formattedText.Width;
        }

        private void ClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenTemplateWindow(object sender, MouseButtonEventArgs e)
        {
            Template template = new Template(this);
            template.Title = "New Template";
            template.ShowDialog();
        }

        private void OpenTemplateWorkspace(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid && grid.Tag is TemplateData data)
            {
                Workspace workspace = new Workspace(data);
                workspace.Title = "PixelForge | Untitled";
                workspace.Show();
                Close();
            }
        }
    }
}
