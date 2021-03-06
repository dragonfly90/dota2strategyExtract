﻿using System;
using System.Collections.Generic;
using System.Xml;

namespace Dota2Lib
{
	public class Match
	{
		// counters for the number of players on each team to have entered the game
		// each team should have 5 players
//		int RadiantCount = 0;
//		int DireCount = 0;

		// each of the five players on each team is represented by a Hero character
//		private Hero[] RadiantTeam = new Hero[5];
//		private Hero[] DireTeam = new Hero[5];
		private List<Hero> RadiantTeam = new List<Hero>();
		private List<Hero> DireTeam = new List<Hero>();

		// a few (5) items sold in the stores have limited inventory that replenishes after a fixed time
		// keep track of those items
		private Store RadiantStore = new Store(TeamNames.Radiant);
		private Store DireStore = new Store(TeamNames.Dire);


		// keep track of whether a team has activated a courier yet
		private bool RadiantCourierActive = false;
		private bool DireCourierActive = false;

		// in the games log files, the game state is updated each tick
		// there are 30 ticks per second
		// track the current tick as we move through the parse,
		//	along with the first and last recorded ticks
		private int Tick = -1;
		private int FirstTick = 0;
		private int LastTick = -1;

		public Match()
		{
			// initiallize the arrays that will store the players' heroes
/*			for (int i = 0; i < 5; i++) {
				RadiantTeam[i] = new Hero(TeamNames.Radiant);
				DireTeam[i] = new Hero(TeamNames.Dire);
			}
*/
		}

		// search the Hero arrays for a character with a matching name
		private Hero GetHeroByName(string name)
		{
			int i;

			// check the list of Radiant heroes
			foreach (Hero hero in RadiantTeam) {
				if (hero.Name.Equals(name)) return hero;
			}
/*			for (i = 0; i < RadiantCount; i++) {
				if (RadiantTeam[i].Name.Equals(name)) {
					return RadiantTeam[i];
				}
			}
*/
			// check the list of Dire heroes
			foreach (Hero hero in DireTeam) {
				if (hero.Name.Equals(name)) return hero;
			}
/*			for (i = 0; i < DireCount; i++) {
				if (DireTeam[i].Name.Equals(name)) {
					return DireTeam[i];
				}
			}
*/
			return null;	// no matching hero found
		}

