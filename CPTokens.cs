using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace Creaturebook
{
    internal class DiscoveredCreaturesFromAChapter
    {
        private string prefix = ModEntry.chapterModels[0].CreatureNamePrefix;
        private string b;
        private int c;
        private int d;

        public bool AllowsInput()
        {
            return true;
        }
        public bool CanHaveMultipleValues(string input = null)
        {
            return false;
        }

        public bool IsReady()
        {
            return Context.IsWorldReady;
        }

        public bool UpdateContext()
        {
            if (IsReady())
            {
                foreach (var date in ModEntry.singleModData.DiscoveryDates)
                {
                    foreach (var item in ModEntry.creatures)
                    {
                        if (item.Prefix == b && date.Value != null)
                        {
                            if (date.Key.Contains(prefix))
                            {
                                d++;
                            }
                        }
                    }
                }
                return c != d;
            }
            else           
                return false;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public IEnumerable<string> GetValues(string input)
        {
            b = input;
            if (IsReady() && input != "" && input != null)
            {
                foreach (var date in ModEntry.singleModData.DiscoveryDates)
                {
                    foreach (var item in ModEntry.creatures)
                    {
                        if (item.Prefix == input && date.Value != null)
                        {
                            if (date.Key.Contains(prefix))
                            {
                                c++;
                            }
                        }
                    }
                }
                yield return Convert.ToString(c);
            }
            else
            {
                yield return null;
            }
        }
    }
    internal class IsCreatureDiscovered 
    {
        private string FullID = ModEntry.chapterModels[0].CreatureNamePrefix + "_" + ModEntry.creatures[0].ID.ToString();
        private bool a;
        private bool b;

        public bool AllowsInput()
        {
            return true;
        }
        public bool CanHaveMultipleValues(string input = null)
        {
            return false;
        }

        public bool IsReady()
        {
            return Context.IsWorldReady;
        }

        public bool UpdateContext()
        {
            if (IsReady())
            {
                foreach (var date in ModEntry.singleModData.DiscoveryDates)
                {
                    foreach (var item in ModEntry.creatures)
                    {
                        if (item.Prefix == FullID && date.Value != null)
                        {
                            b = true;
                            break;
                        }
                    }
                    break;
                }
                return a != b;
            }
            else
                return false;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public IEnumerable<string> GetValues(string input)
        {
            if (IsReady())
            {
                foreach (var date in ModEntry.singleModData.DiscoveryDates)
                {
                    foreach (var item in ModEntry.creatures)
                    {
                        if (item.Prefix == FullID && date.Value != null)
                        {
                            a = true;
                            break;
                        }
                    }
                    break;
                }
                yield return Convert.ToString(a);
            }
            else
            {
                yield return null;
            }
        }
    }
}
