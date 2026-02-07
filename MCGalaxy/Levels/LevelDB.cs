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
using MCGalaxy.Blocks.Extended;
using MCGalaxy.Maths;
using MCGalaxy.SQL;
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    public static class LevelDB
    {
        internal static void SaveBlockDB(Level lvl)
        {
            if (lvl.BlockDB.Cache.Head != null)
            {
                if (!lvl.Config.UseBlockDB) 
                { 
                    lvl.BlockDB.Cache.Clear(); 
                    return;
                }
                using (IDisposable wLock = lvl.BlockDB.Locker.AccquireWrite(60 * 1000))
                {
                    if (wLock == null)
                    {
                        Logger.Log(6, "&WCouldn't accquire BlockDB write lock on {0}, skipping save", lvl.name);
                        return;
                    }
                    lvl.BlockDB.FlushCache();
                }
                Logger.Log(0, "Saved BlockDB changes for: {0}", lvl.name);
            }
        }
        static Zone ParseZone(ISqlRecord record)
        {
            Zone z = new()
            {
                MinX = (ushort)record.GetInt("SmallX"),
                MinY = (ushort)record.GetInt("SmallY"),
                MinZ = (ushort)record.GetInt("SmallZ"),
                MaxX = (ushort)record.GetInt("BigX"),
                MaxY = (ushort)record.GetInt("BigY"),
                MaxZ = (ushort)record.GetInt("BigZ")
            };
            z.Config.Name = record.GetText("Owner");
            return z;
        }
        internal static void LoadZones(Level level, string map)
        {
            if (Database.TableExists("Zone" + map))
            {
                List<Zone> zones = new();
                Database.ReadRows("Zone" + map, "*",
                                    record => zones.Add(ParseZone(record)));
                bool changedPerbuild = false;
                for (int i = 0; i < zones.Count; i++)
                {
                    Zone z = zones[i];
                    string owner = z.Config.Name;
                    if (owner.StartsWith("grp"))
                    {
                        Group grp = Group.Find(owner.Substring(3));
                        if (grp != null) z.Access.Min = grp.Permission;
                    }
                    else if (z.CoversMap(level))
                    {
                        level.BuildAccess.Whitelisted.Add(owner);
                        changedPerbuild = true;
                        continue;
                    }
                    else
                    {
                        z.Access.Whitelisted.Add(owner);
                        z.Access.Min = 100;
                    }
                    z.Config.Name = "Zone" + i;
                    z.AddTo(level);
                }
                if (changedPerbuild) level.SaveSettings();
                if (level.Zones.Count <= 0 && level.Save(true))
                {
                    Database.DeleteTable("Zone" + map);
                    Logger.Log(1, "Upgraded zones for map " + map);
                }
            }
        }
        internal static void LoadPortals(Level level, string map)
        {
            List<Vec3U16> coords = Portal.GetAllCoords(map);
            level.hasPortals = coords.Count > 0;
            if (level.hasPortals)
            {
                int deleted = 0;
                foreach (Vec3U16 p in coords)
                {
                    if (level.Props[level.GetBlock(p.X, p.Y, p.Z)].IsPortal) continue;
                    Database.Execute(SqlUtils.WithTable("DELETE FROM {table} WHERE EntryX=@0 AND EntryY=@1 AND EntryZ=@2", "Portals" + map), p.X, p.Y, p.Z);
                    deleted++;
                }
                if (deleted != 0)
                {
                    Logger.Log(0, "Autodeleted {0} non-existent portals in {1}", deleted, level.name);
                }
            }
        }
        internal static void LoadMessages(Level level, string map)
        {
            List<Vec3U16> coords = MessageBlock.GetAllCoords(map);
            level.hasMessageBlocks = coords.Count > 0;
            if (level.hasMessageBlocks)
            {
                int deleted = 0;
                foreach (Vec3U16 p in coords)
                {
                    if (level.Props[level.GetBlock(p.X, p.Y, p.Z)].IsMessageBlock) continue;
                    Database.DeleteRows("Messages" + map,
                                    "WHERE X=@0 AND Y=@1 AND Z=@2", p.X, p.Y, p.Z);
                    deleted++;
                }
                if (deleted != 0)
                {
                    Logger.Log(0, "Autodeleted {0} non-existent message blocks in {1}", deleted, level.name);
                }
            }
        }
        internal static ColumnDesc[] createPortals = new ColumnDesc[] {
            new("EntryX", ColumnType.UInt16),
            new("EntryY", ColumnType.UInt16),
            new("EntryZ", ColumnType.UInt16),
            new("ExitMap", ColumnType.Char, 20),
            new("ExitX", ColumnType.UInt16),
            new("ExitY", ColumnType.UInt16),
            new("ExitZ", ColumnType.UInt16),
        };
        internal static ColumnDesc[] createMessages = new ColumnDesc[] {
            new("X", ColumnType.UInt16),
            new("Y", ColumnType.UInt16),
            new("Z", ColumnType.UInt16),
            new("Message", ColumnType.Char, 255),
        };
    }
}
