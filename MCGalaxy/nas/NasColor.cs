#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using System;
using System.Drawing;
using System.IO;
namespace NotAwesomeSurvival
{
    public class DynamicColor
    {
        public static SchedulerTask task;
        public static ColorDesc[] defaultColors,
            fullHealthColors,
            mediumHealthColors,
            lowHealthColors,
            direHealthColors;
        public const string selectorImageName = "selectorColors.png";
        public static void Log(string format, params object[] args)
        {
            Logger.Log(LogType.Debug, string.Format(format, args));
        }
        public static bool Setup()
        {
            if (File.Exists("plugins/" + selectorImageName))
            {
                FileIO.TryMove("plugins/" + selectorImageName, Nas.Path + selectorImageName);
            }
            if (!File.Exists(Nas.Path + selectorImageName))
            {
                Log("Could not locate {0} (needed for tool health/selection colors)", selectorImageName);
                return false;
            }
            Bitmap colorImage;
            colorImage = new(Nas.Path + "selectorColors.png");
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
            colorImage.Dispose();
            task = Server.MainScheduler.QueueRepeat(Update, null, TimeSpan.FromMilliseconds(100));
            return true;
        }
        public static void SetupDescs(int yOffset, Bitmap colorImage, ref ColorDesc[] colorDescs)
        {
            for (int i = 0; i < colorImage.Width; i++)
            {
                Color color = colorImage.GetPixel(i, yOffset);
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
            if (task == null)
            {
                return;
            }
            Server.MainScheduler.Cancel(task);
        }
        public static int index;
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
                NasPlayer np = NasPlayer.GetNasPlayer(p);
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
#endif