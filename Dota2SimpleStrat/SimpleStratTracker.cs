using System;

using Dota2Lib;

namespace Dota2SimpleStrat
{
	class MainClass
	{
		static Match MyMatch = new Match();

		static Locations[] RadiantLocations = new Locations[5];
		static Locations[] DireLocations = new Locations[5];
		static int AliveRadiantTowers = 11;
		static int AliveDireTowers = 11;

		static int Tick = 0;

		static bool Pause = false;

		public static void Main(string[] args)
		{
			MyMatch.Init("log.xml");

			foreach (Hero hero in MyMatch.GetRadiantTeam()) {
				hero.InitReplay();
			}
			foreach (Hero hero in MyMatch.GetDireTeam()) {
				hero.InitReplay();
			}

			MyMatch.GetBuildingManager().InitReplay();

			int heroOffset;
			Locations location = Locations.undefined;


			Tick = MyMatch.GetFirstTick();

			// NOTE: the locations from the log file are offset by 64 units in both x and y directions
			// not sure why, but they are

			// skip ahead to the point where all 10 heroes have spawned
			int spawnCount = 0;
			while (Tick <= MyMatch.GetLastTick() && spawnCount < 10) {
				// check radiant heroes
				heroOffset = 0;
				foreach (Hero hero in MyMatch.GetRadiantTeam()) {
					if (hero.GetCurrentStep() != null && hero.GetCurrentStep().Tick == Tick) {
						foreach (StateChange diff in hero.GetCurrentStep().Diffs) {
							if (diff.Type == UpdateType.HeroSpawn) {
								location = MyMatch.GetLocation(((HeroSpawn)diff).CellX - 64, ((HeroSpawn)diff).CellY - 64);
								RadiantLocations[heroOffset] = location;
								spawnCount++;
							}
						}
						hero.DoTimeStep(Tick);
					}
					heroOffset++;
				}

				// check dire heroes
				heroOffset = 0;
				foreach (Hero hero in MyMatch.GetDireTeam()) {
					if (hero.GetCurrentStep() != null && hero.GetCurrentStep().Tick == Tick) {
						foreach (StateChange diff in hero.GetCurrentStep().Diffs) {
							if (diff.Type == UpdateType.HeroSpawn) {
								location = MyMatch.GetLocation(((HeroSpawn)diff).CellX - 64, ((HeroSpawn)diff).CellY - 64);
								DireLocations[heroOffset] = location;
								spawnCount++;
							}
						}
						hero.DoTimeStep(Tick);
					}
					heroOffset++;
				}

				// check building manager
				if (MyMatch.GetBuildingManager().GetCurrentStep() != null && MyMatch.GetBuildingManager().GetCurrentStep().Tick == Tick) {
					MyMatch.GetBuildingManager().DoTimeStep(Tick);
				}

				Tick++;
			}

			UpdateConsole();

			// run through the rest of the match
			bool updateNeeded = false;
			while (Tick <= MyMatch.GetLastTick()) {
				// check radiant heroes
				heroOffset = 0;
				foreach (Hero hero in MyMatch.GetRadiantTeam()) {
					if (hero.GetCurrentStep() != null && hero.GetCurrentStep().Tick == Tick) {
						foreach (StateChange diff in hero.GetCurrentStep().Diffs) {
							if (diff.Type == UpdateType.LocationUpdate) {
								location = MyMatch.GetLocation(((LocationUpdate)diff).CellX - 64, ((LocationUpdate)diff).CellY - 64);
								if (RadiantLocations[heroOffset] != location) {
									updateNeeded = true;
									RadiantLocations[heroOffset] = location;
								}
							}
						}
						hero.DoTimeStep(Tick);
					}
					heroOffset++;
				}

				// check dire heroes
				heroOffset = 0;
				foreach (Hero hero in MyMatch.GetDireTeam()) {
					if (hero.GetCurrentStep() != null && hero.GetCurrentStep().Tick == Tick) {
						foreach (StateChange diff in hero.GetCurrentStep().Diffs) {
							if (diff.Type == UpdateType.LocationUpdate) {
								location = MyMatch.GetLocation(((LocationUpdate)diff).CellX - 64, ((LocationUpdate)diff).CellY - 64);
								if (DireLocations[heroOffset] != location) {
									updateNeeded = true;
									DireLocations[heroOffset] = location;
								}
							}
						}
						hero.DoTimeStep(Tick);
					}
					heroOffset++;
				}

				// check building manager
				if (MyMatch.GetBuildingManager().GetCurrentStep() != null && MyMatch.GetBuildingManager().GetCurrentStep().Tick == Tick) {
					foreach (StateChange diff in MyMatch.GetBuildingManager().GetCurrentStep().Diffs) {
						if (diff.Type == UpdateType.BuildingKilled) {
							if (((BuildingKilled)diff).Name.ToString().Contains("tower")) {
								if (((BuildingKilled)diff).Name.ToString().Contains("goodguys")) {
									AliveRadiantTowers--;
								}
								else {

									AliveDireTowers--;
								}
								Pause = true;
								updateNeeded = true;
							}
						}
					}
					MyMatch.GetBuildingManager().DoTimeStep(Tick);
				}

				if (updateNeeded == true) {
					UpdateConsole();
					updateNeeded = false;
				}

				Tick++;
				if (Tick >= 232) {
					int fred = 0;
				}
			}
		}