		public bool Init(string logFilePath) {
			// read through the parsed shopping log file
			// storing the recorded state changes in the associated actor's list
			// in this case, an actor may be a hero or one of the team stores
			using (XmlReader logReader = XmlReader.Create(logFilePath))
			{
				while (logReader.Read())
				{
					if (logReader.IsStartElement())	// if we have a start tag
					{
						string attribute;	// the xml attribute being read
						ItemType type;		// we will attempt to parse item names to get the corresponding ItemType enum value
						int id;				// items should have a unique id number to id the instance
						string name;		// the name of a hero
						Hero target;		// reference to the corresponding hero data structure

						// handle the element
						switch (logReader.Name)
						{
							case "shopping_parse":
								// this just identifies the log as a parse containing
								// info about item purchases, etc.
								// we don't really need to do anything here
								break;
							case "time_step":
								// the parsed log file groups together state changes across all entities
								// that occur at the same time step
								// this marks the start of a new time step
								attribute = logReader["tick"];	// get the tick (time) of this step in the log file
								if (attribute != null) {
									Tick = Convert.ToInt32(attribute);

									// save the time of the first tick in the log
									if (FirstTick == -1) {
										FirstTick = Tick;
									}
								}
								break;
							case "hero_spawn":
								// this identifies the entry of a new character into the match
								// each player controls one hero
								name = logReader["hero"];	// retrieve the name of the hero
								attribute = logReader["team"];	// retrieve the team to which the hero belongs (radiant or dire)

								if (name == null || attribute == null) break;

								// add the hero to the correct team and get a reference
								// to its data structure
								if (attribute.Equals("radiant")) {
									target = new Hero(name, TeamNames.Radiant);
									RadiantTeam.Add(target);
								}
								else if (attribute.Equals("dire")) {
									target = new Hero(name, TeamNames.Dire);
									DireTeam.Add(target);
								}
								else break;
/*								target = null;
								if (attribute.Equals("radiant") && RadiantCount < 5) {
									RadiantTeam[RadiantCount].Name = name;
									target = RadiantTeam[RadiantCount];
									RadiantCount++;
								}
								else if (attribute.Equals("dire") && DireCount < 5){
									DireTeam[DireCount].Name = name;
									target = DireTeam[DireCount];
									DireCount++;
								}
								else break;
*/

								// store the time at which the hero entered the match
								target.SpawnTime = Tick;

								// create a new event to record the state change
								HeroSpawn spawnEvent = new HeroSpawn();

								// record the hero's starting maximum health and mana and starting gold values
								attribute = logReader["max_health"];
								if (attribute != null) {
									spawnEvent.MaxHealth = Convert.ToInt32(attribute);
								}
								attribute = logReader["max_mana"];
								if (attribute != null) {
									spawnEvent.MaxMana = Convert.ToInt32(attribute);
								}
								attribute = logReader["gold"];
								if (attribute != null) {
									spawnEvent.Gold = Convert.ToInt32(attribute);
								}

								// add the event to the hero's list of state changes
								target.AddDiff(Tick, spawnEvent);

								break;
							case "courier_spawn":
								// indicates that someone on the specified team has activated a courier for the team
								attribute = logReader["team"];
								if (attribute == null) break;

								if (attribute == "radiant") {
									RadiantCourierActive = true;
								}
								else if (attribute == "dire") {
									DireCourierActive = true;
								}
								break;
							case "level_up":
								// this indicates that the specified hero has gained a level
								// leveling up increases the hero's maximum health and mana
								// it also gives them a new ability point to spend, but we're not
								// tracking abilities here
								target = GetHeroByName(logReader["hero"]);	// find the hero with the matching name
								if (target == null) break;

								LevelUp levelEvent = new LevelUp();	// create a new level up state change event

								// read in the new level
								// not strictly necessary, since you can only gain one level at a time
								attribute = logReader["level"];
								if (attribute == null) break;
								levelEvent.Level = Convert.ToInt32(attribute);

								// read in the new maximum health and mana values
								attribute = logReader["max_health"];
								if (attribute == null) break;
								levelEvent.MaxHealth = Convert.ToInt32(attribute);
								attribute = logReader["max_mana"];
								if (attribute == null) break;
								levelEvent.MaxMana = Convert.ToInt32(attribute);

								// add the state change event to the hero's list
								target.AddDiff(Tick, levelEvent);
								break;
							case "status_update":
								// this indicates that the hero's health or mana (or both) changed (up or down)
								// there are also other status effects (e.g., stuns, death, etc.), but we aren't
								//	tracking those here
								target = GetHeroByName(logReader["hero"]);	// find the matching hero
								if (target == null) break;

								// create a new status update state change event
								StatusUpdate statusEvent = new StatusUpdate();

								// read in the new health value (if specified)
								attribute = logReader["health"];
								if (attribute != null) {
									statusEvent.Health = Convert.ToInt32(attribute);
								}

								// read in the new mana value (if specified)
								attribute = logReader["mana"];
								if (attribute != null) {
									statusEvent.Mana = Convert.ToInt32(attribute);
								}

								// check that at least one or the other (health or mana) changed
								if (statusEvent.Health == -1 && statusEvent.Mana == -1) break;

								// add the event to the hero's status changes list
								target.AddDiff(Tick, statusEvent);
								break;
							case "gold_update":
								// this indicates that the amount of gold a hero has changed
								// goes down when the hero spends it to buy something
								// goes up at a slow, but steady rate or when the hero earns gold by
								//	doing something (or sharing in something the team did)
								target = GetHeroByName(logReader["hero"]);	// find the matching hero
								if (target == null) break;

								// create a new status change event
								GoldUpdate goldEvent = new GoldUpdate();

								// read in the new gold value
								attribute = logReader["gold"];
								if (attribute == null) break;
								goldEvent.Gold = Convert.ToInt32(attribute);

								// add the event to the hero's list of state changes
								target.AddDiff(Tick, goldEvent);
								break;
							case "item_gained":
								// this indicates that the hero has just gained an item (purchased and/or upgraded)
								// will usually co-occur with an inventory_update and often a gold_update
								// but inventory_updates and gold_updates can also occur under other circumstances
								// and there is at least one case (buying a bottle) that may not always generate
								//	an inventory_update (the bottle may go directly to the courier's inventory)
								target = GetHeroByName(logReader["hero"]);	// find the matching hero
								if (target == null) break;

								// create a new status change event
								ItemGained itemGainedEvent = new ItemGained();

								// get the id of the item (should be unique to the instance)
								attribute = logReader["id"];
								if (attribute == null) break;
								id = Convert.ToInt32(attribute);

								// read in the name of the item
								attribute = logReader["item"];
								if (attribute == null) break;
								if (Enum.TryParse(attribute, out type) == true) {

									// read in the number of charges (if specified)
									attribute = logReader["charges"];
									if (attribute != null) {
										itemGainedEvent.NewItem = new Item(type, id, Convert.ToInt32(attribute));
									}
									else {
										itemGainedEvent.NewItem = new Item(type, id);
									}
								}
								else {
									Console.WriteLine("Unrecognized item name: " + attribute);
									break;
								}
								// add the event to the hero's state change list
								target.AddDiff(Tick, itemGainedEvent);
								break;
							case "item_lost":
								// this indicates that the hero has just lost an item (sold, consumed, or used in an upgrade)
								// will usually co-occur with an inventory_update and sometimes a gold_update
								// but inventory_updates and gold_updates can also occur under other circumstances
								target = GetHeroByName(logReader["hero"]);	// find the matching hero
								if (target == null) break;

								// create a new status change event
								ItemLost itemLostEvent = new ItemLost();

								// get the id of the item (should be unique to the instance)
								attribute = logReader["id"];
								if (attribute == null) break;
								id = Convert.ToInt32(attribute);

								// read in the name of the item
								attribute = logReader["item"];
								if (attribute == null) break;
								if (Enum.TryParse(attribute, out type) == true) {
									itemLostEvent.LostItemType = type;
									itemLostEvent.LostItemId = id;
								}
								else {
									Console.WriteLine("Unrecognized item name: " + attribute);
									break;
								}
								// add the event to the hero's state change list
								target.AddDiff(Tick, itemLostEvent);
								break;
							case "inventory_update":
								// this indicates that the hero's inventory has changed
								// buying an item, dropping or selling an item, or using a consumable
								// some items have charges that are used up, those changes are also
								//	reflected here
								target = GetHeroByName(logReader["hero"]);	// find the matching hero
								if (target == null) break;

								// create a new state change event
								InventoryUpdate inventoryEvent = new InventoryUpdate();

								// read in the slot number
								// a hero has 6 inventory slots and 6 bank (stash) slots which are only
								//	accessable at one of the stores or by using a courier
								// I'm not really sure what the 13th and 14th slots correspond to (if anything).
								// They might be some sort of buffer for when upgrading items when the bought components
								//	take up more space than the completed item? Maybe?
								attribute = logReader["slot"];
								if (attribute == null) break;
								inventoryEvent.Slot = Convert.ToInt32(attribute);
								if (inventoryEvent.Slot < 0 || inventoryEvent.Slot >= Hero.GetInventorySize()) break;

								// get the unique id of the item
								attribute = logReader["id"];
								if (attribute == null) break;
								id = Convert.ToInt32(attribute);

								// read in the name of the item
								attribute = logReader["item"];
								if (attribute == null) break;
								if (Enum.TryParse(attribute, out type) == true) {
									inventoryEvent.Contents = new Item(type, id);
								}
								else {
									Console.WriteLine("Unrecognized item name: " + attribute);
									break;
								}

								// read in the number of charges (if specified)
								attribute = logReader["charges"];
								if (attribute != null) {
									inventoryEvent.Contents.Charges = Convert.ToInt32(attribute);
								}

								// add the event to the hero's state change list
								target.AddDiff(Tick, inventoryEvent);
								break;
							case "stock_update":
								// a few (5) items are sold from a limited stock in the stores
								// this stock does replenish after a fixed time
								// we keep track of whether (and how many of) each item is avaiable to each team
								// this tag indicates that the stock has changed in a store

								attribute = logReader["team"];	// read in the name of the store's corresponding team
								if (attribute == null) break;
								Store store = null;

								// get a reference to the corresponding store's data structure
								if (attribute == "radiant") {
									store = RadiantStore;
								}
								else if (attribute == "dire") {
									store = DireStore;
								}
								else break;

								// create a new state change event
								StockUpdate stockEvent = new StockUpdate();

								// read in the name of the item whose stock has changed
								attribute = logReader["item"];
								if (attribute == null) break;
								if (Enum.TryParse(attribute, out type) == true) {
									stockEvent.Stock = new Item(type);
								}
								else {
									Console.WriteLine("Unrecognized item name: " + attribute);
									break;
								}


								// read in the number of charges (count of available items in stock)
								attribute = logReader["count"];
								if (attribute == null) break;
								stockEvent.Stock.Charges = Convert.ToInt32(attribute);

								// add the event to the store's list of state changes
								store.AddDiff(Tick, stockEvent);
								break;
							default:	// handle the case of an unrecognized start tag
								Console.WriteLine(Tick + " Unhandled start tag: " + logReader.Name);
								break;
						}
					}
				}
			}

			LastTick = Tick; // keep track of the last tick read in

			InitReplay();

			return true;
		}

