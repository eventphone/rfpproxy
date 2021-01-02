using System;
using RfpProxy.AaMiDe.Sys;

namespace RfpProxy.Virtual
{
    partial class VirtualRfp
    {
        private TimeSpan _licenseGracePeriod = TimeSpan.FromMinutes(UInt16.MaxValue);

        private void OnLicenseTimer(SysLicenseTimerMessage message)
        {
            if (message.GracePeriod.TotalMinutes > Int32.MaxValue)
            {
                //query
                var licenseTimer = new SysLicenseTimerMessage(_licenseGracePeriod, message.Md5);
                SendMessage(licenseTimer);
            }
            else
            {
                _licenseGracePeriod = message.GracePeriod;
            }
        }
    }
}
