using System.Collections.Generic;
using RfpProxy.Log.Messages.Dnm;

namespace RfpProxy.Log.Messages
{
    public class AaMiDeReassembler
    {
        private readonly Dictionary<byte, NwkReassembler> _reassemblers = new Dictionary<byte, NwkReassembler>();

        public NwkReassembler GetNwk(byte mcei)
        {
            if (_reassemblers.TryGetValue(mcei, out var nwk))
                return nwk;
            return new NwkReassembler();
        }

        public void Return(byte mcei, NwkReassembler reassembler)
        {
            if (reassembler.IsEmpty)
            {
                if (_reassemblers.ContainsKey(mcei))
                    _reassemblers.Remove(mcei);
            }
            else
            {
                if (!_reassemblers.ContainsKey(mcei))
                    _reassemblers.Add(mcei, reassembler);
            }
        }
    }
}