		// initialize all the actors for playback of the match
		public void InitReplay() {
			foreach (Hero hero in RadiantTeam) {
				hero.InitReplay();
			}
			foreach (Hero hero in DireTeam) {
				hero.InitReplay();
			}
/*			for (int i = 0; i < 5; i++) {
				RadiantTeam[i].InitReplay();
				DireTeam[i].InitReplay();
			}
*/
			RadiantStore.InitReplay();
			DireStore.InitReplay();

			Tick = FirstTick - 1;
		}

		// run through the state changes one step at a time
		// return false once we've reached the end of playback
		public bool DoTimeStep() {
			Tick++;
			if (Tick > LastTick) return false;

			foreach (Hero hero in RadiantTeam) {
				hero.DoTimeStep(Tick);
			}
			foreach (Hero hero in DireTeam) {
				hero.DoTimeStep(Tick);
			}
/*			for (int i = 0; i < 5; i++) {
				RadiantTeam[i].DoTimeStep(Tick);
				DireTeam[i].DoTimeStep(Tick);
			}
*/
			RadiantStore.DoTimeStep(Tick);
			DireStore.DoTimeStep(Tick);

			return true;
		}

		public List<Hero> GetRadiantTeam() {
			return RadiantTeam;
		}
		public List<Hero> GetDireTeam() {
			return DireTeam;
		}
/*		public Hero[] GetRadiantTeam() {
			return RadiantTeam;
		}
		public Hero[] GetDireTeam() {
			return DireTeam;
		}
*/
		public Store GetRadiantStore() {
			return RadiantStore;
		}
		public Store GetDireStore() {
			return DireStore;
		}

		public int GetTick() {
			return Tick;
		}
		public int GetFirstTick() {
			return FirstTick;
		}
		public int GetLastTick() {
			return LastTick;
		}
	}
}

