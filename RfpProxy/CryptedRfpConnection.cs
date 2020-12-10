using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RfpProxyLib;

namespace RfpProxy
{
    public class CryptedRfpConnection : RfpConnection
    {
        private static readonly byte[] InitivRfp = HexEncoding.HexToByte("68e8364be9c234c1");
        private static readonly byte[] InitivOmm = HexEncoding.HexToByte("dfe66571fac45a42");
        
        private BlowFish _rfpToOmmEncipher;
        private BlowFish _rfpToOmmDecipher;
        private BlowFish _ommToRfpEncipher;
        private BlowFish _ommToRfpDecipher;

        public CryptedRfpConnection(TcpClient client, TcpClient server) : base(client, server)
        {
        }

        public ReadOnlyMemory<byte> RfpToOmmIv { get; set; }

        public ReadOnlyMemory<byte> OmmToRfpIv { get; set; }

        public void SetOmmKey(ReadOnlySpan<byte> key)
        {
            _ommToRfpDecipher = new BlowFish(key);
            _ommToRfpEncipher = new BlowFish(_ommToRfpDecipher);
        }

        public void SetRfpKey(ReadOnlySpan<byte> key)
        {
            _rfpToOmmDecipher = new BlowFish(key);
            _rfpToOmmEncipher = new BlowFish(_rfpToOmmDecipher);
        }

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