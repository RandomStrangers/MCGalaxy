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
using MCGalaxy.Network;
namespace MCGalaxy.Authentication
{
    public class LoginAuthenticator
    {
        public bool Verify(Player p, string mppass)
        {
            foreach (AuthService auth in AuthService.Services)
            {
                if (Authenticate(auth, p, mppass))
                {
                    return true;
                }
            }
            return false;
        }
        static bool Authenticate(AuthService auth, Player p, string mppass)
        {
            string calc = Server.CalcMppass(p.truename, auth.Salt);
            if (!mppass.CaselessEq(calc))
            {
                return false;
            }
            auth.AcceptPlayer(p);
            return true;
        }
        public static bool VerifyLogin(Player p, string mppass)
        {
            LoginAuthenticator auth = new();
            if (auth.Verify(p, mppass))
            {
                return true;
            }
            return !Server.Config.VerifyNames || (IPUtil.IsPrivate(p.IP) && !Server.Config.VerifyLanIPs);
        }
    }
}
