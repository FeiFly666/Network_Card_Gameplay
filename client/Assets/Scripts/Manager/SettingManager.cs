using Common;
using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class ClientSettings
    {
        public int width = 1600;
        public int height = 900;
        public FullScreenMode screenMode = FullScreenMode.Windowed;

        public int targetFps = 60;
        public bool vSync = false;

        //public float masterVolume = 1f;
    }
    public class SettingManager : Singleton<SettingManager>
    {
        private string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

        public ClientSettings Settings { get; private set; } = new ClientSettings();
        public void Init()
        {
            Load();
        }
        public void Load()
        {
            if(!File.Exists(SavePath))
            {
                Save();
                Apply();
                return;
            }
            var json = File.ReadAllText(SavePath);
            if (json == null) return;

            Settings = JsonConvert.DeserializeObject<ClientSettings>(json);
            if(Settings == null) Settings = new ClientSettings();
            Apply();
        }
        public void Save()
        {
            var json = JsonConvert.SerializeObject(Settings,Formatting.Indented);

            File.WriteAllText(SavePath, json);
        }
        public void Apply()
        {
            QualitySettings.vSyncCount = Settings.vSync ? 1 : 0;
            Application.targetFrameRate = Settings.vSync ? -1 : Settings.targetFps;

            Screen.SetResolution(Settings.width,Settings.height,Settings.screenMode);

        }
    }
}
