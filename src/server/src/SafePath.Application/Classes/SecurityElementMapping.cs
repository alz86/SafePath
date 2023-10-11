using Itinero.SafePath;
using System.Collections.Generic;

namespace SafePath.Classes
{
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
