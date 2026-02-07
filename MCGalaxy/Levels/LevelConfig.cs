/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using MCGalaxy.Config;
using MCGalaxy.Modules.Games.ZS;
using System;
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy
{
    class ConfigEnvIntAttribute : ConfigIntegerAttribute
    {
        readonly int minValue, maxValue;
        public ConfigEnvIntAttribute(string name, int min, int max)
            : base(name, "Env") { minValue = min; maxValue = max; }
        public override object Parse(string value)
        {
            if (value == "-1.0") return -1;
            // "-1" was used in past as value for "ENV_USE_DEFAULT", so must keep
            //  doing that for backwards compatibility
            // (would have been better to use "" for default, but too late now)
            int num = ParseInteger(value, -1, minValue, maxValue);
            if (num == -1) num = int.MaxValue;
            return num;
        }
        public override string Serialise(object value)
        {
            int num = (int)value;
            // -1 is already used for "use default", so use this instead
            if (num == -1) return "-1.0";
            if (num == int.MaxValue) num = -1;
            return NumberUtils.StringifyInt(num);
        }
    }
    // Hacky workaround for old ExponentialFog attribute which was a bool
    class ConfigExpFogAttribute : ConfigEnvIntAttribute
    {
        public ConfigExpFogAttribute(string name) : base(name, -1, 1) { }
        public override object Parse(string raw)
        {
            if (bool.TryParse(raw, out bool value)) return value ? 1 : 0;
            return base.Parse(raw);
        }
    }
    public abstract class EnvConfig
    {
        // Environment settings
        [ConfigEnvInt("Weather", -1, 2)]
        public int Weather = int.MaxValue;
        /// <summary> Elevation of the "ocean" that surrounds maps. Default is map height / 2. </summary>
        [ConfigEnvInt("EdgeLevel", -0xFFFFFF, 0xFFFFFF)]
        public int EdgeLevel = int.MaxValue;
        /// <summary> Offset of the "bedrock" that surrounds map sides from edge level. Default is -2. </summary>
        [ConfigEnvInt("SidesOffset", -0xFFFFFF, 0xFFFFFF)]
        public int SidesOffset = int.MaxValue;
        /// <summary> Elevation of the clouds. Default is map height + 2. </summary>
        [ConfigEnvInt("CloudsHeight", -0xFFFFFF, 0xFFFFFF)]
        public int CloudsHeight = int.MaxValue;
        /// <summary> Max fog distance the client can see. Default is 0, means use client-side defined max fog distance. </summary>
        [ConfigEnvInt("MaxFog", -0xFFFFFF, 0xFFFFFF)]
        public int MaxFogDistance = int.MaxValue;
        /// <summary> Clouds speed, in units of 256ths. Default is 256 (1 speed). </summary>
        [ConfigEnvInt("clouds-speed", -0xFFFFFF, 0xFFFFFF)]
        public int CloudsSpeed = int.MaxValue;
        /// <summary> Weather speed, in units of 256ths. Default is 256 (1 speed). </summary>
        [ConfigEnvInt("weather-speed", -0xFFFFFF, 0xFFFFFF)]
        public int WeatherSpeed = int.MaxValue;
        /// <summary> Weather fade, in units of 256ths. Default is 256 (1 speed). </summary>
        [ConfigEnvInt("weather-fade", -0xFFFFFF, 0xFFFFFF)]
        public int WeatherFade = int.MaxValue;
        /// <summary> Skybox horizontal speed, in units of 1024ths. Default is 0 (0 speed). </summary>
        [ConfigEnvInt("skybox-hor-speed", -0xFFFFFF, 0xFFFFFF)]
        public int SkyboxHorSpeed = int.MaxValue;
        /// <summary> Skybox vertical speed, in units of 1024ths. Default is 0 (0 speed). </summary>
        [ConfigEnvInt("skybox-ver-speed", -0xFFFFFF, 0xFFFFFF)]
        public int SkyboxVerSpeed = int.MaxValue;
        /// <summary> The block which will be displayed on the horizon. </summary>
        [ConfigBlock("HorizonBlock", "Env", 0xff)]
        public ushort HorizonBlock = 0xff;
        /// <summary> The block which will be displayed on the edge of the map. </summary>
        [ConfigBlock("EdgeBlock", "Env", 0xff)]
        public ushort EdgeBlock = 0xff;
        /// <summary> Whether exponential fog mode is used client-side. </summary>
        [ConfigExpFog("ExpFog")]
        public int ExpFog = int.MaxValue;
        /// <summary> Color of the clouds (Hex RGB color). Set to "" to use client defaults. </summary>
        [ConfigString("CloudColor", "Env", "", true)]
        public string CloudColor = "";
        /// <summary> Color of the fog (Hex RGB color). Set to "" to use client defaults. </summary>
        [ConfigString("FogColor", "Env", "", true)]
        public string FogColor = "";
        /// <summary> Color of the sky (Hex RGB color). Set to "" to use client defaults. </summary>
        [ConfigString("SkyColor", "Env", "", true)]
        public string SkyColor = "";
        /// <summary> Color of the blocks in shadows (Hex RGB color). Set to "" to use client defaults. </summary>
        [ConfigString("ShadowColor", "Env", "", true)]
        public string ShadowColor = "";
        /// <summary> Color of the blocks in the light (Hex RGB color). Set to "" to use client defaults. </summary>
        [ConfigString("LightColor", "Env", "", true)]
        public string LightColor = "";
        /// <summary> Color of the skybox (Hex RGB color). Set to "" to use client defaults. </summary>
        [ConfigString("SkyboxColor", "Env", "", true)]
        public string SkyboxColor = "";
        /// <summary> Color emitted by bright natural blocks (Hex RGB color). Set to "" to use client defaults. </summary>
        [ConfigString("LavaLightColor", "Env", "", true)]
        public string LavaLightColor = "";
        /// <summary> Color emitted by bright artificial blocks (Hex RGB color). Set to "" to use client defaults. </summary>
        [ConfigString("LampLightColor", "Env", "", true)]
        public string LampLightColor = "";
        [ConfigInt("LightingMode", "Env", 0, 0,2)]
        public int LightingMode;
        [ConfigBool("LightingModeLocked", "Env", false)]
        public bool LightingModeLocked = false;
        public void ResetEnv()
        {
            // TODO: Rewrite using ConfigElement somehow
            Weather = int.MaxValue;
            EdgeLevel = int.MaxValue;
            SidesOffset = int.MaxValue;
            CloudsHeight = int.MaxValue;
            MaxFogDistance = int.MaxValue;
            CloudsSpeed = int.MaxValue;
            WeatherSpeed = int.MaxValue;
            WeatherFade = int.MaxValue;
            SkyboxHorSpeed = int.MaxValue;
            SkyboxVerSpeed = int.MaxValue;
            HorizonBlock = 0xff;
            EdgeBlock = 0xff;
            ExpFog = int.MaxValue;
            CloudColor = "";
            FogColor = "";
            SkyColor = "";
            ShadowColor = "";
            LightColor = "";
            SkyboxColor = "";
            LavaLightColor = "";
            LampLightColor = "";
            LightingMode = 0;
            LightingModeLocked = false;
        }
        public string GetColor(int i)
        {
            if (i == 0) return SkyColor;
            if (i == 1) return CloudColor;
            if (i == 2) return FogColor;
            if (i == 3) return ShadowColor;
            if (i == 4) return LightColor;
            if (i == 5) return SkyboxColor;
            if (i == 6) return LavaLightColor;
            if (i == 7) return LampLightColor;
            return null;
        }
        public int GetEnvProp(int i)
        {
            if (i == 0) return EdgeBlock;
            if (i == 1) return HorizonBlock;
            if (i == 2) return EdgeLevel;
            if (i == 3) return CloudsHeight;
            if (i == 4) return MaxFogDistance;
            if (i == 5) return CloudsSpeed;
            if (i == 6) return WeatherSpeed;
            if (i == 7) return WeatherFade;
            if (i == 8) return ExpFog;
            if (i == 9) return SidesOffset;
            if (i == 10) return SkyboxHorSpeed;
            if (i == 11) return SkyboxVerSpeed;
            if (i == 255) return Weather;
            return int.MaxValue;
        }
        /// <summary> Calculates the default value for the given env property </summary>
        public static int DefaultEnvProp(int i, int height)
        {
            if (i == 0) return 7;
            if (i == 1) return 8;
            if (i == 2) return height / 2;
            if (i == 3) return height + 2;
            if (i == 5) return 256;
            if (i == 6) return 256;
            if (i == 7) return 128;
            if (i == 9) return -2;
            return 0;
        }
    }
    public abstract class AreaConfig : EnvConfig
    {
        [ConfigString("MOTD", "General", "ignore", true)]
        public string MOTD = "ignore";
        // Permission settings
        [ConfigBool("Buildable", "Permissions", true)]
        public bool Buildable = true;
        [ConfigBool("Deletable", "Permissions", true)]
        public bool Deletable = true;
        [ConfigPerm("PerBuild", "Permissions", 0)]
        public sbyte BuildMin = 0;
        [ConfigPerm("PerBuildMax", "Permissions", 120)]
        public sbyte BuildMax = 120;
        // Other blacklists/whitelists
        [ConfigStringList("BuildWhitelist", "Permissions")]
        public List<string> BuildWhitelist = new();
        [ConfigStringList("BuildBlacklist", "Permissions")]
        public List<string> BuildBlacklist = new();
    }
    public sealed class LevelConfig : AreaConfig
    {
        [ConfigBool("LoadOnGoto", "General", true)]
        public bool LoadOnGoto = true;
        [ConfigString("Theme", "General", "Normal", true)]
        public string Theme = "Normal";
        [ConfigString("Seed", "General", "", true)]
        public string Seed = "";
        [ConfigBool("Unload", "General", true)]
        public bool AutoUnload = true;
        /// <summary> true if this map may see server-wide chat, false if this map has level-only/isolated chat </summary>
        [ConfigBool("WorldChat", "General", true)]
        public bool ServerWideChat = true;
        [ConfigBool("UseBlockDB", "Other", true)]
        public bool UseBlockDB = true;
        [ConfigInt("LoadDelay", "Other", 0, 0, 2000)]
        public int LoadDelay = 0;
        [ConfigString("Texture", "Env", "", true)]
        public string Terrain = "";
        [ConfigString("TexturePack", "Env", "", true)]
        public string TexturePack = "";
        // Permission settings
        [ConfigString("RealmOwner", "Permissions", "", true)]
        public string RealmOwner = "";
        [ConfigPerm("PerVisit", "Permissions", 0)]
        public sbyte VisitMin = 0;
        [ConfigPerm("PerVisitMax", "Permissions", 120)]
        public sbyte VisitMax = 120;
        // Other blacklists/whitelists
        [ConfigStringList("VisitWhitelist", "Permissions")]
        public List<string> VisitWhitelist = new();
        [ConfigStringList("VisitBlacklist", "Permissions")]
        public List<string> VisitBlacklist = new();
        // Physics settings
        [ConfigInt("Physics", "Physics", 0, 0, 5)]
        public int Physics;
        [ConfigInt("Physics overload", "Physics", 1500)]
        public int PhysicsOverload = 1500;
        [ConfigInt("Physics speed", "Physics", 250)]
        public int PhysicsSpeed = 250;
        [ConfigBool("RandomFlow", "Physics", true)]
        public bool RandomFlow = true;
        [ConfigBool("LeafDecay", "Physics", false)]
        public bool LeafDecay;
        [ConfigBool("Finite mode", "Physics", false)]
        public bool FiniteLiquids;
        [ConfigBool("GrowTrees", "Physics", false)]
        public bool GrowTrees;
        [ConfigBool("Animal AI", "Physics", true)]
        public bool AnimalHuntAI = true;
        [ConfigBool("GrassGrowth", "Physics", true)]
        public bool GrassGrow = true;
        [ConfigString("TreeType", "Physics", "fern", false)]
        public string TreeType = "fern";
        // Survival settings
        [ConfigInt("Drown", "Survival", 70)]
        public int DrownTime = 70;
        [ConfigBool("Edge water", "Survival", false)]
        public bool EdgeWater;
        [ConfigInt("Fall", "Survival", 9)]
        public int FallHeight = 9;
        [ConfigBool("Guns", "Survival", false)]
        public bool Guns;
        [ConfigBool("Survival death", "Survival", false)]
        public bool SurvivalDeath;
        [ConfigBool("Killer blocks", "Survival", true)]
        public bool KillerBlocks = true;
        // Games settings
        [ConfigInt("Likes", "Game", 0)]
        public int Likes;
        [ConfigInt("Dislikes", "Game", 0)]
        public int Dislikes;
        [ConfigString("Authors", "Game", "", true)]
        public string Authors = "";
        [ConfigBool("Pillaring", "Game", false)]
        public bool Pillaring = !ZSGame.Instance.Config.NoPillaring;
        [ConfigInt("BuildType", "Game", 0, 0, 2)]
        public int BuildType = 0;
        [ConfigTimespan("MinRoundTime", "Game", 4, true)]
        public TimeSpan RoundTime = TimeSpan.FromMinutes(5);
        [ConfigBool("DrawingAllowed", "Game", true)]
        public bool Drawing = true;
        [ConfigInt("RoundsPlayed", "Game", 0)]
        public int RoundsPlayed = 0;
        [ConfigInt("RoundsHumanWon", "Game", 0)]
        public int RoundsHumanWon = 0;
        readonly object saveLock = new();
        public string Color
        {
            get
            {
                sbyte maxPerm = VisitMin;
                if (maxPerm < BuildMin) maxPerm = BuildMin;
                return Group.GetColor(maxPerm);
            }
        }
        public bool Load(string path) => ConfigElement.ParseFile(Server.levelConfig, path, this);
        public void SaveFor(string map) => Save(LevelInfo.PropsPath(map), map);
        public void Save(string path, string map)
        {
            try
            {
                lock (saveLock)
                {
                    using StreamWriter w = FileIO.CreateGuarded(path);
                    w.WriteLine("#Level properties for " + map);
                    w.WriteLine("#Drown-time is in tenths of a second");
                    ConfigElement.Serialise(Server.levelConfig, w, this);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving level properties for " + map, ex);
            }
        }
    }
}
