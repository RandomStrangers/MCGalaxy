#if NAS
using System.Collections.Generic;

namespace NotAwesomeSurvival
{

    public partial class ItemProp
    {

        public string name;
        public string color;
        public string character;

        public List<NasBlock.Material> materialsEffectiveAgainst;
        public int tier;
        public float percentageOfTimeSaved;
        public const int baseHPconst = 200;
        public float baseHP;
        public float damage;
        public int recharge;
        public float armor;
        public float knockback;
        public Dictionary<string, bool> allowedEnchants = new Dictionary<string, bool>();
        public static Dictionary<string, ItemProp> props = new Dictionary<string, ItemProp>();

        public ItemProp(string description, Dictionary<string, bool> enchants = null, NasBlock.Material effectiveAgainst = NasBlock.Material.None, float percentageOfTimeSaved = 0, int tier = 1)
        {
            string[] descriptionBits = description.Split('|');
            name = descriptionBits[0];
            color = descriptionBits[1];
            character = descriptionBits[2];
            allowedEnchants = new Dictionary<string, bool>(){
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
            {
                allowedEnchants[x.Key] = x.Value;
            }

            if (effectiveAgainst != NasBlock.Material.None)
            {
                materialsEffectiveAgainst = new List<NasBlock.Material>
                {
                    effectiveAgainst
                };
            }
            else
            {
                materialsEffectiveAgainst = null;
            }
            //tier 0 is fists
            this.tier = tier;
            this.percentageOfTimeSaved = percentageOfTimeSaved;
            baseHP = baseHPconst;
            damage = 0.5f;
            recharge = 250;
            armor = 0f;
            knockback = 0.5f;
            props.Add(name, this);
        }
    }

}
#endif