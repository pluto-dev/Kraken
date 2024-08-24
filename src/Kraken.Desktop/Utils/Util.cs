using System;
using System.Linq;

namespace Kraken.Desktop.Utils;

public class Util
{
    public static string BaseifyColorsArray(byte[][] array)
    {
        var flattenedArray = array.SelectMany(b => b).ToArray();
        return Convert.ToBase64String(flattenedArray);
    }

    public static byte[][] UnBaseifyColorsArray(string base64String)
    {
        var flattenedArray = Convert.FromBase64String(base64String);
        var baseLength = flattenedArray.Length / 4;
        return Enumerable
            .Range(0, baseLength)
            .Select(i => flattenedArray.Skip(i * 4).Take(4).ToArray())
            .ToArray();
    }
}
