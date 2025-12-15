using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gamma_Manager
{
    public class Preset
    {
        public string Name { get; set; }
        public float RGamma { get; set; }
        public float GGamma { get; set; }
        public float BGamma { get; set; }
        public float RContrast { get; set; }
        public float GContrast { get; set; }
        public float BContrast { get; set; }
        public float RBright { get; set; }
        public float GBright { get; set; }
        public float BBright { get; set; }

        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"Name\":\"{0}\",", Name);
            sb.AppendFormat("\"RGamma\":{0},", RGamma.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendFormat("\"GGamma\":{0},", GGamma.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendFormat("\"BGamma\":{0},", BGamma.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendFormat("\"RContrast\":{0},", RContrast.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendFormat("\"GContrast\":{0},", GContrast.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendFormat("\"BContrast\":{0},", BContrast.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendFormat("\"RBright\":{0},", RBright.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendFormat("\"GBright\":{0},", GBright.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendFormat("\"BBright\":{0}", BBright.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.Append("}");
            return sb.ToString();
        }

        public static Preset FromJson(string json)
        {
            Preset preset = new Preset();
            // Simple JSON parsing without external library
            json = json.Trim().Trim('{', '}');
            string[] pairs = json.Split(',');
            foreach (string pair in pairs)
            {
                string[] kv = pair.Split(':');
                if (kv.Length != 2) continue;
                string key = kv[0].Trim().Trim('"');
                string value = kv[1].Trim().Trim('"');

                switch (key)
                {
                    case "Name": preset.Name = value; break;
                    case "RGamma": preset.RGamma = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "GGamma": preset.GGamma = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "BGamma": preset.BGamma = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "RContrast": preset.RContrast = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "GContrast": preset.GContrast = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "BContrast": preset.BContrast = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "RBright": preset.RBright = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "GBright": preset.GBright = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "BBright": preset.BBright = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture); break;
                }
            }
            return preset;
        }
    }

    public class PresetManager
    {
        private static string PresetDirectory
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dir = Path.Combine(appData, "GammaManager", "Presets");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

        public static void SavePreset(string name, Preset preset)
        {
            preset.Name = name;
            string filePath = Path.Combine(PresetDirectory, name + ".json");
            string json = preset.ToJson();
            File.WriteAllText(filePath, json);
        }

        public static Preset LoadPreset(string name)
        {
            string filePath = Path.Combine(PresetDirectory, name + ".json");
            if (!File.Exists(filePath))
            {
                return null;
            }
            string json = File.ReadAllText(filePath);
            return Preset.FromJson(json);
        }

        public static List<string> GetPresetList()
        {
            List<string> presets = new List<string>();
            string[] files = Directory.GetFiles(PresetDirectory, "*.json");
            foreach (string file in files)
            {
                presets.Add(Path.GetFileNameWithoutExtension(file));
            }
            return presets;
        }

        public static void DeletePreset(string name)
        {
            string filePath = Path.Combine(PresetDirectory, name + ".json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
