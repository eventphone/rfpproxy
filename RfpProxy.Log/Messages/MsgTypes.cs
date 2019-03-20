namespace RfpProxy.Log.Messages
{
    public enum MsgType : ushort
    {
        ACK = 0x0001,
        NACK = 0x0002,
        HEARTBEAT = 0x0003,

        SYS_IP_OPTIONS = 0x0101,
        SYS_LED = 0x0102,
        SYS_SPY = 0x0104,
        SYS_HEARTBEAT_INTERVAL = 0x0105,
        SYS_RSX = 0x0106,
        SYS_SYSLOG = 0x0107,
        SYS_MAX_CHANNELS = 0x0108,
        SYS_HTTP_SET = 0x0109,
        SYS_PASSWD = 0x010a,
        SYS_CRYPTED_PACKET = 0x010b,
        SYS_OMM_CONTROL = 0x010c,
        SYS_STATE_DUMP = 0x010d,
        SYS_RPING = 0x010e,
        SYS_STATE_DUMP_REQ = 0x010f,
        SYS_STATE_DUMP_RES = 0x0110,
        SYS_NEW_SW = 0x0111,
        SYS_AUDIO_LOG = 0x0112,
        SYS_USB_OVERLOAD = 0x0113,

        SYS_COREFILE_URL = 0x0116,
        SYS_ROUNDTRIP_DELAY = 0x0117,
        SYS_INIT = 0x0120,
        SYS_RESET = 0x0121,
        SYS_AUTHENTICATE = 0x012d,
        SYS_LICENSE_TIMER = 0x0134,

        MEDIA_OPEN = 0x0200,
        MEDIA_UNKNOWN = 0x0201,
        MEDIA_CLOSE = 0x0202,
        MEDIA_START = 0x0203,
        MEDIA_STOP = 0x0204,
        MEDIA_STATISTICS = 0x0205,
        MEDIA_REDIRECT_START = 0x0206,
        MEDIA_REDIRECT_STOP = 0x0207,
        MEDIA_RESTART = 0x0208,
        MEDIA_DTMF = 0x0209,
        MEDIA_DSP_CLOSE = 0x020a,
        MEDIA_TONE2 = 0x020b,
        MEDIA_CHANNEL_MOD = 0x020c,
        MEDIA_MUTE = 0x020d,
        MEDIA_G729_USED = 0x020e,
        MEDIA_TRACE_PPN = 0x020f,
        MEDIA_EOS_DETECT = 0x0210,
        MEDIA_AUDIO_STATISTICS = 0x0211,
        MEDIA_VIDEO_STATE = 0x0212,

        DNM = 0x0301,
        SYNC = 0x0302,

        WLAN_RFP_CONFIG = 0x0401,
        WLAN_RFP_UP = 0x0402,
        WLAN_RFP_DOWN = 0x0403,
        WLAN_RFP_CLIENT_REQ = 0x0404,
        WLAN_RFP_CLIENT_REP = 0x0405,
        WLAN_RFP_SET_ACL = 0x0406,
        WLAN_RFP_CLIENT_INFO = 0x0407,
        WLAN_RFP_ACK = 0x0408,
        WLAN_RFP_LINK_NOK_NACK = 0x0409,
        WLAN_RFP_IFACE_REP = 0x040e,

        SNMP_RFP_UPDATE = 0x0501,

        CONF_OPEN = 0x0600,
        CONF_ADD_SUBSCR = 0x0601,
        CONF_CHG_SUBSCR = 0x0602,
        CONF_DEL_SUBSCR = 0x0603,
        CONF_CLOSE = 0x0604,
        CONF_RTP = 0x0605,

        BLUETOOTH_DEVICE = 0x0700,
        BLUETOOTH_CONFIG = 0x0701,
        BLUETOOTH_DATA = 0x0702,

        VIDEO_DEVICE = 0x0800,
        VIDEO_CONFIG = 0x0801,
    }


}