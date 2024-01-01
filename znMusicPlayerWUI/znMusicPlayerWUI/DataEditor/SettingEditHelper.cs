using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace znMusicPlayerWUI.DataEditor
{
    public static class SettingEditHelper
    {
        public static void EditSetting(JObject eData, DataFolderBase.SettingParams settingParams, object data)
        {
            eData[settingParams.ToString()] = (JToken)data;
        }

        public static async Task EditSetting(DataFolderBase.SettingParams settingParams, object data)
        {
            await Task.Run(() =>
            {
                EditSetting(DataFolderBase.JSettingData, settingParams, data);
            });
        }

        public static object GetSetting(JObject eData, DataFolderBase.SettingParams settingParams)
        {
            return eData[settingParams.ToString()];
        }

        public static async Task<object> GetSetting(DataFolderBase.SettingParams settingParams)
        {
            return await Task.Run(() =>
            {
                return GetSetting(DataFolderBase.JSettingData, settingParams);
            });
        }
    }
}
