using MCGalaxy.Config;
using MCGalaxy.Network;
using MCGalaxy.Util.Imaging;
namespace MCGalaxy
{
    public class NASEffect
    {
        public byte ID;
        [ConfigByte("pixelU1", "NASEffect")]
        public byte pixelU1 = 1;
        [ConfigByte("pixelV1", "NASEffect")]
        public byte pixelV1 = 0;
        [ConfigByte("pixelU2", "NASEffect")]
        public byte pixelU2 = 10;
        [ConfigByte("pixelV2", "NASEffect")]
        public byte pixelV2 = 10;
        [ConfigByte("tintRed", "NASEffect")]
        public byte tintRed = 255;
        [ConfigByte("tintGreen", "NASEffect")]
        public byte tintGreen = 255;
        [ConfigByte("tintBlue", "NASEffect")]
        public byte tintBlue = 255;
        [ConfigByte("frameCount", "NASEffect")]
        public byte frameCount = 1;
        [ConfigByte("particleCount", "NASEffect")]
        public byte particleCount = 1;
        [ConfigFloat("pixelSize", "NASEffect", 8, 0, 127.5f)]
        public float pixelSize = 8;
        [ConfigFloat("sizeVariation", "NASEffect", 0.0f, 0)]
        public float sizeVariation;
        [ConfigFloat("spread", "NASEffect", 0.0f, 0)]
        public float spread;
        [ConfigFloat("speed", "NASEffect", 0.0f)]
        public float speed;
        [ConfigFloat("gravity", "NASEffect", 0.0f)]
        public float gravity;
        [ConfigFloat("baseLifetime", "NASEffect", 1.0f, 0)]
        public float baseLifetime = 1.0f;
        [ConfigFloat("lifetimeVariation", "NASEffect", 0.0f, 0)]
        public float lifetimeVariation;
        [ConfigBool("expireUponTouchingGround", "NASEffect", true)]
        public bool expireUponTouchingGround = true;
        [ConfigBool("collidesSolid", "NASEffect", true)]
        public bool collidesSolid = true;
        [ConfigBool("collidesLiquid", "NASEffect", true)]
        public bool collidesLiquid = true;
        [ConfigBool("collidesLeaves", "NASEffect", true)]
        public bool collidesLeaves = true;
        [ConfigBool("fullBright", "NASEffect", true)]
        public bool fullBright = true;
        public float offset;
        static ConfigElement[] cfg;
        public bool Load(string effectName)
        {
            cfg ??= ConfigElement.GetAll(typeof(NASEffect));
            if (!ConfigElement.ParseFile(cfg, Path + effectName + ".properties", this))
            {
                Logger.Log(LogType.Warning, "NAS: Could not find required effect file {0}", effectName);
                return false;
            }
            offset = pixelSize / 32;
            return true;
        }
        public const string Path = NASPlugin.Path + "Effects/";
        public static NASEffect breakMeter, breakEarth, breakLeaves;
        public static NASEffect[] breakEffects = new NASEffect[(int)NASMaterial.Count];
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
            for (int i = 0; i < (int)NASMaterial.Count; i++)
            {
                breakEffects[i] = breakEarth;
            }
            breakEffects[(int)NASMaterial.Leaves] = breakLeaves;
            return true;
        }
        public static void Define(Player p, byte ID, NASEffect effect, Pixel? color = null, float? lifetime = null)
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
        public static void UndefineEffect(Player p, byte ID) => p.Send(Packet.DefineEffect(ID, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1000, 0, 0,
                                       false, false, false, false, false
                                      ));
        public static void Spawn(Player p, byte ID, NASEffect effect, float x, float y, float z, float originX, float originY, float originZ)
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
