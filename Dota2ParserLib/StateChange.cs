﻿using System;

namespace Dota2Lib
{
	// list of the different types of state change events we can handle
	public enum UpdateType {Undefined, HeroSpawn, HeroRespawn, HeroDeath, LevelUp, StatusUpdate, GoldUpdate, 
							LocationUpdate, ChargeUpdate, ItemGained, ItemLost, InventoryUpdate, StockUpdate, 
							BuildingSpawn, BuildingUpdate, BuildingKilled};

	// base abstract class for all state change classes
	public abstract class StateChange
	{
		public UpdateType Type {get; set;}	// identifier for the type of state change represented
		public int Tick {get; set;}	// the tick at which the change occurs

		public StateChange() {
			Tick = -1;
		}
	}

	// state change for a hero first entering a match
	public class HeroSpawn : StateChange
	{
		// hero's initial health, mana, and gold levels
		public int MaxHealth {get; set;}
		public int MaxMana {get; set;}
		public int Gold {get; set;}
		public int CellX {get; set;}
		public int CellY {get; set;}

		public HeroSpawn() {
			Type = UpdateType.HeroSpawn;
			MaxHealth = MaxMana = Gold = CellX = CellY = -1;
		}
		public HeroSpawn(int maxHealth, int maxMana, int gold, int cellX, int cellY) {
			Type = UpdateType.HeroSpawn;
			MaxHealth = maxHealth;
			MaxMana = maxMana;
			Gold = gold;
			CellX = cellX;
			CellY = cellY;
		}
	}

	// state change for a hero respawning after a death
	public class HeroRespawn : StateChange
	{
		public HeroRespawn() {
			Type = UpdateType.HeroRespawn;
		}
	}

	// state change for a hero getting killed
	public class HeroDeath : StateChange
	{
		public HeroDeath() {
			Type = UpdateType.HeroDeath;
		}
	}

	// state change for a hero moving to a new map cell
	public class LocationUpdate : StateChange
	{
		public int CellX {get; set;}
		public int CellY {get; set;}

		public LocationUpdate() {
			Type = UpdateType.LocationUpdate;
			CellX = CellY = -1;
		}
	}


	// state change for an npc building (tower, filler, ancient, barracks) spawning
	public class BuildingSpawn : StateChange
	{
		public BuildingNames Name {get; set;}
		public int MaxHealth {get; set;}
		public int CellX {get; set;}
		public int CellY {get; set;}

		public BuildingSpawn() {
			Type = UpdateType.BuildingSpawn;
			MaxHealth = CellX = CellY = -1;
		}
	}

	// state change for an npc building (tower, filler, ancient, barracks) getting damaged
	public class BuildingUpdate : StateChange
	{
		public BuildingNames Name {get; set;}
		public int Health {get; set;}

		public BuildingUpdate() {
			Type = UpdateType.BuildingUpdate;
			Health = -1;
		}
	}

	// state change for an npc building (tower, filler, ancient, barracks) getting killed (zero health)
	public class BuildingKilled : StateChange
	{
		public BuildingNames Name {get; set;}

		public BuildingKilled() {
			Type = UpdateType.BuildingKilled;
		}
	}

	// state change for a hero gaining a level
	public class LevelUp : StateChange
	{
		// hero's new level and maximum health and mana values
		public int Level {get; set;}
		public int MaxHealth {get; set;}
		public int MaxMana {get; set;}

		public LevelUp() {
			Type = UpdateType.LevelUp;
			Level = MaxHealth = MaxMana = -1;
		}
		public LevelUp(int level, int maxHealth, int maxMana) {
			Type = UpdateType.LevelUp;
			Level = level;
			MaxHealth = maxHealth;
			MaxMana = maxMana;
		}
	}

	// state change for a hero's current health and mana changes
	// e.g., from damage, healing, casting a spell, using an item
	// might one day also record other status changes (e.g., stuns, death)
	public class StatusUpdate : StateChange
	{
		// hero's new health and/or mana values
		// (might only record a change in one or the other)
		public int Health {get; set;}
		public int Mana {get; set;}

		public StatusUpdate() {
			Type = UpdateType.StatusUpdate;
			Health = Mana = -1;
		}
		public StatusUpdate(int health, int mana) {
			Type = UpdateType.StatusUpdate;
			Health = health;
			Mana = mana;
		}
	}

	// state change for the hero's current total money
	// note: Dota 2 keeps track of 2 differnt types of gold
	//	for now, we are only concerned with the sum of these,
	//	so we aren't tracking them individually
	public class GoldUpdate : StateChange
	{
		public int Gold {get; set;}

		public GoldUpdate() {
			Type = UpdateType.GoldUpdate;
			Gold = -1;
		}
		public GoldUpdate(int gold) {
			Type = UpdateType.GoldUpdate;
			Gold = gold;
		}
	}

