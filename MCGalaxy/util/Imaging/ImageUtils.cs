/*
    Copyright 2015-2024 MCGalaxy
        
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
using System;
namespace MCGalaxy.Util
{
    public static class ImageUtils
    {
        public static IBitmap2D DecodeImage(byte[] data, Player p)
        {
            IBitmap2D bmp = null;
            try
            {
                bmp = IBitmap2D.Create();
                bmp.Decode(data);
                return bmp;
            }
            catch (ArgumentException ex)
            {
                Logger.Log(LogType.Warning, "Error decoding image: " + ex.Message);
                OnDecodeError(p, bmp);
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error decoding image", ex);
                OnDecodeError(p, bmp);
                return null;
            }
        }
        static void OnDecodeError(Player p, IBitmap2D bmp)
        {
            bmp?.Dispose();
            p.Message("&WThere was an error reading the downloaded image.");
            p.Message("&WThe url may need to end with its extension (such as .jpg).");
        }
    }
}