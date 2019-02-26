﻿using System;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public abstract class NwkSingleByteInformationElement : NwkInformationElement
    {
        protected NwkSingleByteInformationElement(byte identifier, byte content)
        {
        }

        public static NwkInformationElement Create(byte identifier, byte content)
        {
            switch (identifier)
            {
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}