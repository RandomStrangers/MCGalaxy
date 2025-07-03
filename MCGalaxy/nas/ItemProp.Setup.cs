#if NAS && TEN_BIT_BLOCKS
using System.Collections.Generic;
namespace NotAwesomeSurvival
{
    public partial class ItemProp
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public static void Setup()
        {
            Dictionary<string, bool> nothing = new Dictionary<string, bool>();
            Dictionary<string, bool> toolEnchants = new Dictionary<string, bool>(){
                {"Efficiency",true},
                {"Fortune",true},
                {"Mending",true},
                {"Silk Touch",true},
                {"Unbreaking",true},
            };
            Dictionary<string, bool> swordEnchants = new Dictionary<string, bool>(){
                {"Knockback",true},
                {"Mending",true},
                {"Sharpness",true},
                {"Unbreaking",true},
            };
            Dictionary<string, bool> bootEnchants = new Dictionary<string, bool>(){
                {"Feather Falling",true},
                {"Mending",true},
                {"Protection",true},
                {"Thorns",true},
                {"Unbreaking",true},
            };
            Dictionary<string, bool> helmetEnchants = new Dictionary<string, bool>(){
                {"Aqua Affinity",true},
                {"Mending",true},
                {"Protection",true},
                {"Respiration",true},
                {"Thorns",true},
                {"Unbreaking",true},
            };
            Dictionary<string, bool> armorEnchants = new Dictionary<string, bool>(){
                {"Mending",true},
                {"Protection",true},
                {"Thorns",true},
                {"Unbreaking",true},
            };
            ItemProp fist = new ItemProp("Fist|f|¬", nothing, NasBlock.Material.None, 0, 0)
            {
                baseHP = int.MaxValue
            };
            Item.Fist = new Item("Fist");
            ItemProp key = new ItemProp("Key|f|σ", nothing, NasBlock.Material.None, 0, 0)
            {
                baseHP = int.MaxValue
            };
            ItemProp fishing = new ItemProp("Fishing Rod|f|δ", nothing, NasBlock.Material.None, 0, 0)
            {
                baseHP = baseHPconst * 7,
                damage = 0f,
                knockback = -1f
            };
            ItemProp shears = new ItemProp("Shears|f|µ", nothing, NasBlock.Material.Organic, 0.75f, 1)
            {
                baseHP = baseHPconst * 8
            };
            ItemProp woodPick = new ItemProp("Wood Pickaxe|s|ß", toolEnchants, NasBlock.Material.Stone, 0.0f, 1)
            {
                baseHP = 12
            };
            ItemProp stonePick = new ItemProp("Stone Pickaxe|7|ß", toolEnchants, NasBlock.Material.Stone, 0.75f, 1);
            ItemProp stoneShovel = new ItemProp("Stone Shovel|7|Γ", toolEnchants, NasBlock.Material.Earth, 0.50f, 1);
            ItemProp stoneAxe = new ItemProp("Stone Axe|7|π", toolEnchants, NasBlock.Material.Wood, 0.60f, 1);
            stoneAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp stoneSword = new ItemProp("Stone Sword|7|α", swordEnchants, NasBlock.Material.Leaves, 0.50f, 1)
            {
                damage = 2.5f,
                recharge = 750
            };
            const int ironBaseHP = baseHPconst * 8;
            ItemProp ironPick = new ItemProp("Iron Pickaxe|f|ß", toolEnchants, NasBlock.Material.Stone, 0.85f, 3)
            {
                baseHP = ironBaseHP
            };
            ItemProp ironShovel = new ItemProp("Iron Shovel|f|Γ", toolEnchants, NasBlock.Material.Earth, 0.60f, 3)
            {
                baseHP = ironBaseHP
            };
            ItemProp ironAxe = new ItemProp("Iron Axe|f|π", toolEnchants, NasBlock.Material.Wood, 0.75f, 3)
            {
                baseHP = ironBaseHP
            };
            ironAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp ironSword = new ItemProp("Iron Sword|f|α", swordEnchants, NasBlock.Material.Leaves, 0.75f, 3)
            {
                damage = 3f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = ironBaseHP
            };
            const int goldBaseHP = baseHPconst * 64;
            ItemProp goldPick = new ItemProp("Gold Pickaxe|6|ß", toolEnchants, NasBlock.Material.Stone, 0.90f, 3)
            {
                baseHP = goldBaseHP
            };
            ItemProp goldShovel = new ItemProp("Gold Shovel|6|Γ", toolEnchants, NasBlock.Material.Earth, 0.85f, 3)
            {
                baseHP = goldBaseHP
            };
            ItemProp goldAxe = new ItemProp("Gold Axe|6|π", toolEnchants, NasBlock.Material.Wood, 0.90f, 3)
            {
                baseHP = goldBaseHP
            };
            goldAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp goldSword = new ItemProp("Gold Sword|6|α", swordEnchants, NasBlock.Material.Leaves, 0.85f, 3)
            {
                damage = 3f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = goldBaseHP
            };
            const int diamondBaseHP = baseHPconst * 128;
            ItemProp diamondPick = new ItemProp("Diamond Pickaxe|b|ß", toolEnchants, NasBlock.Material.Stone, 0.95f, 4)
            {
                baseHP = diamondBaseHP
            };
            ItemProp diamondShovel = new ItemProp("Diamond Shovel|b|Γ", toolEnchants, NasBlock.Material.Earth, 1f, 4)
            {
                baseHP = diamondBaseHP
            };
            ItemProp diamondAxe = new ItemProp("Diamond Axe|b|π", toolEnchants, NasBlock.Material.Wood, 0.95f, 4)
            {
                baseHP = diamondBaseHP
            };
            diamondAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp diamondSword = new ItemProp("Diamond Sword|b|α", swordEnchants, NasBlock.Material.Leaves, 1f, 4)
            {
                damage = 3.5f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = diamondBaseHP
            };
            const int emeraldBaseHP = baseHPconst * 192;
            ItemProp emeraldPick = new ItemProp("Emerald Pickaxe|2|ß", toolEnchants, NasBlock.Material.Stone, 0.975f, 5)
            {
                baseHP = emeraldBaseHP
            };
            emeraldPick.allowedEnchants["Efficiency"] = true;
            ItemProp emeraldShovel = new ItemProp("Emerald Shovel|2|Γ", toolEnchants, NasBlock.Material.Earth, 1f, 5)
            {
                baseHP = emeraldBaseHP
            };
            ItemProp emeraldAxe = new ItemProp("Emerald Axe|2|π", toolEnchants, NasBlock.Material.Wood, 0.975f, 5)
            {
                baseHP = emeraldBaseHP
            };
            emeraldAxe.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            ItemProp emeraldSword = new ItemProp("Emerald Sword|2|α", swordEnchants, NasBlock.Material.Leaves, 1f, 5)
            {
                damage = 4f,
                recharge = 750,
                knockback = 1.25f,
                baseHP = emeraldBaseHP
            };
            ItemProp bedrockPick = new ItemProp("Bedrock Pickaxe|m|╟", nothing, NasBlock.Material.None, -1f, 5)
            {
                baseHP = int.MaxValue
            };
            ItemProp etheriumPick = new ItemProp("Etherium Pickaxe|h|ß", toolEnchants, NasBlock.Material.Stone, 1f, 6)
            {
                baseHP = int.MaxValue
            };
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Stone);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Earth);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Wood);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Plant);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Leaves);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Organic);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Glass);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Metal);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Liquid);
            etheriumPick.materialsEffectiveAgainst.Add(NasBlock.Material.Lava);
            ItemProp bedrockSword = new ItemProp("Bedrock Sword|0|α", swordEnchants, NasBlock.Material.Leaves, 1f, 6)
            {
                damage = 50f,
                knockback = 2f,
                recharge = 1,
                baseHP = int.MaxValue
            };
            ItemProp ironHelmet = new ItemProp("Iron Helmet|f|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 2f
            };
            ItemProp ironChest = new ItemProp("Iron Chestplate|f|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 5f
            };
            ItemProp ironLegs = new ItemProp("Iron Leggings|f|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 4f
            };
            ItemProp ironBoots = new ItemProp("Iron Boots|f|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = ironBaseHP / 2f,
                armor = 1f
            };
            ItemProp goldHelmet = new ItemProp("Gold Helmet|6|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 2f
            };
            ItemProp goldChest = new ItemProp("Gold Chestplate|6|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 6f
            };
            ItemProp goldLegs = new ItemProp("Gold Leggings|6|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 5f
            };
            ItemProp goldBoots = new ItemProp("Gold Boots|6|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = goldBaseHP / 2f,
                armor = 2f
            };
            ItemProp diamondHelmet = new ItemProp("Diamond Helmet|b|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 3f
            };
            ItemProp diamondChest = new ItemProp("Diamond Chestplate|b|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 7f
            };
            ItemProp diamondLegs = new ItemProp("Diamond Leggings|b|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 5f
            };
            ItemProp diamondBoots = new ItemProp("Diamond Boots|b|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = diamondBaseHP / 2f,
                armor = 3f
            };
            ItemProp emeraldHelmet = new ItemProp("Emerald Helmet|2|τ", helmetEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 3f
            };
            ItemProp emeraldChest = new ItemProp("Emerald Chestplate|2|Φ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 8f
            };
            ItemProp emeraldLegs = new ItemProp("Emerald Leggings|2|Θ", armorEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 6f
            };
            ItemProp emeraldBoots = new ItemProp("Emerald Boots|2|Ω", bootEnchants, NasBlock.Material.None, 0, 0)
            {
                baseHP = emeraldBaseHP / 2f,
                armor = 3f
            };
        }
    }
}
#endif