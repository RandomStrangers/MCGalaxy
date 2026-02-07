using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Util.Imaging;
using System;
using System.IO;
namespace MCGalaxy
{
    public class NASColor
    {
        public static SchedulerTask task, GoldTask;
        public static ColorDesc[] defaultColors,
            fullHealthColors,
            mediumHealthColors,
            lowHealthColors,
            direHealthColors;
        public static int index, GoldIndex;
        public const string selectorImageName = "selectorColors.png";
        static readonly string[] GoldColors = { "ffe649", "e4bf16", "c39800", "a57800", "c39800", "e4bf16", "ffe649" };
        public static void GoldCallback(SchedulerTask task)
        {
            GoldIndex = (GoldIndex + 1) % GoldColors.Length;
            ColorDesc desc = Colors.ParseHex(GoldColors[GoldIndex]);
            desc.Code = '`';
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players)
            {
                p.Session.SendSetTextColor(desc);
            }
        }
        public static bool Setup()
        {
            if (File.Exists("plugins/" + selectorImageName))
            {
                FileIO.TryMove("plugins/" + selectorImageName, NASPlugin.Path + selectorImageName);
            }
            if (!File.Exists(NASPlugin.Path + selectorImageName))
            {
                Logger.Log(15, "Could not locate {0} (needed for tool health/selection colors)", selectorImageName);
                return false;
            }
            byte[] data = File.ReadAllBytes(NASPlugin.Path + "selectorColors.png");
            Bitmap2D colorImage = ImageDecoder.DecodeFrom(data);
            defaultColors = new ColorDesc[colorImage.Width];
            fullHealthColors = new ColorDesc[colorImage.Width];
            mediumHealthColors = new ColorDesc[colorImage.Width];
            lowHealthColors = new ColorDesc[colorImage.Width];
            direHealthColors = new ColorDesc[colorImage.Width];
            int index = 0;
            SetupDescs(index++, colorImage, ref defaultColors);
            SetupDescs(index++, colorImage, ref fullHealthColors);
            SetupDescs(index++, colorImage, ref mediumHealthColors);
            SetupDescs(index++, colorImage, ref lowHealthColors);
            SetupDescs(index++, colorImage, ref direHealthColors);
            task = Server.MainScheduler.QueueRepeat(Update, null, TimeSpan.FromMilliseconds(100));
            GoldTask = Server.MainScheduler.QueueRepeat(GoldCallback, null, TimeSpan.FromMilliseconds(100));
            return true;
        }
        public static void SetupDescs(int yOffset, Bitmap2D colorImage, ref ColorDesc[] colorDescs)
        {
            for (int i = 0; i < colorImage.Width; i++)
            {
                Pixel color = colorImage.Get(i, yOffset);
                colorDescs[i].R = color.R;
                colorDescs[i].G = color.G;
                colorDescs[i].B = color.B;
                colorDescs[i].A = 255;
                colorDescs[i].Code = 'h';
                colorDescs[i].Fallback = 'f';
            }
        }
        public static void TakeDown()
        {
            if (task != null)
            {
                Server.MainScheduler.Cancel(task);
            }
            if (GoldTask != null)
            {
                Server.MainScheduler.Cancel(GoldTask);
            }
        }
        public static void Update(SchedulerTask task)
        {
            index = (index + 1) % defaultColors.Length;
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players)
            {
                if (!p.Supports(CpeExt.TextColors))
                {
                    continue;
                }
                NASPlayer np = NASPlayer.GetPlayer(p);
                if (np == null)
                {
                    continue;
                }
                ColorDesc desc = np.inventory.selectorColors[index];
                np.Send(Packet.SetTextColor(desc));
            }
        }
    }
}
