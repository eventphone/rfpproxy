# IPUI Modification #

## Problem ##

The Motorola S120x (STARTAC) seems to overwrite the last bits of the assigned IPUI-O - or have not enough memory to store a 48 bit IPUI-O.

``` log
FT> MM(d0) AccessRightsAccept
FT>   PortableIdentity    : IPUI-O(301400fdf)
PP> MM(o0) LocateRequest
PP>   PortableIdentity    : IPUI-O(301400f10)
```

## Technical details ##

The OMM uses the IPEI as IPUI-O (`12308 0004063 5` -> `0x3014_00fdf`). The IPEI has 36 bit (16 bit EMC and 20 bit PSN). The IPUI-O is transmitted with a length of 48 bit (4 bit PUT `IPUI-O` and 44 bit PUN).

Since only 36 bit of the IPUI-O can be used by an IPEI, we can shift the IPUI by 8 bit to the left, before transmitting to the phone, and reverse the shift before transmitting to OMM.

## unhandled cases ##

The rfpproxy subscription currently only works, when the Portable Identity IE is the first IE.

### ETSI EN 300 175-5 section 6.3.6.22 ###

MM-INFO-SUGGEST may contain a Portable Identity IE as the second IE.

### ETSI EN 300 175-5 section 6.3.6.14 ###

IDENTITY-REPLY may contain a Repeat Indicator IE before the Portable Identity IE.

### Fragmented Messages ###

Since we modify the message, we need to process a message, before all fragments are available. Thus there is currently no defragmentation or connection tracking. There is a very unlikely possibility, that the second or third part of a fragmented message may contain an IE, which matches our filter and is not a Portable Identity IE, so we may shift wrong data and produce invalid IE content.

## risk ##

Since we modify the stored IPUI, the PP has no longer a valid registration, if the software is stopped. If we modify the replacement in a future version, all PPs have to be re-registered.
