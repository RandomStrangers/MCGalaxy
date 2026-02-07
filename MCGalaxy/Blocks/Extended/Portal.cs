/*
    Copyright 2011 MCForge
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
using System.Collections.Generic;
using MCGalaxy.Maths;
using MCGalaxy.SQL;
namespace MCGalaxy.Blocks.Extended
{
    public class PortalExit
    {
        public string Map;
        public ushort X, Y, Z;
    }
    public static class Portal
    {
        public static bool Handle(Player p, ushort x, ushort y, ushort z)
        {
            if (!p.level.hasPortals)
            {
                return false;
            }
            PortalExit exit = Get(p.level.MapName, x, y, z);
            if (exit == null)
            {
                return false;
            }
            Orientation rot = p.Rot;
            if (p.level.name != exit.Map)
            {
                p.summonedMap = exit.Map;
                bool changedMap;
                try
                {
                    changedMap = PlayerActions.ChangeMap(p, exit.Map);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    changedMap = false;
                }
                p.summonedMap = null;
                if (!changedMap)
                {
                    p.Message("Unable to use this portal, as this portal goes to that map.");
                    return true;
                }
                p.BlockUntilLoad(10);
            }
            Position pos = Position.FromFeetBlockCoords(exit.X, exit.Y, exit.Z);
            p.SendPosition(pos, rot);
            return true;
        }
        internal static Vec3U16 ParseCoords(ISqlRecord record) => new()
        {
            X = (ushort)record.GetInt32(0),
            Y = (ushort)record.GetInt32(1),
            Z = (ushort)record.GetInt32(2)
        };
        static PortalExit ParseExit(ISqlRecord record) => new()
        {
            Map = record.GetText(0),
            X = (ushort)record.GetInt32(1),
            Y = (ushort)record.GetInt32(2),
            Z = (ushort)record.GetInt32(3)
        };
        /// <summary> Returns whether a Portals table for the given map exists in the DB. </summary>
        public static bool ExistsInDB(string map) => Database.TableExists("Portals" + map);
        /// <summary> Returns the coordinates for all portals in the given map. </summary>
        public static List<Vec3U16> GetAllCoords(string map)
        {
            List<Vec3U16> coords = new();
            if (!ExistsInDB(map))
            {
                return coords;
            }
            Database.ReadRows("Portals" + map, "EntryX,EntryY,EntryZ",
                              record => coords.Add(ParseCoords(record)));
            return coords;
        }
        /// <summary> Returns the exit details for the given portal in the given map. </summary>
        /// <remarks> Returns null if the given portal does not actually exist. </remarks>
        public static PortalExit Get(string map, ushort x, ushort y, ushort z)
        {
            PortalExit exit = null;
            Database.ReadRows("Portals" + map, "ExitMap,ExitX,ExitY,ExitZ",
                              record => exit = ParseExit(record),
                              "WHERE EntryX=@0 AND EntryY=@1 AND EntryZ=@2", x, y, z);
            return exit;
        }
    }
}
