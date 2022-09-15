using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
namespace Creaturebook
{
    public class ModConfig
    {
        public bool ShowScientificNames { get; set; } = true;
        public bool ShowDiscoveryDates { get; set; } = true;
        public KeybindList OpenMenuKeybind { get; set; } = KeybindList.Parse("LeftShift + B, LeftControl + B, LeftControl + LeftShift + B");
        public string WayToGetNotebook { get; set; } = "Letter";
    }
    public class ModData
    {
        public IDictionary<string, SDate> DiscoveryDates { get; set; } = new Dictionary<string, SDate>();
        public bool IsNotebookObtained { get; set; } = false;
    }
    public class Chapter
    {
        public int CreatureAmount { get; set; }
        public string ChapterTitle { get; set; }
        public string ChapterFolder { get; set; }
        public string CreatureNamePrefix { get; set; }
        public string Author { get; set; } = "Example Author name for Header Page";
        public bool EnableSets { get; set; } = false;
        public IDictionary<string, string> setsAndIDs { get; set;} = new Dictionary<string, string>();
    }
    public class Creature
    {
        public int ID { get; set; }
        public int OffsetX { get; set; } = 0;
        public int OffsetX_2 { get; set; } = 0;
        public int OffsetX_3 { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
        public int OffsetY_2 { get; set; } = 0;
        public int OffsetY_3 { get; set; } = 0;
        public string Desc { get; set; } = "";
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string LatinName { get; set; } = null;
        public string OverrideDefaultNaming { get; set; } = "";
        public string directory { get; set; }
        public string BelongsToSet { get; set; } = "Other";
        public bool HasExtraImages { get; set; } = false;
        public bool HasScientificName { get; set; } = false;
        public bool HasFunFact { get; set; } = true;
        public float Scale_1 { get; set; } = 1f;
        public float Scale_2 { get; set; } = 1f;
        public float Scale_3 { get; set; } = 1f;
        public IContentPack FromContentPack { get; set; }
    }
}