		static void UpdateConsole() {
			int radiantTopCount = 0;
			int direTopCount = 0;
			int radiantMidCount = 0;
			int direMidCount = 0;
			int radiantBotCount = 0;
			int direBotCount = 0;
			int radiantJungleCount = 0;
			int direJungleCount = 0;
			int radiantHomeCount = 0;
			int direHomeCount = 0;
			int radiantEnemyCount = 0;
			int direEnemyCount = 0;
			int radiantOtherCount = 5;
			int direOtherCount = 5;

			for (int i = 0; i < 5; i++) {
				if (RadiantLocations[i] == Locations.top) radiantTopCount++;
				else if (RadiantLocations[i] == Locations.mid) { radiantMidCount++; radiantOtherCount--; }
				else if (RadiantLocations[i] == Locations.bot) { radiantBotCount++; radiantOtherCount--; }
				else if (RadiantLocations[i] == Locations.radiant_jungle) { radiantJungleCount++; radiantOtherCount--; }
				else if (RadiantLocations[i] == Locations.radiant_base) { radiantHomeCount++; radiantOtherCount--; }
				else if (RadiantLocations[i] == Locations.dire_base) { radiantEnemyCount++; radiantOtherCount--; }

				if (DireLocations[i] == Locations.top) { direTopCount++; direOtherCount--; }
				else if (DireLocations[i] == Locations.mid) { direMidCount++; direOtherCount--; }
				else if (DireLocations[i] == Locations.bot) { direBotCount++; direOtherCount--; }
				else if (DireLocations[i] == Locations.dire_jungle) { direJungleCount++; direOtherCount--; }
				else if (DireLocations[i] == Locations.dire_base) { direHomeCount++; direOtherCount--; }
				else if (DireLocations[i] == Locations.dire_base) { direEnemyCount++; direOtherCount--; }
			}

			Console.SetCursorPosition(0, 0);
			Console.WriteLine("\t\tRadiant\t\t\tDire\t\tTick = " + Tick);
			Console.WriteLine("towers: \t" + AliveRadiantTowers + "  \t\t\t" + AliveDireTowers + "  ");
			Console.WriteLine("top: \t\t" + radiantTopCount + "\t\t\t" + direTopCount);
			Console.WriteLine("mid: \t\t" + radiantMidCount + "\t\t\t" + direMidCount);
			Console.WriteLine("bot: \t\t" + radiantBotCount + "\t\t\t" + direBotCount);
			Console.WriteLine("jungle: \t" + radiantJungleCount + "\t\t\t" + direJungleCount);
			Console.WriteLine("home_base: \t" + radiantHomeCount + "\t\t\t" + direHomeCount);
			Console.WriteLine("enemy_base: \t" + radiantEnemyCount + "\t\t\t" + direEnemyCount);
			Console.WriteLine("other: \t\t" + radiantOtherCount + "\t\t\t" + direOtherCount);

			if (Pause == true) {
				Console.Write("Press any key ('q' to quit): ");
				ConsoleKeyInfo keyPress = Console.ReadKey();
				if (keyPress.KeyChar == 'q' || keyPress.KeyChar == 'Q') {
					Environment.Exit(0);
				}
				Pause = false;
			}
		}
	}
}
