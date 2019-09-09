using System;
using System.Collections.Generic;
using RfpProxyLib;

namespace RfpProxyLib
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
}