using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrockSharp
{


  public static class Crock32
  {

    static char[] crockmap = new char[32] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

    public static byte[] Decode(string src)
    {
      var blocks = (int)((src.Length * 5 + 39) / 40D);
      var buffer = new byte[blocks * 40 / 8];
      var chars = new char[blocks * 8];

      for (var i = 0; i < src.Length; i++)
      {
        chars[i] = src[i];
      }

      var c = chars.Select(x=>getByte(x)).ToArray();

      for (var i = 0; i < blocks; i++)
      {
        var n = 5 * i;
        var p = 8 * i;
        buffer[n + 0] = (byte)(c[p + 0] | ((c[p + 1] & 7) << 5));
        buffer[n + 1] = (byte)(c[p + 1] >> 3 | (c[p + 2] << 2) | ((c[p + 3] & 1) << 7));
        buffer[n + 2] = (byte)((c[p + 3] >> 1) | ((c[p + 4] & 15) << 4));
        buffer[n + 3] = (byte)((c[p + 4] >> 4) | (c[p + 5] << 1) | ((c[p + 6] & 3) << 6));
        buffer[n + 4] = (byte)((c[p + 6] >> 2) | (c[p + 7] << 3));
      }
      
      var len = (int)(src.Length * 5 / 8D);
      var b = new byte[len];
      Array.Copy(buffer, b, len);
      return b;
    }

    public static string Encode(byte[] src)
    {
      var blocks = (int)((src.Length * 8 + 39) / 40D);
      var buffer = new byte[blocks*40/8];

      Array.Copy(src, buffer, src.Length);
      var result = new int[blocks*40/5];

      for (var i = 0; i < blocks; i++)
      {
        var n = 8 * i;
        var p = 5 * i;
        result[n + 0] = buffer[p + 0] & 31;
        result[n + 1] = (buffer[p + 0] >> 5) | ((buffer[p + 1] & 3) << 3);
        result[n + 2] = (buffer[p + 1] >> 2) & 31;
        result[n + 3] = (buffer[p + 1] >> 7) | ((buffer[p + 2] & 15) << 1);
        result[n + 4] = (buffer[p + 2] >> 4) | ((buffer[p + 3] & 1) << 4);
        result[n + 5] = (buffer[p + 3] >> 1) & 31;
        result[n + 6] = (buffer[p + 3] >> 6) | ((buffer[p + 4] & 7) << 2);
        result[n + 7] = (buffer[p + 4] >> 3);
      }

      var chars = result.Select(x => crockmap[x]).ToArray();
      var len = (int)(src.Length * 8 / 5D + 4 / 5D);
      return new string(chars, 0, len);
    }

    public static bool Compare(byte[] one, byte[] two)
    {
      var max = one;
      var min = two;

      if (one.Length != two.Length)
      {

        if (two.Length > one.Length)
        {
          max = two;
          min = one;
        }
        
        for (var i = min.Length; i < max.Length; i++)
        {
          if (max[i] != 0)
          {
            //different lengths are only ok if padded with 0
            return false;
          }
        }
      }

      for (var i = 0; i < min.Length; i++)
      {
        if (one[i] != two[i])
        {
          return false;
        }
      }
      return true;
    }

    private static byte getByte(char c)
    {
      //TODO: replace this function with a lookup
      var b = (byte)c;

      if (b < 48)
      {
        //for the special case when were at the end of a string that was extended to fit our block size
        return 0;
      }
      else if (b <= 57)
      {
        b -= 48;
      }
      else if (b <= 72)
      {
        b -= 55; //65-10
      }
      else if (b <= 75)
      {
        b -= 56; //74-18
      }
      else if (b <= 78)
      {
        b -= 57;//77-20
      }
      else if (b <= 84)
      {
        b -= 58;
      }
      else if (b <= 90)
      {
        b -= 59;//86-27
      }
      else
      {
        
      }
      return b;
    }

    
  }
}
