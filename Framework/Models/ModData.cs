using System.Collections.Generic;
using StardewModdingAPI.Utilities;

namespace Creaturebook.Framework.Models
{
    public class ModData
    {
        public IDictionary<string, SDate> DiscoveryDates { get; set; } = new Dictionary<string, SDate>();
        public bool IsNotebookObtained { get; set; } = false;
    }
}
