using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PixelForge
{
    public class TemplateManager
    {
        private const string TemplatesFile = "templates.json";

        public List<Template> Templates { get; private set; }

        public TemplateManager()
        {
            Templates = LoadTemplates();
        }

        public void CreateTemplate(string title, int width, int height, int dpi, string backgroundColor)
        {
            var template = new Template
            {
                Title = title,
                Width = width,
                Height = height,
                DPI = dpi,
                BackgroundColor = backgroundColor,
                CreatedAt = DateTime.Now
            };
            Templates.Add(template);
            SaveTemplates();
        }

        private void SaveTemplates()
        {
            var json = JsonConvert.SerializeObject(Templates, Formatting.Indented);
            File.WriteAllText(TemplatesFile, json);
        }

        private List<Template> LoadTemplates()
        {
            if (File.Exists(TemplatesFile))
            {
                var json = File.ReadAllText(TemplatesFile);
                return JsonConvert.DeserializeObject<List<Template>>(json) ?? new List<Template>();
            }

            return new List<Template>();
        }

        public class Template
        {
            public string Title { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int DPI { get; set; }
            public string BackgroundColor { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
