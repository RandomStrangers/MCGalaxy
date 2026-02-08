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
namespace MCGalaxy
{
    /// <summary> Stores extra information for/about a player/level/server. </summary>
    /// <example> Storing a "lives" value for a player. </example>
    public sealed class ExtrasCollection
    {
        readonly Dictionary<string, object> dict = new();
        readonly object locker = new();
        public object this[string key]
        {
            get { lock (locker) { return dict[key]; } }
            set { lock (locker) { dict[key] = value; } }
        }
        public void Clear()
        {
            lock (locker)
            {
                dict.Clear();
            }
        }
        public bool Contains(string key)
        {
            lock (locker)
            {
                return dict.ContainsKey(key);
            }
        }
        public bool Remove(string key)
        {
            lock (locker)
            {
                return dict.Remove(key);
            }
        }
        public bool GetBoolean(string key, bool defaultValue = false)
        {
            if (dict.TryGetValue(key, out object value))
            {
                try
                {
                    return Convert.ToBoolean(value);
                }
                catch (Exception)
                {
                }
            }
            return defaultValue;
        }
        public int GetInt(string key, int defaultValue = 0)
        {
            if (dict.TryGetValue(key, out object value))
            {
                try
                {
                    return Convert.ToInt32(value);
                }
                catch (Exception)
                {
                }
            }
            return defaultValue;
        }
        public string GetString(string key, string defaultValue = null)
        {
            if (dict.TryGetValue(key, out object value))
            {
                try
                {
                    return Convert.ToString(value);
                }
                catch (Exception)
                {
                }
            }
            return defaultValue;
        }
    }
}
