using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PixelForge.Tools
{
    public class PaintSettings
    {
        private const string _toolSettingsFile = "toolSettings.json";
        private Dictionary<string, Dictionary<string, object>> _tools;

        public PaintSettings()
        {
            if (File.Exists(_toolSettingsFile))
                LoadSettings();
            else
            {
                _tools = new Dictionary<string, Dictionary<string, object>>();
                AddTool("brush", new Dictionary<string, object>
                {
                    { "size", 10 },
                    { "color", "#FF000000" },
                    { "rotation", 0 },
                });
                AddTool("pen", new Dictionary<string, object>
                {
                    { "size", 10 },
                    { "color", "#FF000000" },
                });
            }
        }

        // Добавить новый инструмент
        public void AddTool(string toolName, Dictionary<string, object> defaultParameters)
        {
            if (!_tools.ContainsKey(toolName))
            {
                _tools.Add(toolName, defaultParameters);
                SaveSettings(); // Сохранить изменения в файл
            }
            else
            {
                throw new ArgumentException($"Tool '{toolName}' already exists.");
            }
        }

        // Добавить новый параметр к инструменту
        public void AddToolParameter(string toolName, string parameterName, object defaultValue)
        {
            if (_tools.ContainsKey(toolName))
            {
                if (!_tools[toolName].ContainsKey(parameterName))
                {
                    _tools[toolName].Add(parameterName, defaultValue);
                    SaveSettings(); // Сохранить изменения в файл
                }
                else
                {
                    throw new ArgumentException($"Parameter '{parameterName}' already exists for tool '{toolName}'.");
                }
            }
            else
            {
                throw new ArgumentException($"Tool '{toolName}' does not exist.");
            }
        }

        // Получить настройки конкретного инструмента
        public Dictionary<string, object> GetToolSettings(string toolName)
        {
            if (_tools.ContainsKey(toolName))
            {
                return _tools[toolName];
            }
            else
            {
                throw new ArgumentException($"Tool '{toolName}' does not exist.");
            }
        }

        // Изменить параметр инструмента
        public void SetToolParameter(string toolName, string parameterName, object value)
        {
            toolName = toolName.ToLower();
            if (_tools.ContainsKey(toolName))
            {
                if (_tools[toolName].ContainsKey(parameterName))
                {
                    _tools[toolName][parameterName] = value;
                    SaveSettings();
                }
                else
                {
                    throw new ArgumentException($"Parameter '{parameterName}' for tool '{toolName}' does not exist.");
                }
            }
            else
            {
                throw new ArgumentException($"Tool '{toolName}' does not exist.");
            }
        }

        // Сохранить настройки в JSON-файл
        private void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(_tools, Formatting.Indented); // Красивый формат JSON
            File.WriteAllText(_toolSettingsFile, json);
        }

        // Загрузить настройки из JSON-файла
        private void LoadSettings()
        {
            string json = File.ReadAllText(_toolSettingsFile);
            _tools = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json)
                     ?? new Dictionary<string, Dictionary<string, object>>();
        }
    }
}
