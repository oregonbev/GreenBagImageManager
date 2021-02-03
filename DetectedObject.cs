using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GreenBagImageManager2
{
    public class DetectedObject
    {

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Class { get; set; }

        /// <summary>
        ///  The degree of confidence in the detection
        /// </summary>
        public double Confidence { get; set; }
        public bool IsCounted { get; set; }

        public override string ToString()
        {
            return $"{Class} {X} {Y} {Width} {Height} {IsCounted}";
        }


        public static DetectedObject Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var parts = value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            // 5 or 6 
            if (parts.Length < 5 || parts.Length > 6) return null;

            var obj = new DetectedObject();
            obj.Class = parts[0];

            if (int.TryParse(parts[1], out int x))
                obj.X = x;
            else
                return null;

            if (int.TryParse(parts[2], out int y))
                obj.Y = y;
            else
                return null;

            if (int.TryParse(parts[3], out int w))
                obj.Width = w;
            else
                return null;

            if (int.TryParse(parts[4], out int h))
                obj.Height = h;
            else
                return null;

            obj.IsCounted = true;
            if (parts.Length == 6)
            {
                if (bool.TryParse(parts[5], out bool counted))
                    obj.IsCounted = counted;
                else
                    obj.IsCounted = false;
            }

            return obj;

        }

    }
}
