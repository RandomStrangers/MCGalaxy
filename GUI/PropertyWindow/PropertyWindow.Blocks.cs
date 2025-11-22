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
using MCGalaxy.Blocks;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
namespace MCGalaxy.Gui
{
    public partial class PropertyWindow : Form
    {
        ushort curBlock;
        List<ushort> blockIDMap;
        readonly ItemPermsHelper blockItems = new();
        BlockPerms placePermsOrig, placePermsCopy;
        readonly List<BlockPerms> placePermsChanged = new();
        readonly BlockProps[] blockPropsChanged = new BlockProps[Block.Props.Length];
        void LoadBlocks()
        {
            blk_list.Items.Clear();
            placePermsChanged.Clear();
            blockIDMap = new();
            for (int b = 0; b < blockPropsChanged.Length; b++)
            {
                blockPropsChanged[b] = Block.Props[b];
                blockPropsChanged[b].ChangedScope = 0;
                ushort block = (ushort)b;
                if (!Block.ExistsGlobal(block))
                {
                    continue;
                }
                string name = Block.GetName(Player.Console, block);
                blk_list.Items.Add(name);
                blockIDMap.Add(block);
            }
            blockItems.GetCurPerms = BlockGetOrAddPermsChanged;
            if (blk_list.SelectedIndex == -1)
            {
                blk_list.SelectedIndex = 0;
            }
        }
        void SaveBlocks()
        {
            if (placePermsChanged.Count > 0)
            {
                SaveBlockPermissions();
            }
            if (AnyBlockPropsChanged())
            {
                SaveBlockProps();
            }
            LoadBlocks();
        }
        void SaveBlockPermissions()
        {
            foreach (BlockPerms changed in placePermsChanged)
            {
                BlockPerms pOrig = BlockPerms.GetPlace(changed.ID);
                changed.CopyPermissionsTo(pOrig);
                BlockPerms dOrig = BlockPerms.GetDelete(changed.ID);
                changed.CopyPermissionsTo(dOrig);
            }
            BlockPerms.Save();
            BlockPerms.ApplyChanges();
            BlockPerms.ResendAllBlockPermissions();
        }
        bool AnyBlockPropsChanged()
        {
            for (int b = 0; b < blockPropsChanged.Length; b++)
            {
                if (blockPropsChanged[b].ChangedScope != 0)
                {
                    return true;
                }
            }
            return false;
        }
        void SaveBlockProps()
        {
            for (int b = 0; b < blockPropsChanged.Length; b++)
            {
                if (blockPropsChanged[b].ChangedScope == 0)
                {
                    continue;
                }
                Block.Props[b] = blockPropsChanged[b];
            }
            BlockProps.Save("default", Block.Props, 1);
            Block.SetBlocks();
        }
        void Blk_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            curBlock = blockIDMap[blk_list.SelectedIndex];
            placePermsOrig = BlockPerms.GetPlace(curBlock);
            placePermsCopy = placePermsChanged.Find(p => p.ID == curBlock);
            BlockInitSpecificArrays();
            blockItems.SupressEvents = true;
            BlockProps props = blockPropsChanged[curBlock];
            blk_cbMsgBlock.Checked = props.IsMessageBlock;
            blk_cbPortal.Checked = props.IsPortal;
            blk_cbDeath.Checked = props.KillerBlock;
            blk_txtDeath.Text = props.DeathMessage;
            blk_txtDeath.Enabled = blk_cbDeath.Checked;
            blk_cbDoor.Checked = props.IsDoor;
            blk_cbTdoor.Checked = props.IsTDoor;
            blk_cbRails.Checked = props.IsRails;
            blk_cbLava.Checked = props.LavaKills;
            blk_cbWater.Checked = props.WaterKills;
            BlockPerms perms = placePermsCopy ?? placePermsOrig;
            blockItems.Update(perms);
        }
        void BlockInitSpecificArrays()
        {
            if (blockItems.MinBox != null)
            {
                return;
            }
            blockItems.MinBox = blk_cmbMin;
            blockItems.AllowBoxes = new[] 
            { 
                blk_cmbAlw1, blk_cmbAlw2, blk_cmbAlw3 
            };
            blockItems.DisallowBoxes = new[] 
            { 
                blk_cmbDis1, blk_cmbDis2, blk_cmbDis3 
            };
            blockItems.FillInitial();
        }
        ItemPerms BlockGetOrAddPermsChanged()
        {
            if (placePermsCopy != null)
            {
                return placePermsCopy;
            }
            placePermsCopy = placePermsOrig.Copy();
            placePermsChanged.Add(placePermsCopy);
            return placePermsCopy;
        }
        void Blk_cmbMin_SelectedIndexChanged(object sender, EventArgs e)
        {
            blockItems.OnMinRankChanged((ComboBox)sender);
        }
        void Blk_cmbSpecific_SelectedIndexChanged(object sender, EventArgs e)
        {
            blockItems.OnSpecificChanged((ComboBox)sender);
        }
        void Blk_btnHelp_Click(object sender, EventArgs e)
        {
            GetHelp(blk_list.SelectedItem.ToString());
        }
        void Blk_cbMsgBlock_CheckedChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].IsMessageBlock = blk_cbMsgBlock.Checked;
            MarkBlockPropsChanged();
        }
        void Blk_cbPortal_CheckedChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].IsPortal = blk_cbPortal.Checked;
            MarkBlockPropsChanged();
        }
        void Blk_cbDeath_CheckedChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].KillerBlock = blk_cbDeath.Checked;
            blk_txtDeath.Enabled = blk_cbDeath.Checked;
            MarkBlockPropsChanged();
        }
        void Blk_txtDeath_TextChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].DeathMessage = blk_txtDeath.Text;
            MarkBlockPropsChanged();
        }
        void Blk_cbDoor_CheckedChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].IsDoor = blk_cbDoor.Checked;
            MarkBlockPropsChanged();
        }
        void Blk_cbTdoor_CheckedChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].IsTDoor = blk_cbTdoor.Checked;
            MarkBlockPropsChanged();
        }
        void Blk_cbRails_CheckedChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].IsRails = blk_cbRails.Checked;
            MarkBlockPropsChanged();
        }
        void Blk_cbLava_CheckedChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].LavaKills = blk_cbLava.Checked;
            MarkBlockPropsChanged();
        }
        void Blk_cbWater_CheckedChanged(object sender, EventArgs e)
        {
            blockPropsChanged[curBlock].WaterKills = blk_cbWater.Checked;
            MarkBlockPropsChanged();
        }
        void MarkBlockPropsChanged()
        {
            int changed = blockItems.SupressEvents ? 0 : BlockProps.SCOPE_GLOBAL;
            blockPropsChanged[curBlock].ChangedScope = (byte)changed;
        }
    }
}
