using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenBagImageManager2
{
    public class Helpers
    {
        public static IEnumerable<DetectedObject> ParseDetectedObjects(string markup)
        {
            return markup.Split(":", StringSplitOptions.RemoveEmptyEntries)
                .Select(q => DetectedObject.Parse(q))
                .Where(q => q != null)
                .ToList()
                ;
        }
    }
}