	// state change for an item in the hero's inventory
	// the number of charges the item has available has changed
	public class ChargeUpdate : StateChange
	{
		public int ItemId {get; set;}
		public int NewChargeCount {get; set;}

		public ChargeUpdate() {
			Type = UpdateType.ChargeUpdate;
			ItemId = -1;
			NewChargeCount = -1;
		}
		public ChargeUpdate(int itemId, int count) {
			Type = UpdateType.ChargeUpdate;
			ItemId = itemId;
			NewChargeCount = count;
		}
	}

	// state change for the hero purchasing an item from one of the stores
	// actually, not much of a state change on its own, we handle most of the
	// changes with the inventory update event
	// but it can be hard to tell from an inventory update where the item came from
	// the purchase event makes it clear that the hero just bought a new item
	// Note: one complication is that a bottle purchased remotely may not go into the hero's stash
	//	(the stash is treated as inventory slots > 5), but instead seems to go into the courier's inventory
	//	(at least under some circumstances), to facilitate "bottle crowing"
	// Note: "purchases" can also be delayed (I think), e.g., component items are purchased but spread across
	//	inventories (some items in stash, some with hero) - then the upgraded item will not appear until the
	//	two inventories are combined (e.g., returning to base, use of courier) - then the components are combined.
	//	At this point however, there may be no gold spent. So we are (at least for now) renaming this event to
	//	"ItemGained" - it should correspond to the creation of a new item entity as seen in the log parser
//	public class ItemPurchase : StateChange
	public class ItemGained : StateChange
	{
		public Item NewItem {get; set;}	// the item the hero just bought
		public bool WasGift {get; set;} // indicates whether the hero was given (or stole) the item if true

//		public ItemPurchase() {
		public ItemGained() {
//			Type = UpdateType.ItemPurchase;
			Type = UpdateType.ItemGained;
			NewItem = null;
		}

//		public ItemPurchase(Item newItem) {
		public ItemGained(Item newItem) {
//			Type = UpdateType.ItemPurchase;
			Type = UpdateType.ItemGained;
			NewItem = newItem;
			WasGift = false;
		}

		public ItemGained(Item newItem, bool wasGift) {
			//			Type = UpdateType.ItemPurchase;
			Type = UpdateType.ItemGained;
			NewItem = newItem;
			WasGift = wasGift;
		}
	}

	// state change for an item instance owned by the hero being destroyed
	// e.g., sold, consumed, combined as part of an upgrade
	// (I think) does not apply to the (rare) cases of an item being traded, but not sure
	// A complication may result from not knowing which instance is lost if the inventory contains
	//	more than one item of the same type
	public class ItemLost : StateChange
	{
		public ItemType LostItemType {get; set;} // the type of the item just lost
		public int LostItemId {get; set;}	// the id of the item just lost

		public ItemLost() {
			Type = UpdateType.Undefined;
			LostItemId = -1;
		}

		public ItemLost(ItemType itemType, int id) {
			Type = UpdateType.ItemLost;
			LostItemType = itemType;
			LostItemId = id;
		}
	}

	// state change for a change in the items stored in the hero's inventory / stash
	// a lot of things could change this:
	// buying, selling, dropping, picking up, trading, using an item's charges, consuming an item, etc.
	public class InventoryUpdate : StateChange
	{
		public int Slot {get; set;}	// the index of the slot in inventory that changed
//		public Item Contents {get; set;}	// the contents of the slot
		public ItemType ContentsType {get; set;}	// the type of the item in the slot
		public int ContentsId {get; set;}	// the id of the item in the slot
		public int ContentsCharges {get; set;}	// the (optional) number of charges of the item in the slot

		public InventoryUpdate() {
			Type = UpdateType.InventoryUpdate;
			Slot = -1;
//			Contents = null;
			ContentsType = ItemType.empty;
			ContentsId = -1;
			ContentsCharges = 0;
		}
		public InventoryUpdate(int slot, ItemType contentsType, int contentsId) {
			Type = UpdateType.InventoryUpdate;
			Slot = slot;
			ContentsType = contentsType;
			ContentsId = contentsId;
			ContentsCharges = 0;
		}
//		public InventoryUpdate(int slot, Item contents) {
//			Type = UpdateType.InventoryUpdate;
//			Slot = slot;
//			Contents = contents;
//		}
	}

	// state change for an update to the number of certain items available for purchase
	// 	in one of the team's stores
	// a few (5) items are only avaiable in limited quantities that replenish over time
	public class StockUpdate : StateChange
	{
		public Item Stock {get; set;}

		public StockUpdate() {
			Type = UpdateType.StockUpdate;
			Stock = null;
		}
		public StockUpdate(Item stock) {
			Type = UpdateType.InventoryUpdate;
			Stock = stock;
		}
	}
}

