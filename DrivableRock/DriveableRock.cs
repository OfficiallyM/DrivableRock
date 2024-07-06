using System.Linq;
using TLDLoader;
using UnityEngine;
using DrivableRock.Components;

namespace DrivableRock
{
	public class DrivableRock : Mod
	{
		// Mod meta stuff.
		public override string ID => "M_DrivableRock";
		public override string Name => "Drivable Rock";
		public override string Author => "M-";
		public override string Version => "0.1.0";
		public override bool LoadInDB => true;

		public static int powerMultiplier = 1;

		public override void dbLoad()
		{
			GameObject rock = mainscript.M.terrainGenerationSettings.objGeneration.objTypes.Where(o => o.prefab.name == "KO02").FirstOrDefault()?.prefab;
			if (rock != null && rock.GetComponent<Drivable>() == null)
				rock.AddComponent<Drivable>();
		}

#if DEBUG
        public override void Config()
		{
			SettingAPI settings = new SettingAPI(this);
			powerMultiplier = Mathf.RoundToInt(settings.GUISlider("Power multiplier", powerMultiplier, 1, 100, 0, 30));
		}
#endif
    }
}
