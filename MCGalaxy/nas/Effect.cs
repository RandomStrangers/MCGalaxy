#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Config;
using MCGalaxy.Network;
using MCGalaxy.Util.Imaging;
using System.IO;
namespace NotAwesomeSurvival
{
    public class NasEffect
    {
        public const string Path = Nas.Path + "effects/";
        public static Effect breakMeter, breakEarth, breakLeaves;
        public static Effect[] breakEffects = new Effect[(int)NasBlock.Material.Count];
        public static bool Setup()
        {
            breakMeter = new();
            if (!breakMeter.Load("breakmeter"))
            {
                return false;
            }
            breakEarth = new();
            if (!breakEarth.Load("breakdust"))
            {
                return false;
            }
            breakLeaves = new();
            if (!breakLeaves.Load("breakleaf"))
            {
                return false;
            }
            for (int i = 0; i < (int)NasBlock.Material.Count; i++)
            {
                breakEffects[i] = breakEarth;
            }
            breakEffects[(int)NasBlock.Material.Leaves] = breakLeaves;
            return true;
        }
        public class Effect
        {
            public byte ID;
            [ConfigByte("pixelU1", "Effect")]
            public byte pixelU1 = 1;
            [ConfigByte("pixelV1", "Effect")]
            public byte pixelV1 = 0;
            [ConfigByte("pixelU2", "Effect")]
            public byte pixelU2 = 10;
            [ConfigByte("pixelV2", "Effect")]
            public byte pixelV2 = 10;
            [ConfigByte("tintRed", "Effect")]
            public byte tintRed = 255;
            [ConfigByte("tintGreen", "Effect")]
            public byte tintGreen = 255;
            [ConfigByte("tintBlue", "Effect")]
            public byte tintBlue = 255;
            [ConfigByte("frameCount", "Effect")]
            public byte frameCount = 1;
            [ConfigByte("particleCount", "Effect")]
            public byte particleCount = 1;
            [ConfigFloat("pixelSize", "Effect", 8, 0, 127.5f)]
            public float pixelSize = 8;
            [ConfigFloat("sizeVariation", "Effect", 0.0f, 0)]
            public float sizeVariation;
            [ConfigFloat("spread", "Effect", 0.0f, 0)]
            public float spread;
            [ConfigFloat("speed", "Effect", 0.0f)]
            public float speed;
            [ConfigFloat("gravity", "Effect", 0.0f)]
            public float gravity;
            [ConfigFloat("baseLifetime", "Effect", 1.0f, 0)]
            public float baseLifetime = 1.0f;
            [ConfigFloat("lifetimeVariation", "Effect", 0.0f, 0)]
            public float lifetimeVariation;
            [ConfigBool("expireUponTouchingGround", "Effect", true)]
            public bool expireUponTouchingGround = true;
            [ConfigBool("collidesSolid", "Effect", true)]
            public bool collidesSolid = true;
            [ConfigBool("collidesLiquid", "Effect", true)]
            public bool collidesLiquid = true;
            [ConfigBool("collidesLeaves", "Effect", true)]
            public bool collidesLeaves = true;
            [ConfigBool("fullBright", "Effect", true)]
            public bool fullBright = true;
            public float offset;
            public static ConfigElement[] cfg;
            public bool Load(string effectName)
            {
                string fileName = Path + effectName + ".properties",
                    fileNameInPluginsDir = "plugins/" + effectName + ".properties";
                if (File.Exists(fileNameInPluginsDir))
                {
                    FileIO.TryMove(fileNameInPluginsDir, fileName);
                }
                cfg ??= ConfigElement.GetAll(typeof(Effect));
                if (!ConfigElement.ParseFile(cfg, fileName, this))
                {
                    Log("NAS: Could not find required effect file {0}", effectName);
                    return false;
                }
                offset = pixelSize / 32;
                return true;
            }
        }
        public static void Log(string format, params object[] args)
        {
            Logger.Log(LogType.Debug, string.Format(format, args));
        }
        public static void Define(Player p, byte ID, Effect effect, Pixel? color = null, float? lifetime = null)
        {
            byte red, green, blue;
            float baseLifetime;
            if (color != null)
            {
                Pixel realColor = (Pixel)color;
                red = realColor.R;
                green = realColor.G;
                blue = realColor.B;
            }
            else
            {
                red = effect.tintRed;
                green = effect.tintGreen;
                blue = effect.tintBlue;
            }
            if (lifetime != null)
            {
                baseLifetime = (float)lifetime;
            }
            else
            {
                baseLifetime = effect.baseLifetime;
            }
            p.Send(Packet.DefineEffect(
                                        ID,
                                        effect.pixelU1,
                                        effect.pixelV1,
                                        effect.pixelU2,
                                        effect.pixelV2,
                                        red,
                                        green,
                                        blue,
                                        effect.frameCount,
                                        effect.particleCount,
                                        (byte)(effect.pixelSize * 2),
                                        effect.sizeVariation,
                                        effect.spread,
                                        effect.speed,
                                        effect.gravity,
                                        baseLifetime,
                                        effect.lifetimeVariation,
                                        effect.expireUponTouchingGround,
                                        effect.collidesSolid,
                                        effect.collidesLiquid,
                                        effect.collidesLeaves,
                                        effect.fullBright));
        }
        public static void UndefineEffect(Player p, byte ID)
        {
            p.Send(Packet.DefineEffect(ID, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1000, 0, 0,
                                       false, false, false, false, false
                                      ));
        }
        public static void Spawn(Player p, byte ID, Effect effect, float x, float y, float z, float originX, float originY, float originZ)
        {
            if (!p.Supports(CpeExt.CustomParticles))
            {
                return;
            }
            x += 0.5f;
            y += 0.5f;
            z += 0.5f;
            y -= effect.offset;
            originX += 0.5f;
            originY += 0.5f;
            originZ += 0.5f;
            originY -= effect.offset;
            p.Send(Packet.SpawnEffect(ID, x, y, z, originX, originY, originZ));
        }
    }
}
#endif