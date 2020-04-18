using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace NugetPackageManager
{
    public class SettingManager
    {
        private static Setting _setting;
        public static readonly string CurrentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static readonly string SettingFilePath = Path.Combine(CurrentFolder, "Setting.json");

        public static Setting Setting
        {
            get
            {
                if(_setting == null)
                {                    
                    if(File.Exists(SettingFilePath))
                    {
                        _setting =(Setting) JsonConvert.DeserializeObject(File.ReadAllText(SettingFilePath), typeof(Setting));
                    }                    
                }

                return _setting;
            }
        }

        public static void SaveSetting(Setting setting)
        {
            string content = JsonConvert.SerializeObject(setting, Formatting.Indented);

            File.WriteAllText(SettingFilePath, content);

            _setting = setting;
        }
    }
}
