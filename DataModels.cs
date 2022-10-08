using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
namespace Creaturebook
{
    public class ModConfig
    {
        public bool ShowScientificNames { get; set; } = true;
        public bool ShowDiscoveryDates { get; set; } = true;
        public KeybindList OpenMenuKeybind { get; set; } = KeybindList.Parse("LeftControl + LeftShift + B");
        public string WayToGetNotebook { get; set; } = "Letter";
        public bool EnableStickies { get; set; } = true;
    }
    public class ModData
    {
        public IDictionary<string, SDate> DiscoveryDates { get; set; } = new Dictionary<string, SDate>();
        public bool IsNotebookObtained { get; set; } = false;
    }

    public struct Chapter
    {
        public string Title;

        public string Folder;

        public string CreatureNamePrefix;

        public string Category;

        public IContentPack FromContentPack;

        public List<Creature> Creatures;

        public List<Set> Sets;

        public string Author;

        public bool EnableSets;
    }
    public struct Set
    {
        public string InternalName;

        public string DisplayNameKey;

        public int[] CreaturesBelongingToThisSet;

        public int DiscoverWithThisItem;
    }
    public struct Creature
    {
        public int ID;
        
        public string Name;
        
        public string ScientificName;
        
        public int OffsetX;
        
        public int OffsetX_2;

        public int OffsetX_3;

        public int OffsetY;

        public int OffsetY_2;

        public int OffsetY_3;

        public int UseThisItem;

        public string Desc;

        public string OverrideDefaultNaming;

        public string Directory;

        public bool HasExtraImages;

        public bool HasScientificName;

        public bool HasFunFact;

        public float Scale_1;

        public float Scale_2;

        public float Scale_3;
    }
}
