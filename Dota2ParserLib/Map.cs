using System;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace Dota2Lib
{
	// need to work on these enums: redundant
	public enum Locations {undefined, radiant_side, dire_side, river, radiant_fountain, dire_fountain, radiant_base, dire_base,
		side_shop, secret_shop, top_rune, bottom_rune, radiant_jungle, dire_jungle, top, mid, bot, tower1, tower2, tower3, tower4};

	public enum TerritoryVals {radiant = 255, dire = 200, river = 150, undefined = 0};
	public enum RegionVals {radiant_base = 255, dire_base = 50, top = 200, mid = 150, bottom = 100, undefined = 0};
	public enum LandmarkVals {hero_spawn = 255, tower = 200, jungle = 150, shop = 100, rune = 50, undefined = 0};

	public class Map
	{
		Assembly _assembly;
		Stream _imageStream;

		Bitmap ColorMap;

		public Map()
		{
			try {
				_assembly = Assembly.GetExecutingAssembly();
				_imageStream = _assembly.GetManifestResourceStream("Dota2Lib.dota_color_map4_scaled.png");
			}
			catch {
				Console.WriteLine("Error: could not access embedded color map resource.");
			}
				
			ColorMap = new Bitmap(_imageStream);
		}

		public TerritoryVals GetTerritory(int x, int y) {
			if (x < 0 || x > ColorMap.Width || y < 0 || y > ColorMap.Height) {
				return TerritoryVals.undefined;
			}

			int r = ColorMap.GetPixel(x, y).R;

			if (Enum.IsDefined(typeof(TerritoryVals), r) == true) {
				return (TerritoryVals)r;
			}
			else return TerritoryVals.undefined;
		}

		public RegionVals GetRegion(int x, int y) {
			if (x < 0 || x > ColorMap.Width || y < 0 || y > ColorMap.Height) {
				return RegionVals.undefined;
			}

			int r = ColorMap.GetPixel(x, y).G;

			if (Enum.IsDefined(typeof(RegionVals), r) == true) {
				return (RegionVals)r;
			}
			else return RegionVals.undefined;
		}

		public LandmarkVals GetLandmark(int x, int y) {
			if (x < 0 || x > ColorMap.Width || y < 0 || y > ColorMap.Height) {
				return LandmarkVals.undefined;
			}

			int r = ColorMap.GetPixel(x, y).B;

			if (Enum.IsDefined(typeof(LandmarkVals), r) == true) {
				return (LandmarkVals)r;
			}
			else return LandmarkVals.undefined;
		}
	}
}
	