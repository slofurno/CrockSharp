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
    /* crock[0]=0
     * crock[10]=A
     * crock[18]=J -i
     * crock[20]=M -l
     * crock[22]=P -o
     * crock[27]=V
     * */

    /*
     *multiple of 5 bits... and 8 bits per byte 
     *pad to be multiple of 40 bits
     * 
     * 
     * 
     * */

    public static byte[] Decode(string src)
    {
      var blocks = (int)((src.Length * 5 + 39) / 40D);
      var buffer = new byte[blocks * 40 / 8];

      var chars = src.ToCharArray();
      var c = chars.Select(x=>TestByte(x)).ToArray();

      for (var i = 0; i < blocks; i++)
      {

        buffer[0] = (byte)(c[0] | ((c[1] & 7) << 5));
        buffer[1] = (byte)(c[1] >> 3 | (c[2] << 2) | ((c[3] & 1)<<7));
        buffer[2] = (byte)((c[3] >> 1) | ((c[4] & 15) << 4));
        buffer[3] = (byte)((c[4] >> 4) | (c[5] << 1) | ((c[6] & 3) << 6));
        buffer[4] = (byte)((c[6] >> 2) | (c[7] << 3));

      }

        return buffer;

    }

    public static string Encode(byte[] src)
    {

      var blocks = (int)((src.Length * 8 + 39) / 40D);
      var buffer = new byte[blocks*40/8];

      Array.Copy(src, buffer, src.Length);

      var result = new int[blocks*40/5];

      for (var i = 0; i < blocks; i++)
      {
        result[0] = buffer[0] & 31;
        result[1] = (buffer[0] >> 5) | ((buffer[1] & 3) << 3);
        result[2] = (buffer[1] >> 2) & 31;
        result[3] = (buffer[1] >> 7) | ((buffer[2] & 15) << 1);
        result[4] = (buffer[2] >> 4) | ((buffer[3] & 1) << 4);
        result[5] = (buffer[3] >> 1) & 31;
        result[6] = (buffer[3] >> 6) | ((buffer[4] & 7) << 2);
        result[7] = (buffer[4] >> 3);
      }

      var chars = result.Select(x => crockmap[x]).ToArray();
      return new string(chars);


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

    public static byte TestByte(char c)
    {
      var b = (byte)c;

      if (b <= 57)
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
        //throw exception
      }
      return b;
    }

    public unsafe static byte[] DecodeUnsafe(string input)
    {
      var len = input.Length;
      var rem = len % 8;

      if (rem > 0)
      {
        char[] zeroes = Enumerable.Repeat('0', 8 - rem).ToArray();
        input = new string(zeroes) + input;
      }

      len = input.Length;
      var byteCount = (len * 5) / 8;
      var bytes = new byte[byteCount];
      var count = len - 8;

      fixed (byte* start = &bytes[0])
      {
        byte* current = start;

        while (count >= 0)
        {
          ulong temp = 0;
          ulong* chunk = (ulong*)current;
          
          for (var i = 0; i < 8; i++)
          {
            var nextChar = input[count + i];
            temp <<= 5;
            temp |= TestByte(nextChar);
          }

          *chunk |= temp;
          current += 5;
          count -= 8;
        }

      }
      return bytes;
    }

    public static byte getByte(char c)
    {
      return (byte)5;
    }

    public unsafe static string EncodeUnsafe(byte[] input)
    {
      var len = input.Length;
      var rem = len % 5;

      if (rem > 0)
      {
        var zeroes = new byte[5 - rem];
        zeroes = zeroes.Select(x => (byte)0).ToArray();
        input = input.Concat(zeroes).ToArray();
      }

      len = input.Length;
      var wordCount = (8 * len) / 5;
      var words = new uint[wordCount];
      var count = 0;

      fixed (byte* inputp = &input[0])
      {
        var start = inputp;

        while (count < wordCount)
        {
          var chunk = (ulong*)start;
          words[count] = (uint)*chunk & 31;
          words[count + 1] = (uint)((*chunk >> 5) & 31);
          words[count + 2] = (uint)((*chunk >> 10) & 31);
          words[count + 3] = (uint)((*chunk >> 15) & 31);
          words[count + 4] = (uint)((*chunk >> 20) & 31);
          words[count + 5] = (uint)((*chunk >> 25) & 31);
          words[count + 6] = (uint)((*chunk >> 30) & 31);
          words[count + 7] = (uint)((*chunk >> 35) & 31);

          start += 5;
          count += 8;
        }
      }

      while (wordCount > 0 && words[wordCount - 1] == 0)
      {
        wordCount--;
      }

      words = words.Take(wordCount).Reverse().ToArray();
      var output = words.Select(x => crockmap[x]).ToArray();
      return new string(output);
    }
  }
}
