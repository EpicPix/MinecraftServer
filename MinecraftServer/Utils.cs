using System.Numerics;
using System.Security.Cryptography;
using MinecraftServer.Networking;

namespace MinecraftServer;

public class Utils
{
    public static Guid GuidFromString(string str)
    {
        using var md5 = MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(str);
        var hashBytes = md5.ComputeHash(inputBytes);
        hashBytes[6]  &= 0x0f;  /* clear version        */
        hashBytes[6]  |= 0x30;  /* set to version 3     */
        hashBytes[8]  &= 0x3f;  /* clear variant        */
        hashBytes[8]  |= 0x80;  /* set to IETF variant  */
        return new Guid(hashBytes);
    }
    public static string MinecraftShaDigest(byte[] input) 
    {
        var hash = SHA1.HashData(input);
        // Reverse the bytes since BigInteger uses little endian
        Array.Reverse(hash);
        
        BigInteger b = new BigInteger(hash);
        // very annoyingly, BigInteger in C# tries to be smart and puts in
        // a leading 0 when formatting as a hex number to allow roundtripping 
        // of negative numbers, thus we have to trim it off.
        if (b < 0)
        {
            // toss in a negative sign if the interpreted number is negative
            return "-" + (-b).ToString("x").TrimStart('0');
        }
        else
        {
            return b.ToString("x").TrimStart('0');
        }
    }
    
    public static async ValueTask FillBytes(ArraySegment<byte> toRead, DataAdapter stream, CancellationToken ct)
    {
        int remLen = toRead.Count;
        int pos = 0;
        while (remLen > 0)
        {
            int read = await stream.ReadAsync(toRead.Slice(pos, remLen), ct);
            pos += read;
            remLen -= read;
            if (read == 0)
            {
                return;
            }
        }
    }
    
    public static int GetVarIntLength(int value)
    {
        var val = (uint) value;
        var len = 0;
        while (true)
        {
            len++;
            if ((val & ~0x7F) == 0)
            {
                return len;
            }

            val >>= 7;
        }
    }
    
    public static int WriteVarInt(int value, ArraySegment<byte> bytes)
    {
        var val = (uint) value;
        int len = 0;
        while (true)
        {
            if ((val & ~0x7F) == 0)
            {
                bytes[len] = (byte)val;
                len++;
                return len;
            }

            bytes[len] = (byte)((val & 0x7F) | 0x80);
            len++;
            val >>= 7;
        }
    }
}