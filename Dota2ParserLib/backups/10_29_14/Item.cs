using System;

namespace Dota2Lib
{
	// Note: often the components for an upgraded item (e.g., the recipe) that a player has not already purchased
	//	will automatically be purchased and consumed in the same instant when the player purchases the upgraded item
	//	thus they may never appear in the Hero's inventory
	// Note: we have not added in all the items yet (it can be hard to guess how they are represented in the log files - 
	//	often the names are very different from the in-game names), so we'll be adding them in as we encounter unhandled
	//	items in new log files - check the console output for the warnings that an item name was not recognized and add it here
	public enum ItemType { empty, 

		// reduced item list for testing
		//		stout_shield, branches, boots, circlet, robe
		//	};

		// roshan drops - Note: cannot be purchased
		aegis, cheese, 

		// consumables - Note: flask is what the log files call the healing salve
		bottle, clarity, courier, dust_of_appearance, flask, flying_courier, smoke_of_deceit, 
		tango, tango_single, tpscroll, ward_observer, ward_sentry, 

		// attributes
		belt_of_strength, blade_of_alacrity, branches, circlet, gauntlets, mantle, ogre_axe, 
		robe, slippers, staff_of_wizardry, ultimate_orb,

		// armaments
		blades_of_attack, broadsword, chainmail, claymore, helm_of_iron_will, magic_stick, 
		mithril_hammer, platemail, quarterstaff, ring_of_protection, stout_shield, 


		// arcane - Note: sobi_mask is what the log files call the sage's mask
		blink, boots, cloak, gem, gloves, ring_of_regen, sobi_mask, talisman_of_evasion, 

		// secret shop (some are also available at the side shop)
		eagle, energy_booster, hyperstone, point_booster, mystic_staff, ring_of_health, void_stone, 

		// recipes
		recipe_ancient_janggo, recipe_arcane_boots, recipe_armlet, recipe_black_king_bar, recipe_blade_mail, 
		recipe_buckler, recipe_butterfly, recipe_force_staff, recipe_hand_of_midas, recipe_headdress, 
		recipe_maelstrom, recipe_magic_wand, recipe_mekansm, recipe_medallion_of_courage, recipe_mjollnir, 
		recipe_necronomicon, recipe_necronomicon_2, recipe_necronomicon_3, recipe_phase_boots, recipe_power_treads, 
		recipe_ring_of_aquila, recipe_sheepstick, recipe_shivas_guard, recipe_sphere, recipe_tranquil_boots, 
		recipe_travel_boots, recipe_ultimate_scepter, recipe_urn_of_shadows, recipe_wraith_band, 

		// common upgrades - Note: pers is what the log files call perserverance
		bracer, hand_of_midas, magic_wand, null_talisman, pers, phase_boots, power_treads, travel_boots, wraith_band, 

		// support upgrades - Note: ancient_janggo is what the log files call the drum of endurance
		ancient_janggo, arcane_boots, buckler, headdress, medallion_of_courage, mekansm, ring_of_aquila,
		ring_of_basilius, tranquil_boots, urn_of_shadows, 

		// caster upgrades - Note: sheepstick => scythe of vyse; ultimate_scepter => aghanim's scepter
		force_staff, necronomicon, necronomicon_2, necronomicon_3, sheepstick, ultimate_scepter, 

		// weapon upgrades - Note: lesser_crit => crystalys
		armlet, bfury, butterfly, lesser_crit, 

		// armor upgrades
		black_king_bar, blade_mail, shivas_guard, sphere, 

		// artifact upgrades
		maelstrom, mjollnir
	};

	public class Item
	{
		public ItemType Type {get; set;}
		public int Id {get; set;}
		public int Charges {get; set;}

		public Item()
		{
			Type = ItemType.empty;
			Id = -1;
			Charges = 0;
		}
		public Item(string name, int id) {
			Type = GetTypeByName(name);
			Id = id;
			Charges = 0;
		}
		public Item(string name, int id, int charges) {
			Type = GetTypeByName(name);
			Id = id;
			Charges = charges;
		}
		public Item(ItemType type) {
			Type = type;
			Id = -1;
			Charges = 0;
		}
		public Item(ItemType type, int id) {
			Type = type;
			Id = id;
			Charges = 0;
		}
		public Item(ItemType type, int id, int charges) {
			Type = type;
			Id = id;
			Charges = charges;
		}

		public static ItemType GetTypeByName(string name) {
			return ItemType.empty;
		}

		static public bool IsConsumable(ItemType type) {
			if (type >= ItemType.cheese && type <= ItemType.ward_sentry) {
				return true;
			}
			return false;
		}
		public bool IsConsumable() {
			return IsConsumable(Type);
		}

		static public bool IsPurchasable(ItemType type) {
			if (type >= ItemType.bottle) {
				return true;
			}
			return false;
		}
		public bool IsPurchasable() {
			return IsPurchasable(Type);
		}

		static public bool IsUpgrade(ItemType type) {
			if (type >= ItemType.bracer) {
				return true;
			}
			return false;
		}
		public bool IsUpgrage() {
			return IsUpgrade(Type);
		}

		public bool IsEmpty() {
			if (Type == ItemType.empty) {
				return true;
			}
			return false;
		}
	}
}

