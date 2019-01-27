using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public enum DnmType : byte
    {
        MacConInd = 0x01,
        MacDisReq = 0x02,
        MacDisInd = 0x03,
        LcDataReq = 0x05,
        LcDataInd = 0x06,
        LcDtrInd = 0x07,
        MacPageReq = 0x08,
        MacEncKeyReq = 0x09,
        MacEncEksInd = 0x0a,
        HoInProgressInd = 0x0b,
        HoInProgressRes = 0x0c,
        HoFailedInd = 0x0d,
        HoFailedReq = 0x0e,
        DlcRfpErrorInd = 0x14,
        MacConExtInd = 0x15,
        HoInProgressExtInd = 0x16,
        MacModReq = 0x17,
        MacModCnf = 0x18,
        MacModInd = 0x19,
        MacModRej = 0x1a,
        MacRecordAudio = 0x1b,
        MacInfoInd = 0x1c,
        MacGetDefCkeyInd = 0x1d,
        MacGetDefCkeyRes = 0x1e,
        MacClearDefCkeyReq = 0x1f,
        MacGetCurrCkeyIdReq = 0x20,
        MacGetCurrCkeyIdCnf = 0x21,
    }

    public class DnmMessage : AaMiDeMessage
    {

        public byte Reserved0 { get; }

        /// <summary>
        /// MAC Connection Endpoint Identification 
        /// </summary>
        public byte MCEI { get; }

        public DnmType DnmType { get; }

        public byte Reserved1 { get; }

        public DnmMessage(ReadOnlyMemory<byte> data) : base(MsgType.DNM, data)
        {
            var span = data.Span;
            Reserved0 = span[0];
            DnmType = (DnmType) span[1];
            MCEI = span[2];
            Reserved1 = data.Span[3];
        }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(3);

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.WriteLine();
            writer.WriteLine($"\tDNM: Length({Raw.Length}) Type({DnmType,20:G}) MCEI({MCEI:X}) Reserved0({Reserved0:X}) Reserved1({Reserved1:x})");
            writer.Write($"\t{HexEncoding.ByteToHex(Raw.Span)}");
        }
    }
}