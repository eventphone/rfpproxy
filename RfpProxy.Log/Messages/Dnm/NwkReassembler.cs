using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RfpProxy.Log.Messages.Dnm
{
    public class NwkReassembler
    {
        private readonly Dictionary<byte, ReadOnlyMemory<byte>[]> _fragments = new Dictionary<byte, ReadOnlyMemory<byte>[]>();

        private readonly Dictionary<byte, ReadOnlyMemory<byte>[]> _retransmits = new Dictionary<byte, ReadOnlyMemory<byte>[]>();

        public bool IsEmpty
        {
            get { return _fragments.Count == 0 && _retransmits.Count == 0; }
        }

        public void AddFragment(byte lln, byte ns, ReadOnlyMemory<byte> fragment)
        {
            if (!_fragments.ContainsKey(lln))
            {
                _fragments.Add(lln, new ReadOnlyMemory<byte>[8]);
                _retransmits.Remove(lln);
            }
            _fragments[lln][ns] = fragment;
        }

        public ReadOnlyMemory<byte> Reassemble(byte lln, byte ns, in ReadOnlyMemory<byte> fragment)
        {
            if (!_fragments.ContainsKey(lln))
            {
                if (_retransmits.TryGetValue(lln, out var fragments))
                {
                    var previous = fragments[ns];
                    if (!previous.IsEmpty)
                    {
                        if (previous.Length == fragment.Length)
                        {
                            if (previous.Span.SequenceEqual(fragment.Span))
                            {
                                if (Debugger.IsAttached)
                                    Debugger.Break();//todo mark as retransmit
                            }
                        }
                    }
                }
                return fragment;
            }
            //todo validate
            _fragments[lln][ns] = fragment;
            var size = _fragments[lln].Sum(x => x.Length);
            var result = new byte[size].AsMemory();
            var slice = result;
            for (int i = ns +1; i < 8; i++)
            {
                var frag = _fragments[lln][i];
                frag.CopyTo(slice);
                slice = slice.Slice(frag.Length);
            }
            for (int i = 0; i <= ns; i++)
            {
                var frag = _fragments[lln][i];
                frag.CopyTo(slice);
                slice = slice.Slice(frag.Length);
            }

            _retransmits.Add(lln, _fragments[lln]);
            _fragments.Remove(lln);
            return result;
        }

        public void Clear()
        {
            _fragments.Clear();
            _retransmits.Clear();
        }
    }
}