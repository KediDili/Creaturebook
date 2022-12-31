using StardewModdingAPI.Utilities;

namespace Creaturebook.Framework.Models
{
    public class ModConfig
    {
        public bool ShowScientificNames { get; set; } = true;
        public bool ShowDiscoveryDates { get; set; } = true;
        public KeybindList OpenMenuKeybind { get; set; } = KeybindList.Parse("LeftControl + LeftShift + B");
        public string WayToGetNotebook { get; set; } = "Letter";
        public bool EnableStickies { get; set; } = true;
    }
}
