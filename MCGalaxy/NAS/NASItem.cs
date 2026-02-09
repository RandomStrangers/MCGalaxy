using Newtonsoft.Json;
using System.Collections.Generic;
namespace MCGalaxy
{
    public class NASItem
    {
        [JsonIgnore] public NASItemProp Prop => NASItemProp.props[name];
        [JsonIgnore]
        public string ColoredName => "&" + NASItemProp.props[name].color + name;
        [JsonIgnore]
        public string ColoredIcon => "&" + NASItemProp.props[name].color + NASItemProp.props[name].character;
        [JsonIgnore]
        public ColorDesc[] HealthColors
        {
            get
            {
                if (HP == int.MaxValue)
                {
                    return NASColor.defaultColors;
                }
                if (HP <= 1)
                {
                    return NASColor.direHealthColors;
                }
                float healthPercent = HP / Prop.baseHP;
                if (healthPercent > 0.5f)
                {
                    return NASColor.fullHealthColors;
                }
                if (healthPercent > 0.25)
                {
                    return NASColor.mediumHealthColors;
                }
                return NASColor.lowHealthColors;
            }
        }
        public static NASItem Fist;
        public string name, displayName = null;
        public float HP, armor;
        public Dictionary<string, int> enchants = new(){
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
        public NASItem(string name)
        {
            NASItemProp prop = NASItemProp.props[name];
            this.name = prop.name;
            HP = prop.baseHP;
            armor = prop.armor;
            displayName ??= ColoredName;
        }
        public bool TakeDamage(float amount)
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
