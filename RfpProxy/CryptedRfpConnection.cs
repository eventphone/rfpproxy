using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxy
{
    public class CryptedRfpConnection : RfpConnection
    {
        private static readonly byte[] KeyRfp = BlowFish.HexToByte("87E0F9B38927F7231541FA19C2E2DE7629EB96C85E7C794D2EA55DE608E4AE07CFF431A267B8790A36C6E41C21F8350C77871E168798731F");
        private static readonly byte[] KeyOmm  = BlowFish.HexToByte("9DC880C7C2DA904C841515CA34BF7E9C51D41DABBD8E9E07BBEB457EF4A16D884F7DC48414D1EA501C77BBF73C586C4D684BB1869246C895");
        
        private static readonly byte[] InitivRfp = BlowFish.HexToByte("6F9F5B898C88ACE6");
        private static readonly byte[] InitivOmm = BlowFish.HexToByte("0C8213A6B79642BC");
        
        private readonly BlowFish _rfpToOmmEncipher;
        private readonly BlowFish _rfpToOmmDecipher;
        private readonly BlowFish _ommToRfpEncipher;
        private readonly BlowFish _ommToRfpDecipher;

        public CryptedRfpConnection(TcpClient client, TcpClient server) : base(client, server)
        {
            _rfpToOmmDecipher = new BlowFish(KeyRfp);
            _rfpToOmmEncipher = new BlowFish(_rfpToOmmDecipher);
            _ommToRfpDecipher = new BlowFish(KeyOmm);
            _ommToRfpEncipher = new BlowFish(_ommToRfpDecipher);
        }

        public ReadOnlyMemory<byte> RfpToOmmIv { get; set; }

        public ReadOnlyMemory<byte> OmmToRfpIv { get; private set; }

        public void InitRfpToOmmIv(ReadOnlySpan<byte> iv)
        {
            var rfpIv = InitivRfp.AsSpan().ToArray();
            BlowFish.XorBlock(rfpIv, iv);
            RfpToOmmIv = rfpIv;
        }

        public void InitOmmToRfpIv(ReadOnlySpan<byte> iv)
        {
            var ommIv = InitivOmm.AsSpan().ToArray();
            BlowFish.XorBlock(ommIv, iv);
            OmmToRfpIv = ommIv;
        }

        public Memory<byte> CryptOmmToRfp(ReadOnlyMemory<byte> data)
        {
            var result = _ommToRfpEncipher.Encrypt_CBC(OmmToRfpIv.Span, data.Span);
            OmmToRfpIv = result.Slice(result.Length - 8);
            return result;
        }

        public ReadOnlyMemory<byte> CryptRfpToOmm(ReadOnlyMemory<byte> data)
        {
            var result = _rfpToOmmEncipher.Encrypt_CBC(RfpToOmmIv.Span, data.Span);
            RfpToOmmIv = result.Slice(result.Length - 8);
            return result;
        }

        public Memory<byte> DecryptRfpToOmm(ReadOnlyMemory<byte> data, ReadOnlyMemory<byte> iv)
        {
            return _rfpToOmmDecipher.Decrypt_CBC(iv.Span, data.Span);
        }

        public Memory<byte> DecryptOmmToRfp(ReadOnlyMemory<byte> data, ReadOnlyMemory<byte> iv)
        {
            return _ommToRfpDecipher.Decrypt_CBC(iv.Span, data.Span);
        }

        public override ValueTask<int> SendToServerAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (!RfpToOmmIv.IsEmpty)
                data = CryptRfpToOmm(data);
            return base.SendToServerAsync(data, cancellationToken);
        }

        public override ValueTask<int> SendToClientAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (!OmmToRfpIv.IsEmpty)
                data = CryptOmmToRfp(data);
            return base.SendToClientAsync(data, cancellationToken);
        }
    }
}