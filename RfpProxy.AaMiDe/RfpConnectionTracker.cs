using System.Collections.Generic;
using System.Linq;
using RfpProxyLib;

namespace RfpProxy.AaMiDe
{
    public class RfpConnectionTracker
    {
        private readonly MacConnectionTracker _tracker;
        private readonly Dictionary<byte, MacConnection> _connections = new Dictionary<byte, MacConnection>();
        
        public RfpIdentifier Rfp { get; }

        public RfpConnectionTracker(RfpIdentifier rfp)
            : this(rfp, new MacConnectionTracker())
        {
        }

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
}