using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace Dota2Lib
{
	// identifiers for the two teams
	public enum TeamNames {Undefined, Radiant, Dire};

	// ids for different recommended item purchase stages
	public enum ItemRecommendations {Starting, Early, Core, Situational};

	// basic structure for an item
	// Note: it might eventually make more sense to use enums instead of strings for the identifier,
	//	we could then use the enum.ToString() function to get a string if need be
	// Note: since item is currently defined as a struct rather than a class, it will get passed by
	//	value instead of reference (fun with trying to learn C# as I go)
	// Note: ok, for now at least we have replaced this with the Item Class afterall
	//	public struct item
	//	{
	//		public string Name;	// name of the object (used as the item's identifier
	//		public int Charges;	// number of charges the item has remaining - not all items use this variable
	//	}

	// TimeStep = all the state changes associated with a given tick
	// Dota 2 log files record 30 ticks a second
	public class TimeStep
	{
		// the tick (time) at which the recorded changes occur
		public int Tick {get; set;}

		// a list of all the state changes that occur at this tick
		public List<StateChange> Diffs {get; set;}

		public TimeStep() {
			Tick = -1;
			Diffs = new List<StateChange>();
		}
		public TimeStep(int tick) {
			Tick = tick;
			Diffs = new List<StateChange>();
		}
	}

	// probably not the best name for this class, but...
	// entities in the match whose states change over the course of the match
	// currently, we only pay attention to the player-controlled heros and the
	// 	stores for each team (which track the inventory of some items)
	public abstract class Actor
	{
		protected TeamNames Team = TeamNames.Undefined;	// the team with which the actor is affiliated
		protected Item[] Inventory = null;	// the items the actor has stored
		public List<TimeStep> TimeSteps {get; set;}	// a list of state changes the actor undergoes grouped by ticks

		private List<TimeStep>.Enumerator Stepper;	// an enumerated used for iterating through the state changes once initialized


		// a flag indicated whether the enumerator is valid
		// reaching the end or altering the list invalidates the enumerator
		// use InitReplay() to reinitialize the enumerator
		private bool EnumeratorValid = false;

		public Actor()
		{
			TimeSteps = new List<TimeStep>();
		}

		// add a new difference (state change) at a given time step
		public void AddDiff(int tick, StateChange diff) {
			// if the list is empty or the new diff occurs after the last recorded tick
			// add in a new time step for the new tick
			if (TimeSteps.Count == 0 || TimeSteps.Last().Tick < tick) {
				TimeSteps.Add(new TimeStep(tick));
			}

			// add the new state change at the current last tick
			diff.Tick = tick;
			TimeSteps.Last().Diffs.Add(diff);

			// changing the list will render the enumerator invalid
			// set this so we know not to try to use it without initializing it
			EnumeratorValid = false;
		}

		public List<StateChange> GetDiffsByName(string type) {
			List<StateChange> diffList = new List<StateChange>();
			InitReplay();
			while (IsEnumeratorValid() == true && Stepper.Current != null) {
				foreach (StateChange diff in Stepper.Current.Diffs) {
					if (diff.Type == UpdateType.InventoryUpdate) {
						diffList.Add(diff);
					}
				}
				GetNextStep();
			}
			return diffList;
		}

		// get ready to step through the state changes from the beginning by intializing the enumerator
		public void InitReplay() {
			Stepper = TimeSteps.GetEnumerator();
			EnumeratorValid = Stepper.MoveNext();;
		}

		// return whether or not the enumerator is currently valid and safe to use
		public bool IsEnumeratorValid() {
			return EnumeratorValid;
		}

		// return the time step (list of the actor's state changes) at the current point in the iteration
		public TimeStep GetCurrentStep() {
			if (EnumeratorValid == true) {
				return Stepper.Current;
			}

			return null;
		}

		// increment the enumerator to the next recorded time step for this actor
		public TimeStep GetNextStep() {
			if (EnumeratorValid == true) {
				EnumeratorValid = Stepper.MoveNext();
			}

			// check that enumerator is still valid
			// going past the end of the list will invalidate the enumerator
			if (EnumeratorValid == false) {
				return null;
			}

			// incremement the enumerator and return the next (now current) step
			return Stepper.Current;
		}

		public abstract void DoTimeStep(int tick);
	}

	// there are a few (5) items that are only available for purchase in limited amounts to each team
	// after a fixed time the stock of these items replenishes
	// we associate a store with each team to keep track of the availability of these items
	public class Store : Actor
	{
		private const int InventorySize = 5; // the number of slots in the store's inventory
		public Store() {

			// currently there are only 5 items with limited stock in the game
			Inventory = new Item[InventorySize];

			//fred
			/*
			Inventory[0] = new Item(ItemId.gem);
			Inventory[1] = new Item(ItemId.ward_observer);
			Inventory[2] = new Item(ItemId.courier);
			Inventory[3] = new Item(ItemId.flying_courier);
			Inventory[4] = new Item(ItemId.smoke_of_deceit);
			*/
		}

		public Store(TeamNames team) {
			Team = team;

			// currently there are only 5 items with limited stock in the game
			Inventory = new Item[InventorySize];
			//fred
			/*
			Inventory[0] = new Item(ItemId.gem);
			Inventory[1] = new Item(ItemId.ward_observer);
			Inventory[2] = new Item(ItemId.courier);
			Inventory[3] = new Item(ItemId.flying_courier);
			Inventory[4] = new Item(ItemId.smoke_of_deceit);
			*/
		}

		// check whether any state change occurs at the specified tick
		// if there are changes, update the state and increment the enumerator
		public override void DoTimeStep(int tick) {
			// check that we have a valid enumerator
			// also check whether anything happens are the specified tick (if not, there is nothing else to do)
			if (IsEnumeratorValid() == false || GetCurrentStep() == null || GetCurrentStep().Tick > tick) return;

			// if there are state changes for the store at this time,
			//	apply the each change
			foreach (StateChange diff in GetCurrentStep().Diffs) {
				switch (diff.Type) {
					case UpdateType.StockUpdate:
						// currently there is only one kind of state change the stores handle
						//	that is just a change in the available number of one of the tracked items
						for (int i = 0; i < InventorySize; i++) {
							if (((StockUpdate)diff).Stock.Id == Inventory[i].Id) {
								Inventory[i].Charges = ((StockUpdate)diff).Stock.Charges;
								//								Console.WriteLine(tick + ": " + Team.ToString() + " Store: " + Inventory[i].Id + "=" + Inventory[i].Charges);
								break;
							}
						}
						break;
					default:
						break;
				}
			}

			GetNextStep();	// increment the enumerator to point to the next set of state changes
		}

		public int GetInventorySize() {
			return InventorySize;
		}
	}

	// each player in a match controls a Hero character
	public class Hero : Actor
	{
		public int SpawnTime {get; set;}	// the time the hero entered the match
		public string Name {get; set;}		// the name of the hero
		public int Level {get; set;}		// the current level of the hero
		public int MaxHealth {get; set;}	// the current maximum health of the hero
		public int MaxMana {get; set;}		// the current maximum mana of the hero
		public int Health {get; set;}		// the current health of the hero
		public int Mana {get; set;}			// the current mana of the hero
		public int Gold {get; set;}			// the current gold of the hero

		private const int InventorySize = 20; // the number of slots in the hero's inventory

		public ItemType[][] RecommendedItemTable = new ItemType[Enum.GetValues(typeof(ItemRecommendations)).Length][];	// table of recommended items for the hero

		public Hero(string name, TeamNames team) {
			Name = name;
			Team = team;
			// fill in the table of recommended item purchases based on data in the xml file
			// these are the recmmendations displayed in the store UI for each hero in game
			//			GetRecommendedItems();

			InitHero();
		}
		/*
		public Hero(TeamNames team) {
			InitHero();
			Team = team;
		}
*/
		public void InitHero() {
			//			Team = TeamNames.Undefined;
			SpawnTime = -1;
			Level = 1;
			MaxHealth = MaxMana = Health = Mana = Gold = -1;

			// intialize all the inventory slots to "emtpy"
			// not entirely sure how inventory slots are mapped:
			// 0...5 should be the six slots that hold what the hero is carrying / has equiped and can use
			// 6...11 should (I think) be the hero's stash that is stored at the base and hold item bought while in the field
			// 12 and 13 - I have no idea - may be buffer space for when a player upgrades an item without all the base components
			//	in which case the remaining components are automatically purchased - maybe they sometimes go into these slots?
			// 14...19 are not referred to in the game log or the Clarity parser - we are currently using these slots to represent
			//	the case where the item is held in the team courier's inventory slots
			Inventory = new Item[InventorySize];
			for (int i = 0; i < InventorySize; i++) Inventory[i] = null;
		}

		// fill in the table of recommended item purchases based on data in the xml file
		// these are the recmmendations displayed in the store UI for each hero in game
		private void GetRecommendedItems() {
			using (XmlReader logReader = XmlReader.Create("item_recommendations.xml"))
			{
				while (logReader.Read())
				{
					if (logReader.IsStartElement())	// if we have a start tag
					{
						string name;		// the name of a hero
						string[] itemList;	// space delineated list of items
						ItemType type;			// we will attempt to parse item names to get the corresponding ItemType enum value

						// handle the element
						switch (logReader.Name)
						{
							case "dota2_hero_item_data":
								// this just identifies the file containing
								// info about item recommened items for each hero
								break;
							case "hero":
								// check whether we have found the info for this hero
								name = logReader["name"];	// retrieve the name of the hero
								if (name.Equals(Name)) {
									while (logReader.Read()) {
										if (logReader.IsStartElement()) {
											if  (logReader.Name == "hero") { // reached data for another hero
												return;
											}
											else {
												int n = -1;
												if (logReader.Name == "starting") n = (int)ItemRecommendations.Starting;
												else if (logReader.Name == "early") n = (int)ItemRecommendations.Early;
												else if (logReader.Name == "core") n = (int)ItemRecommendations.Core;
												else if (logReader.Name == "situational") n = (int)ItemRecommendations.Situational;

												if (n >= 0 && logReader.Read()) {
													itemList = logReader.Value.Trim().Split(' ');
													RecommendedItemTable[n] = new ItemType[itemList.Length];
													int i = 0;
													foreach (string item in itemList) {
														if (Enum.TryParse(item, out type) == true) {
															RecommendedItemTable[n][i++] = type;
														}
													}
												}
											}
										}
									}
								}
								break;
							default:
								break;
						}
					}
				}
			}
		}

		// check whether any state change occurs at the specified tick
		// if there are changes, update the state and increment the enumerator
		public override void DoTimeStep(int tick) {
			// check that we have a valid enumerator
			// also check whether anything happens are the specified tick (if not, there is nothing else to do)
			if (IsEnumeratorValid() == false || GetCurrentStep() == null || GetCurrentStep().Tick > tick) return;

			// if there are state changes for the store at this time,
			//	apply the each change
			foreach (StateChange diff in GetCurrentStep().Diffs) {
				switch (diff.Type) {
					case UpdateType.HeroSpawn:
						// the hero has entered the match
						// set the starting values for health, mana, and gold
						MaxHealth = Health = ((HeroSpawn)diff).MaxHealth;
						MaxMana = Mana = ((HeroSpawn)diff).MaxMana;
						Gold = ((HeroSpawn)diff).Gold;
						//						Console.WriteLine(tick + ": " + Name + ": spawned");
						break;
					case UpdateType.LevelUp:
						// the hero has gained a level
						// update the values for level and max health and mana
						Level = ((LevelUp)diff).Level;
						MaxHealth = ((LevelUp)diff).MaxHealth;
						MaxMana = ((LevelUp)diff).MaxMana;
						//						Console.WriteLine(tick + ": " + Name + ": reached level " + Level);
						break;
					case UpdateType.StatusUpdate:
						// the hero's current health or mana (or both) changed
						// may eventually track other status changes (e.g., stuns, death), but not now
						//						Console.Write(tick + ": " + Name + ":");
						if (((StatusUpdate)diff).Health != -1) {
							Health = ((StatusUpdate)diff).Health;
							//							Console.Write(" health=" + Health);
						}
						if (((StatusUpdate)diff).Mana != -1) {
							Mana = ((StatusUpdate)diff).Mana;
							//							Console.Write(" mana=" + Mana);
						}
						//						Console.WriteLine("");
						break;
					case UpdateType.GoldUpdate:
						// the hero has spent or earned money
						Gold = ((GoldUpdate)diff).Gold;
						//						Console.WriteLine(tick + ": " + Name + ": gold=" + Gold);
						break;
					case UpdateType.ItemGained:
						// there actually isn't anything we need to do with these at the moment
						// everything is handled by the InventoryUpdate event
						break;
					case UpdateType.ItemLost:
						// there actually isn't anything we need to do with these at the moment
						// everything is handled by the InventoryUpdate event
						break;
					case UpdateType.InventoryUpdate:
						// the hero has bought, picked up, sold, or dropped an item
						// or moved an item between inventory slots
						// or used up some of an item's charges
						int slot = ((InventoryUpdate)diff).Slot;
						if (slot >= 0 && slot < InventorySize) {
							Inventory[slot] = ((InventoryUpdate)diff).Contents;
							if (Inventory[slot] == null) {
								//								Console.Write(tick + ": " + Name + ": slot_" + slot + "=empty");
							}
							else {
								//								Console.Write(tick + ": " + Name + ": slot_" + slot + "=" + Inventory[slot].Id);
								//								Console.Write(" charges=" + Inventory[slot].Charges);
							}
							//							Console.WriteLine("");
						}
						break;
					default:
						Console.WriteLine(tick + ": Unhandled log event: " + diff.Type);
						break;
				}
			}

			GetNextStep();
		}

		static public int GetInventorySize() {
			return InventorySize;
		}

		// set the state of an inventory slot
		public void SetSlot(int index, ItemType type, int id, int charges) {
			if (index < 0 || index >= InventorySize) return;

			Inventory[index].Type = type;
			Inventory[index].Id = id;
			Inventory[index].Charges = charges;
		}
		public Item GetSlot(int index) {
			if (index < 0 || index >= InventorySize) {
				return null;
			}

			return Inventory[index];
		}
	}
}

