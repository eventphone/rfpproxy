using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RfpProxy.Log.Messages.Dnm
{
    public class NwkReassembler
    {
        private readonly struct Fragment
        {
            public readonly byte Ns;

            public readonly ReadOnlyMemory<byte> Data;

            public Fragment(byte ns, ReadOnlyMemory<byte> data)
            {
                Ns = ns;
                Data = data;
            }

            public override string ToString()
            {
                return $"Ns={Ns}";
            }
        }

        private readonly Dictionary<byte, List<Fragment>> _fragments = new Dictionary<byte, List<Fragment>>();

        private readonly Dictionary<byte, List<Fragment>> _retransmits = new Dictionary<byte, List<Fragment>>();

        public bool IsEmpty
        {
            get { return _fragments.Count == 0 && _retransmits.Count == 0; }
        }

        public void AddFragment(byte lln, byte ns, ReadOnlyMemory<byte> fragment)
        {
            if (!_fragments.ContainsKey(lln))
            {
                _fragments.Add(lln, new List<Fragment>());
                _retransmits.Remove(lln);
            }
            _fragments[lln].Add(new Fragment(ns, fragment));
        }

        public ReadOnlyMemory<byte> Reassemble(byte lln, byte ns, in ReadOnlyMemory<byte> fragment, out bool retransmit)
        {
            retransmit = false;
            List<Fragment> fragments;
            if (!_fragments.ContainsKey(lln))
            {
                if (_retransmits.TryGetValue(lln, out fragments))
                {
                    var previous = fragments[fragments.Count-1];//todo handle more than the last
                    if (previous.Ns == ns)
                    {
                        if (previous.Data.Length == fragment.Length)
                        {
                            if (previous.Data.Span.SequenceEqual(fragment.Span))
                            {
                                retransmit = true;
                                _fragments.Add(lln, fragments);
                                _retransmits.Remove(lln);
                            }
                        }
                    }
                }
            }
            if (!_fragments.ContainsKey(lln))
                return fragment;
            fragments = _fragments[lln];
            if (!retransmit)
                fragments.Add(new Fragment(ns, fragment));
            var modulus = lln == 1 ? 2 : 8;
            int vr = fragments[0].Ns;
            for (int i = 1; i < fragments.Count; i++)
            {
                vr = (vr + 1) % modulus;
                var frag = fragments[i];
                if (frag.Ns != vr)
                {
                    //invalid Ns
                    if (Debugger.IsAttached)
                        Debugger.Break();
                }
            }
            var size = fragments.Sum(x => x.Data.Length);
            var result = new byte[size].AsMemory();
            var slice = result;
            foreach (var frag in fragments)
            {
                frag.Data.CopyTo(slice);
                slice = slice.Slice(frag.Data.Length);
            }

            _retransmits.Add(lln, fragments);
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