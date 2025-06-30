#if NAS && !NET_20 && TEN_BIT_BLOCKS
using Newtonsoft.Json;
using MCGalaxy;
using System.Collections.Generic;
namespace NotAwesomeSurvival
{
    public class Item
    {
        public static Item Fist;
        public string name;
        public float HP;
        public float armor;
        public string displayName = null;
        public Dictionary<string, int> enchants = new Dictionary<string, int>(){
                {"Aqua Affinity",0},
                {"Efficiency",0},
                {"Feather Falling",0},
                {"Fire Protection",0},
                {"Fortune",0},
                {"Knockback",0},
                {"Mending",0},
                {"Protection",0},
                {"Respiration",0},
                {"Sharpness",0},
                {"Silk Touch",0},
                {"Thorns",0},
                {"Unbreaking",0},
            };
        [JsonIgnore] public ItemProp Prop { get { return ItemProp.props[name]; } }
        public Item(string name)
        {
            ItemProp prop = ItemProp.props[name];
            this.name = prop.name;
            HP = prop.baseHP;
            armor = prop.armor;
            if (displayName == null)
            {
                displayName = ColoredName;
            }
        }
        [JsonIgnore]
        public string ColoredName
        {
            get { return "&" + ItemProp.props[name].color + name; }
        }
        [JsonIgnore]
        public string ColoredIcon
        {
            get { return "&" + ItemProp.props[name].color + ItemProp.props[name].character; }
        }
        [JsonIgnore]
        public ColorDesc[] HealthColors
        {
            get
            {
                if (HP == int.MaxValue) 
                {
                    return DynamicColor.defaultColors; 
                }
                if (HP <= 1) 
                { 
                    return DynamicColor.direHealthColors; 
                }
                float healthPercent = HP / Prop.baseHP;
                if (healthPercent > 0.5f) 
                {
                    return DynamicColor.fullHealthColors; 
                }
                if (healthPercent > 0.25) 
                { 
                    return DynamicColor.mediumHealthColors; 
                }
                return DynamicColor.lowHealthColors;
            }
        }
        /// <summary>
        /// Call to take damage
        /// </summary>
        /// <param name="amount">the amount of damage to take. Breaking a block normally gives 1 damage</param>
        /// <returns>true if the item should break</returns>
        public bool TakeDamage(float amount = 1)
        {
            if (HP == int.MaxValue) 
            { 
                return false; 
            }
            HP -= amount;
            if (HP <= 0)
            {
                return true;
            }
            return false;
        }
        public bool Enchanted()
        {
            try
            {
                foreach (KeyValuePair<string, int> x in enchants)
                {
                    if (x.Value > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch 
            { 
                return false; 
            }
        }
        public int Enchant(string s)
        {
            if (enchants.ContainsKey(s))
            {
                return enchants[s];
            }
            enchants.Add(s, 0);
            return 0;
        }
    }
}
#endif