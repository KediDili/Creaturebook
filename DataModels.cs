using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Netcode;

namespace Creaturebook
{
    public class ModData
    {
        public IDictionary<string, SDate> DiscoveryDates { get; set; } = new Dictionary<string, SDate>();
        public bool IsNotebookObtained { get; set; } = false;
    }
    public class ChapterModel
    {
        public int CreatureAmount { get; set; }
        public string ChapterTitle { get; set; }
        public string CreatureNamePrefix { get; set; }
        public string Author { get; set; } = "Example Author name for Header Page";
    }
    public class CreatureModel
    {
        public int CreatureID { get; set; }
        public bool HasExtraImages { get; set; } = false;
        public bool HasScientificName { get; set; } = true;
        public bool HasFunFact { get; set; } = true;
        public string ScientificName { get; set; }
        public float ImageScale_1 { get; set; } = 1f;
        public float ImageScale_2 { get; set; }
        public float ImageScale_3 { get; set; }
        public int OffsetX { get; set; } = 0;
        public int OffsetX_2 { get; set; } = 0;
        public int OffsetX_3 { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
        public int OffsetY_2 { get; set; } = 0;
        public int OffsetY_3 { get; set; } = 0;
    }
    public class Creature
    {
        public int ID { get; set; }
        public Texture2D Image_1 { get; set; }
        public Texture2D Image_2 { get; set; } = null;
        public Texture2D Image_3 { get; set; } = null;
        public string Desc { get; set; } = "";
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string LatinName { get; set; } = null;
        public string FromContentPack { get; set; }
        public float Scale_1 { get; set; } = 1f;
        public float Scale_2 { get; set; } = 1f;
        public float Scale_3 { get; set; } = 1f;
        public int OffsetX { get; set; } = 0;
        public int OffsetX_2 { get; set; } = 0;
        public int OffsetX_3 { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
        public int OffsetY_2 { get; set; } = 0;
        public int OffsetY_3 { get; set; } = 0;
    }
}
