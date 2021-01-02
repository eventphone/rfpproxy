using System;
using System.Threading;
using RfpProxy.AaMiDe;
using RfpProxy.AaMiDe.Sys;

namespace RfpProxy.Virtual
{
    partial class VirtualRfp
    {
        private readonly Timer _heartbeatTimer;
        private readonly DateTime _bootTimestamp = DateTime.Now;

        private void SendHeartbeat(object state)
        {
            var heartbeat = new HeartbeatMessage(DateTime.Now - _bootTimestamp);
            SendMessage(heartbeat);
        }

        void OnHeartbeatInterval(SysHeartbeatIntervalMessage message)
        {
            _heartbeatTimer.Change(message.Interval, message.Interval);
        }
    }
}
