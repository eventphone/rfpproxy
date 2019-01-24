## Protocol
There are two phases - first a handshake, in plaintext JSON, where the client sends all parameters for the messages it wants to receive or handle. after this handshake is completed, there is a protocol switch to a binary protocol.

### Handshake

There are two JSON formatted message types: `text` & `subscription`. Every message has to be terminated with a line-break `\n`. If an invalid message is received during the handshake, the proxy will send a plaintext error message and close the connection.

#### Flow
```
# > // from proxy to client
# < // from client to proxy
> Hello (text)
< Subscription (subscription)
< Subscription (subscription)
< Subscription (subscription)
...
< End Of Subscribe (subscription)
> Ack (text)
<> binary
```

#### Text
`{"msg":"<content>"}`

Example: `{"msg":"stay connected"}`

#### Subscription
```json
{
  "type": "Listen", // Listen|Handle|End
  "prio": 255, // single byte
  "rfp": {
    "filter": "000000000000", //hex encoded mac address
    "mask": "000000000000" //binary mask
  },
  "filter": {
    "filter": "", //hex encoded binary
    "mask": "" //binary mask
  }
}
```
* type:
  * always required - all other fields can be left empty for type `End`
  * Listen: you will only receive the message - no answer is expected
  * Handle: you have 10ms to send a modified version of the message (or an empty message to suppress it if it is still empty after all other handlers)
  * End: you are finished with your subscriptions
* prio:
  * all subscriptions are processed in ascending priority
  * to log the message before and after modification you have to subscribe twice: first with prio 0 and second with prio 255
* rfp:
  * filter:
    * HEX encoded MAC address
    * must be exactly 6 bytes
    * may change in future versions
  * mask:
    * HEX encoded mask
    * must be exactly 6 bytes
    * the mask will be &ed with the RFP's MAC Address and then compared against `rfp.filter` to decide if you want this message
* filter:
  * see `rfp`
  * filter:
    * can match the whole message (arbitrary length)
    * if the filter is longer than the message it will __not__ match
  * mask:
    * see `rfp.mask`
    * length has to match `filter.filter`

### Binary Protocol
All messages (subscriptions, replies & unsolicited messages) are composed of a header and the raw decrypted message from/to OMM or RFP.

#### Header
* 4 bytes Length:
  * does not include the 4 bytes length itself
  * BigEndian
  * unsigned (max 0x7FFFFFC7)
* 1 byte Direction:
  * 0: FromOmm / ToRfp
  * 1; FromRfp / ToOmm
* 4 byte message id:
  * when received from proxy and subscribed with handle the response has to contain this message id - even if the message should be discarded
  * when sending unsolicited messages this has to be 0
  * BigEndian
  * unsinged
* (currently) 6 bytes RFP Identifier
  * MAC Address

### Remarks

#### RFP identifier in replies
Even though you can change the identifier in handled messages, this is not supported. The RFP identifier in your reply is ignored - it was only left in the message to keep the protocol as simple as possible.

#### Unsolicited Messages
If you send unsolicited messages and one of your subscriptions matches, you __will__ get this message as if it was sent from OMM or an RFP.

#### Messages without identifier
The RFP Identifier is set when the `SYS_INIT` message is received. It is therefore possible to get messages without a valid identifier. Currently it is not possible to inject unsolicited messages into any connection which has not yet received a `SYS_INIT` message.

## TPROXY
command to redirect without TPROXY:
iptables -t nat -I PREROUTING 1 -d 172.20.23.1 -p tcp --dport 16321 -j DNAT --to 172.20.23.1:16000
