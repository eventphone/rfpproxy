using RfpProxy.AaMiDe.Mac;

namespace RfpProxy.AaMiDe
{
    public class MacConnection
    {
        private readonly RfpConnectionTracker _tracker;
        public byte MCEI { get; }

        public uint PMID { get; private set; }

        public NwkReassembler Reassembler { get; private set; }

        public bool IsConnected { get; private set; }

        public MacConnection(RfpConnectionTracker tracker, byte mcei)
        {
            _tracker = tracker;
            MCEI = mcei;
            IsConnected = false;
        }

        public void Open(MacConIndPayload macConInd)
        {
            PMID = macConInd.PMID;
            Reassembler = new NwkReassembler();
            if (macConInd.Ho)
            {
                var previous = _tracker.Find(PMID);
                if (previous != null)
                    Reassembler.CopyFrom(previous.Reassembler);
            }
            IsConnected = true;
        }

        public void Close()
        {
            if (IsConnected)
                Reassembler.Clear();
            _tracker.Close(this);
        }
    }
}