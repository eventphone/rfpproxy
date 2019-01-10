using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Elskom.Generic.Libs;

namespace RfpProxy
{
    public class CryptedRfpConnection : RfpConnection
    {
        private readonly byte[] _key_tx = HexToByte("87E0F9B38927F7231541FA19C2E2DE7629EB96C85E7C794D2EA55DE608E4AE07CFF431A267B8790A36C6E41C21F8350C77871E168798731F");
        private readonly byte[] _key_rx  = HexToByte("9DC880C7C2DA904C841515CA34BF7E9C51D41DABBD8E9E07BBEB457EF4A16D884F7DC48414D1EA501C77BBF73C586C4D684BB1869246C895");
        private readonly byte[] _initiv_tx = HexToByte("6F9F5B898C88ACE6");
        private readonly byte[] _initiv_rx = HexToByte("0C990CF983945A73");
        private readonly BlowFish _rfpToOmmCipher;
        private readonly BlowFish _ommToRfpCipher;

        public CryptedRfpConnection(TcpClient client, TcpClient server) : base(client, server)
        {
            _rfpToOmmCipher = new BlowFish(_key_tx);
            _ommToRfpCipher = new BlowFish(_key_rx);
        }

        public void SetRfpToOmmIV(ReadOnlySpan<byte> iv)
        {
            
            _rfpToOmmCipher.IV = Xor(iv, _initiv_tx);
        }
        
        public void SetOmmToRfpIV(ReadOnlySpan<byte> iv)
        {
            _ommToRfpCipher.IV = Xor(iv, _initiv_rx);
        }

        public ReadOnlyMemory<byte> CryptOmmToRfp(ReadOnlyMemory<byte> data)
        {
            return _ommToRfpCipher.Encrypt_CBC(data.ToArray());
        }

        public ReadOnlyMemory<byte> CryptRfpToOmm(ReadOnlyMemory<byte> data)
        {
            return _rfpToOmmCipher.Encrypt_CBC(data.ToArray());
        }

        public ReadOnlyMemory<byte> DecryptRfpToOmm(ReadOnlyMemory<byte> data)
        {
            return _rfpToOmmCipher.Decrypt_CBC(data.ToArray());
        }

        public ReadOnlyMemory<byte> DecryptOmmToRfp(ReadOnlyMemory<byte> data)
        {
            return _ommToRfpCipher.Decrypt_CBC(data.ToArray());
        }

        public ValueTask<int> SendToServerPlainAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return base.SendToServerAsync(data, cancellationToken);
        }

        public ValueTask<int> SendToClientPlainAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            return base.SendToClientAsync(data, cancellationToken);
        }

        public override ValueTask<int> SendToServerAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            var crypted = CryptRfpToOmm(data);
            return base.SendToServerAsync(crypted, cancellationToken);
        }

        public override ValueTask<int> SendToClientAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            var crypted = CryptOmmToRfp(data);
            return base.SendToClientAsync(crypted, cancellationToken);
        }

        private static byte[] Xor(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            if (left.Length != right.Length)
                throw new ArgumentOutOfRangeException("left and right have to be the same length");

            var result = new byte[left.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte) (left[i] ^ right[i]);
            }
            return result;
        }

        private static byte[] HexToByte(string hex)
        {
            byte[] result = new byte[hex.Length / 2];
            int[] hexValue =
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
                0x06, 0x07, 0x08, 0x09, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00,
                0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            };

            for (int x = 0, i = 0; i < hex.Length; i += 2, x += 1)
            {
                result[x] = (byte)(hexValue[Char.ToUpper(hex[i + 0]) - '0'] << 4 |
                                   hexValue[Char.ToUpper(hex[i + 1]) - '0']);
            }

            return result;
        }
    }
}