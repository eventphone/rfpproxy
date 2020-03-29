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
        
        private static readonly byte[] RekeyTable = {

            0x38, 0x63, 0x74, 0xF2, 0xFC, 0xC3, 0xA5, 0x63,
            0x47, 0xE4, 0xEB, 0x35, 0x0F, 0xF8, 0x24, 0x6A,
            0x36, 0x8A, 0x4B, 0x3B, 0x28, 0x59, 0x04, 0x72,
            0x88, 0x3C, 0x8D, 0x38, 0xA9, 0x95, 0xE4, 0x4D,
            0xFD, 0x17, 0x8E, 0x72, 0x68, 0x14, 0x55, 0x87,
            0xB0, 0x50, 0x29, 0x33, 0x78, 0xF4, 0x90, 0xF6,
            0x80, 0x45, 0x64, 0xBC, 0xA2, 0x10, 0x3B, 0x28,
            
            0xA5, 0xAB, 0xF4, 0x35, 0x3E, 0x19, 0x35, 0x2F,
            0xC3, 0xF1, 0xFE, 0xFF, 0x3B, 0x47, 0x5A, 0xF6,
            0x67, 0x5E, 0x56, 0x90, 0x95, 0xD7, 0x9A, 0x75,
            0x33, 0xD7, 0xC8, 0x46, 0x5D, 0x34, 0x89, 0xC5,
            0xB2, 0x6A, 0x4A, 0xF6, 0x7C, 0xC5, 0xBF, 0xD7,
            0xAC, 0x27, 0x92, 0xC4, 0x44, 0xAC, 0xFC, 0xBB,
            0xE8, 0x0E, 0xD5, 0x3A, 0x30, 0x56, 0xF3, 0xBD,
            
            0xB3, 0xD3, 0xBB, 0xC3, 0x33, 0x80, 0x3C, 0x12,
            0x4B, 0x12, 0x5B, 0xE9, 0x21, 0x80, 0x39, 0xB0,
            0xC4, 0xE3, 0x98, 0x75, 0xF4, 0x2F, 0xE2, 0xC6,
            0x66, 0x0A, 0x19, 0xAA, 0x53, 0xE0, 0x05, 0x65,
            0xE3, 0x18, 0x46, 0x34, 0x0B, 0x86, 0xB8, 0xFF,
            0x9C, 0x7C, 0x00, 0xCE, 0x67, 0x96, 0x0C, 0x50,
            0x4C, 0x71, 0x41, 0xDF, 0x7A, 0x19, 0xB2, 0xDC,
            
            0x34, 0x33, 0x42, 0x70, 0xBA, 0xA7, 0xCB, 0x31,
            0x5E, 0x53, 0xA1, 0xF0, 0xE3, 0x62, 0xE7, 0xC6,
            0xED, 0x08, 0x0E, 0xBD, 0xAA, 0x53, 0x9B, 0x8B,
            0x48, 0xAF, 0x44, 0x4C, 0x5B, 0x04, 0xAB, 0x62,
            0x2C, 0xEC, 0x77, 0x96, 0x6C, 0x3E, 0xC1, 0xF5,
            0xAA, 0xBA, 0xE4, 0xD2, 0x46, 0x6E, 0x39, 0x5C,
            0x3B, 0xF6, 0x5F, 0xC9, 0xFD, 0x81, 0xC1, 0xC3,
        };

        private BlowFish _rfpToOmmEncipher;
        private BlowFish _rfpToOmmDecipher;
        private BlowFish _ommToRfpEncipher;
        private BlowFish _ommToRfpDecipher;

        private int _rfpToOmmDecryptCount = -1;
        private int _rfpToOmmEncryptCount = -1;
        private int _ommToRfpDecryptCount = -2;
        private int _ommToRfpEncryptCount = -2;

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

        public void RekeyOmmToRfpDecrypt(ReadOnlyMemory<byte> packet)
        {
            _ommToRfpDecryptCount++;
            if (_ommToRfpDecryptCount == 2500)
            {
                Console.WriteLine("Rekey OMM to RFP decrypt");
                _ommToRfpDecipher = Rekey(packet.Span.Slice(0, 4));
                _ommToRfpDecryptCount = 0;
            }
        }

        public void RekeyRfpToOmmDecrypt(ReadOnlyMemory<byte> packet)
        {
            _rfpToOmmDecryptCount++;
            if (_rfpToOmmDecryptCount == 2500)
            {
                Console.WriteLine("Rekey RFP to OMM decrypt");
                _rfpToOmmDecipher = Rekey(packet.Span.Slice(0, 4));
                _rfpToOmmDecryptCount = 0;
            }
        }

        public override ValueTask<int> SendToServerAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            _rfpToOmmEncryptCount++;
            if (!RfpToOmmIv.IsEmpty)
                data = CryptRfpToOmm(data);
            if (_rfpToOmmEncryptCount == 2500)
            {
                Console.WriteLine("Rekey RFP to OMM encrypt");
                _rfpToOmmEncipher = Rekey(data.Span.Slice(0, 4));
                _rfpToOmmEncryptCount = 0;
            }
            return base.SendToServerAsync(data, cancellationToken);
        }

        public override ValueTask<int> SendToClientAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            _ommToRfpEncryptCount++;
            if (!OmmToRfpIv.IsEmpty)
                data = CryptOmmToRfp(data);
            if (_ommToRfpEncryptCount == 2500)
            {
                Console.WriteLine("Rekey OMM to RFP encrypt");
                _ommToRfpEncipher = Rekey(data.Span.Slice(0, 4));
                _ommToRfpEncryptCount = 0;
            }
            return base.SendToClientAsync(data, cancellationToken);
        }

        public BlowFish Rekey(ReadOnlySpan<byte> seed)
        {
            if (seed.Length != 4)
                throw new ArgumentOutOfRangeException(nameof(seed));
            Span<byte> buffer = stackalloc byte[56];
            var next = BinaryPrimitives.ReadUInt32LittleEndian(seed);
            for (byte i = 0; i < buffer.Length; i++)
            {
                var rnd = StdLib.RandR(ref next);
                var tableNr = rnd & 0x03;
                var offset = 56 * tableNr + i;
                var bt = RekeyTable[offset];
                var res = (byte) (bt ^ (rnd >> 24));
                buffer[i] = res;
            }
            return new BlowFish(buffer);
        }
    }
}