// class for towers, barracks, filler buildings, and the ancients (or forts in the log files)
// Note: the stores are currently considered a subclass of Actor

using System;

namespace Dota2Lib
{
	public enum BuildingNames { // radiant buildings
								dota_goodguys_tower1_top, dota_goodguys_tower2_top, dota_goodguys_tower3_top, dota_goodguys_tower4_top,
								dota_goodguys_tower1_mid, dota_goodguys_tower2_mid, dota_goodguys_tower3_mid,
								dota_goodguys_tower1_bot, dota_goodguys_tower2_bot, dota_goodguys_tower3_bot, dota_goodguys_tower4_bot,
								good_rax_range_top, good_rax_melee_top, good_rax_range_mid, good_rax_melee_mid, good_rax_range_bot, good_rax_melee_bot,
								good_filler_1, good_filler_2, good_filler_3, good_filler_4, good_filler_5,
								good_filler_6, good_filler_7, good_filler_8, good_filler_9, good_filler_10,
								good_filler_11, good_filler_12, good_filler_13, good_filler_14, good_filler_15,
								dota_goodguys_fort,		// fort or ancient

								// dire buildings
								dota_badguys_tower1_top, dota_badguys_tower2_top, dota_badguys_tower3_top, dota_badguys_tower4_top,
								dota_badguys_tower1_mid, dota_badguys_tower2_mid, dota_badguys_tower3_mid,
								dota_badguys_tower1_bot, dota_badguys_tower2_bot, dota_badguys_tower3_bot, dota_badguys_tower4_bot,
								bad_rax_range_top, bad_rax_melee_top, bad_rax_range_mid, bad_rax_melee_mid, bad_rax_range_bot, bad_rax_melee_bot,
								bad_filler_1, bad_filler_2, bad_filler_3, bad_filler_4, bad_filler_5,
								bad_filler_6, bad_filler_7, bad_filler_8, bad_filler_9, bad_filler_10,
								bad_filler_11, bad_filler_12, bad_filler_13, bad_filler_14, bad_filler_15,
								dota_badguys_fort,		// fort or ancient
								}

	public class Building
	{
		public BuildingNames Id {get; set;}
		public TeamNames Team {get; set;}
		public Locations Location {get; set;}
		public int CellX {get; set;}
		public int CellY {get; set;}
		public int MaxHealth {get; set;}
		public int Health {get; set;}

		public Building()
		{
			Team = TeamNames.Undefined;
			Location = Locations.undefined;
			CellX = -1;
			CellY = -1;
			MaxHealth = Health = -1;
		}

		public Building(BuildingNames name, TeamNames team, Locations location)
		{
			Id = name;
			Team = team;
			Location = location;
			CellX = -1;
			CellY = -1;
			MaxHealth = Health = -1;
		}

		public void Init(int cellX, int cellY, int maxHealth)
		{
			CellX = cellX;
			CellY = cellY;
			MaxHealth = Health = maxHealth;
		}

		public bool IsAlive() {
			if (Health > 0) return true;
			return false;
		}
	}
}

