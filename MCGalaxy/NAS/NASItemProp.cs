using System.Collections.Generic;
namespace MCGalaxy
{
    public partial class NASItemProp
    {
        public string name, color, character;
        public List<NASMaterial> materialsEffectiveAgainst;
        public int tier, recharge;
        public float percentageOfTimeSaved, baseHP, damage, armor, knockback;
        public Dictionary<string, bool> allowedEnchants = new();
        public static Dictionary<string, NASItemProp> props = new();
        public NASItemProp(string description, Dictionary<string, bool> enchants = null, NASMaterial effectiveAgainst = NASMaterial.None, float percentageOfTimeSaved = 0, int tier = 1)
        {
            string[] descriptionBits = description.Split('|');
            name = descriptionBits[0];
            color = descriptionBits[1];
            character = descriptionBits[2];
            allowedEnchants = new()
            {
                {"Aqua Affinity",false},
                {"Efficiency",false},
                {"Feather Falling",false},
                {"Fortune",false},
                {"Knockback",false},
                {"Mending",false},
                {"Protection",false},
                {"Respiration",false},
                {"Sharpness",false},
                {"Silk Touch",false},
                {"Thorns",false},
                {"Unbreaking",false},
            };
            foreach (KeyValuePair<string, bool> x in enchants)
                allowedEnchants[x.Key] = x.Value;
            if (effectiveAgainst != NASMaterial.None)
                materialsEffectiveAgainst = new()
                {
                    effectiveAgainst
                };
            else
                materialsEffectiveAgainst = null;
            this.tier = tier;
            this.percentageOfTimeSaved = percentageOfTimeSaved;
            baseHP = 200;
            damage = 0.5f;
            recharge = 250;
            armor = 0f;
            knockback = 0.5f;
            props.Add(name, this);
        }
    }
}
