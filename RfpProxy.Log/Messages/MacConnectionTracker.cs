using System;
using System.Collections.Generic;
using System.Linq;
using RfpProxy.Log.Messages.Dnm;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public class MacConnectionTracker
    {
        private readonly  Dictionary<RfpIdentifier, RfpConnectionTracker> _connections = new Dictionary<RfpIdentifier, RfpConnectionTracker>();

        public RfpConnectionTracker Get(RfpIdentifier rfp)
        {
            if (!_connections.TryGetValue(rfp, out var tracker))
            {
                tracker = new RfpConnectionTracker(rfp, this);
                _connections.Add(rfp, tracker);
            }
            return tracker;
        }

        public MacConnection Find(uint pmid)
        {
            foreach (var connection in _connections.Values)
            {
                if (connection.TryGetByPMID(pmid, out var result))
                    return result;
            }
            return null;
        }
    }

    public class RfpConnectionTracker
    {
        private readonly MacConnectionTracker _tracker;
        private readonly Dictionary<byte, MacConnection> _connections = new Dictionary<byte, MacConnection>();
        
        public RfpIdentifier Rfp { get; }

        public RfpConnectionTracker(RfpIdentifier rfp, MacConnectionTracker tracker)
        {
            _tracker = tracker;
            Rfp = rfp;
        }

        public MacConnection Get(byte mcei)
        {
            if (!_connections.TryGetValue(mcei, out var connection))
            {
                connection = new MacConnection(this, mcei);
                _connections.Add(mcei, connection);
            }
            return connection;
        }

        public MacConnection Find(uint pmid)
        {
            return _tracker.Find(pmid);
        }

        public bool TryGetByPMID(uint pmid, out MacConnection connection)
        {
            connection = _connections.Values.Where(x=>x.IsConnected).FirstOrDefault(x=>x.PMID == pmid);
            return connection != null;
        }

        public void Close(MacConnection connection)
        {
            _connections.Remove(connection.MCEI);
        }
    }

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