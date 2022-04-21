using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace MinecraftServer.Networking;

public class EncryptionAdapter : DataAdapter
{
    private BufferedBlockCipher _encrypt;
    private BufferedBlockCipher _decrypt;

    private BufferedBlockCipher GetInstance(byte[] key, bool encrypt)
    {
        BufferedBlockCipher cipher = new BufferedBlockCipher(new CfbBlockCipher(new AesEngine(), 8));
        cipher.Init(encrypt, new ParametersWithIV(new KeyParameter(key), key));
        return cipher;
    }
    public EncryptionAdapter(DataAdapter baseAdapter, byte[] key)
    {
        _encrypt = GetInstance(key, true);
        _decrypt = GetInstance(key, false);
        BaseAdapter = baseAdapter;
    }

    public override void Flush()
    {
        BaseAdapter.Flush();
    }

    public DataAdapter BaseAdapter { get; }

    private void Transform(BufferedBlockCipher tra, ArraySegment<byte> a, ArraySegment<byte> b)
    {
        tra.ProcessBytes(a.Array, a.Offset, a.Count, 
            b.Array, b.Offset);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buf, CancellationToken ct = default)
    {
        int len = await BaseAdapter.ReadAsync(buf, ct);
        if (len == 0) return 0;
        if (!MemoryMarshal.TryGetArray<byte>(buf, out var seg))
        {
            throw new InvalidOperationException("How tf are u even using non-heap memory with an async method??");
        }
        using var tPlainText = PooledArray.Allocate(len);
        Transform(_decrypt, seg.Slice(0, len), tPlainText.Data);
        new Memory<byte>(tPlainText.Data.Array, tPlainText.Data.Offset, len).CopyTo(buf);
        return len;
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buf, CancellationToken ct = default)
    {
        if (!MemoryMarshal.TryGetArray(buf, out var seg))
        {
            throw new InvalidOperationException("How tf are u even using non-heap memory with an async method??");
        }
        using var tCipherText = PooledArray.Allocate(buf.Length);
        Transform(_encrypt, seg, tCipherText.Data);
        await BaseAdapter.WriteAsync(tCipherText.Data.Slice(0, buf.Length));
    }
}