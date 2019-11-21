namespace RfpProxy.AaMiDe.Nwk
{
    public enum NwkProtocolDiscriminator : byte
    {
        /// <summary>
        /// Link Control Entity (LCE) messages
        /// </summary>
        LCE = 0b0000,

        /// <summary>
        /// Call Control (CC) messages
        /// </summary>
        CC = 0b0011,

        /// <summary>
        /// Call Independent Supplementary Services (CISS) messages
        /// </summary>
        CISS = 0b0100,

        /// <summary>
        /// Mobility Management (MM) messages
        /// </summary>
        MM = 0b0101,

        /// <summary>
        /// ConnectionLess Message Service (CLMS) messages
        /// </summary>
        CLMS = 0b0110,

        /// <summary>
        /// Connection Oriented Message Service (COMS) messages
        /// </summary>
        COMS = 0b0111,

    }
}