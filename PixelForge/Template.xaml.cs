using System.Windows;

namespace PixelForge
{
    public partial class Template : Window
    {
        private MainWindow _mainWindow;
        private TemplateData _data;
        public Template(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void OpenColorPickerClick(object sender, RoutedEventArgs e)
        {
            ColorPicker picker = new ColorPicker(this, null);
            picker.ShowDialog();
        }

        private void ChangeWidthHeightClick(object sender, RoutedEventArgs e)
        {
            string _ = templateWidth.Text;
            templateWidth.Text = templateHeight.Text;
            templateHeight.Text = _;
        }

        private void CloseTemplateClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool CreateNewTemplate()
        {
            TemplateManager templateManager = new TemplateManager();
            templateTitle.Text = templateTitle.Text.Trim();
            try
            {
                templateManager.CreateTemplate(templateTitle.Text, int.Parse(templateWidth.Text),
                    int.Parse(templateHeight.Text), int.Parse(templateDPI.Text), templateBackground.Background.ToString());
                _mainWindow.UpdatesTemplates();
                _data = new TemplateData{
                    Title = templateTitle.Text,
                    Width = int.Parse(templateWidth.Text),
                    Height = int.Parse(templateHeight.Text),
                    DPI = int.Parse(templateDPI.Text),
                    Background = templateBackground.Background.ToString()
                };
                return true;
            }
            catch
            {
                if (templateTitle.Text.Equals("") || templateTitle.Text == null)
                {
                    templateTitle.Focus();
                }
                else if (!int.TryParse(templateWidth.Text, out _))
                {
                    templateWidth.SelectAll();
                    templateWidth.Focus();
                }
                else if (!int.TryParse(templateHeight.Text, out _))
                {
                    templateHeight.SelectAll();
                    templateHeight.Focus();
                }
                else if (!int.TryParse(templateDPI.Text, out _))
                {
                    templateDPI.SelectAll();
                    templateDPI.Focus();
                }
            }
            return false;
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            if (CreateNewTemplate()) Close();
        }

        private void AddAndStartClick(object sender, RoutedEventArgs e)
        {
            if (CreateNewTemplate())
            {
                if (_data !=  null)
                {
                    Workspace workspace = new Workspace(_data);
                    workspace.Show();
                    _mainWindow.Close();
                    Close();
                }
            }
        }
    }
}
