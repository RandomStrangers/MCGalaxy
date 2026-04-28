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
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy
{
    public class NASWayPoint
    {
        public Position Pos;
        public byte Yaw, Pitch;
        public string Name, Level;
    }
    public class NASWayPointList
    {
        public const string Path = NASPlayer.Path + "WayPoints/";
        public List<NASWayPoint> Items = new();
        public NASWayPoint Find(string name)
        {
            foreach (NASWayPoint nwp in Items)
                if (nwp.Name.CaselessEq(name)) return nwp;
            return null;
        }
        public bool Exists(string name) => Find(name) != null;
        public void Create(string name, NASPlayer np)
        {
            NASWayPoint nwp = new();
            Make(nwp, name, np);
            Items.Add(nwp);
            Save(np);
        }
        public void Make(NASWayPoint nwp, string name, NASPlayer np)
        {
            nwp.Pos = np.p.Pos;
            nwp.Name = name;
            nwp.Yaw = np.p.Rot.RotY;
            nwp.Pitch = np.p.Rot.HeadX;
            nwp.Level = np.p.Level.name;
        }
        public void Update(NASWayPoint nwp, NASPlayer np)
        {
            Make(nwp, nwp.Name, np);
            Save(np);
        }
        public void Save(NASPlayer np)
        {
            using StreamWriter w = FileIO.CreateGuarded(Path + np.p.name + ".txt");
            foreach (NASWayPoint nwp in Items)
                w.WriteLine(nwp.Name + ":" + nwp.Level + ":" + nwp.Pos.X + ":" +
                            nwp.Pos.Y + ":" + nwp.Pos.Z + ":" + nwp.Yaw + ":" + nwp.Pitch);
        }
    }
}