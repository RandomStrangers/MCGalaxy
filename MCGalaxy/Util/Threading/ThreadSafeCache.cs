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
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
namespace MCGalaxy.Util
{
    public sealed class ThreadSafeCache
    {
        public static ThreadSafeCache DBCache = new();
        public readonly object locker = new();
        public readonly Dictionary<string, object> items = new();
        public readonly Dictionary<string, DateTime> access = new();
        public object GetLocker(string key)
        {
            lock (locker)
            {
                if (!items.TryGetValue(key, out object value))
                {
                    value = new();
                    items[key] = value;
                }
                access[key] = DateTime.UtcNow;
                return value;
            }
        }
        public void CleanupTask(SchedulerTask _)
        {
            List<string> free = null;
            DateTime now = DateTime.UtcNow;
            lock (locker)
            {
                foreach (KeyValuePair<string, DateTime> kvp in access)
                {
                    if ((now - kvp.Value).TotalMinutes <= 5)
                        continue;
                    free ??= new();
                    free.Add(kvp.Key);
                }
                if (free == null)
                    return;
                foreach (string key in free)
                {
                    items.Remove(key);
                    access.Remove(key);
                }
            }
        }
    }
}
