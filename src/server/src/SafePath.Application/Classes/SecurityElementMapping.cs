using Itinero.SafePath;
using System.Collections.Generic;

namespace SafePath.Classes
{
    /// <summary>
    /// Class representing an element in the map
    /// that can be used to calculate the security 
    /// score.
    /// </summary>
    public class SecurityElementMapping
    {
        public SecurityElementTypes Element { get; set; }

        public IList<ValueItem> Values { get; set; }

        public class ValueItem
        {
            public string Key { get; set; }

            public List<string> Values { get; set; }
        }
    }
}
