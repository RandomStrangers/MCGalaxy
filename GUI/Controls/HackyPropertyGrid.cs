/*
    Copyright 2015 MCGalaxy
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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.PropertyGridInternal;
namespace MCGalaxy.Gui
{
    public sealed class HackyPropertyGrid : PropertyGrid
    {
        sealed class HackyPropertiesTab : PropertiesTab
        {
            public override Bitmap Bitmap
            {
                get 
                { 
                    return base.Bitmap ?? new(16, 16); 
                }
            }
        }
        protected override PropertyTab CreatePropertyTab(Type tabType)
        { 
            return new HackyPropertiesTab(); 
        }
    }
}
