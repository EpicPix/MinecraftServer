namespace MinecraftServer;

public class Utils
{


    public static Guid GuidFromString(string str)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(str);
        var hashBytes = md5.ComputeHash(inputBytes);
        hashBytes[6]  &= 0x0f;  /* clear version        */
        hashBytes[6]  |= 0x30;  /* set to version 3     */
        hashBytes[8]  &= 0x3f;  /* clear variant        */
        hashBytes[8]  |= 0x80;  /* set to IETF variant  */
        return new Guid(hashBytes);
    }
    
}