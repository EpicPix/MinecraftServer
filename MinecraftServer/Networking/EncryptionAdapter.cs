using System.Security.Cryptography;

namespace MinecraftServer.Networking;

public class EncryptionAdapter : DataAdapter
{
    public Stream ReadStream { get; }
    public Stream WriteStream { get; }

    private Aes GetInstance(byte[] key)
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CFB;
        aes.Padding = PaddingMode.None;
        aes.KeySize = 128;
        aes.FeedbackSize = 8;
        aes.Key = key;
        aes.IV = key;
        return aes;
    }
    public EncryptionAdapter(DataAdapter baseAdapter, byte[] key)
    {
        ReadStream = new CryptoStream(baseAdapter, GetInstance(key).CreateDecryptor(), CryptoStreamMode.Read);
        WriteStream = new CryptoStream(baseAdapter, GetInstance(key).CreateEncryptor(), CryptoStreamMode.Write);
        BaseAdapter = baseAdapter;
    }

    public override void Close()
    {
        ReadStream.Close();
        WriteStream.Close();
        base.Close();
    }

    public override async ValueTask DisposeAsync()
    {
        await ReadStream.DisposeAsync();
        await WriteStream.DisposeAsync();
    }

    public override void Flush()
    {
        ReadStream.Flush();
        WriteStream.Flush();
    }

    public DataAdapter BaseAdapter { get; }

    public override ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        return ReadStream.ReadAsync(buf, ct);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        return WriteStream.WriteAsync(buf, ct);
    }
}