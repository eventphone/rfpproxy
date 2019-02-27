using System;
using System.Collections.Generic;
using System.Linq;

namespace RfpProxy.Log.Messages.Dnm
{
    public class NwkReassembler
    {
        private readonly Dictionary<byte, List<ReadOnlyMemory<byte>>> _fragments = new Dictionary<byte, List<ReadOnlyMemory<byte>>>();

        public bool IsEmpty
        {
            get { return _fragments.Count == 0; }
        }

        public void AddFragment(byte lln, ReadOnlyMemory<byte> fragment)
        {
            if (!_fragments.ContainsKey(lln))
                _fragments.Add(lln, new List<ReadOnlyMemory<byte>>(1));
            _fragments[lln].Add(fragment);
        }

        public ReadOnlyMemory<byte> Reassemble(byte lln, in ReadOnlyMemory<byte> fragment)
        {
            if (!_fragments.ContainsKey(lln))
                return fragment;
            var size = _fragments[lln].Sum(x => x.Length) + fragment.Length;
            var result = new byte[size].AsMemory();
            var slice = result;
            foreach (var frag in _fragments[lln])
            {
                frag.CopyTo(slice);
                slice = slice.Slice(frag.Length);
            }
            fragment.CopyTo(slice);
            _fragments.Remove(lln);
            return result;
        }
    }
}