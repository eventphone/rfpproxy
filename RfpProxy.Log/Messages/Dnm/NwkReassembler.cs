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

            public readonly bool MoreData;

            public Fragment(byte ns, bool moreData, ReadOnlyMemory<byte> data)
            {
                Ns = ns;
                MoreData = moreData;
                Data = data;
            }

            public override string ToString()
            {
                return $"Ns={Ns}";
            }
        }

        private readonly Dictionary<byte, List<Fragment>> _fragments = new Dictionary<byte, List<Fragment>>();

        private readonly Dictionary<byte, Queue<Fragment>> _retransmits = new Dictionary<byte, Queue<Fragment>>();

        private readonly Dictionary<byte, List<Fragment>> _retransmitFragments = new Dictionary<byte, List<Fragment>>();

        public bool IsEmpty => _fragments.Count == 0 && _retransmits.Count == 0 && _retransmitFragments.Count == 0;

        public void CopyFrom(NwkReassembler nwkReassembler)
        {
            Clear();
            _retransmitFragments.Clear();
            foreach (var item in nwkReassembler._fragments)
            {
                _fragments.Add(item.Key, item.Value);
            }
            foreach (var item in nwkReassembler._retransmits)
            {
                _retransmits.Add(item.Key, item.Value);
            }
            foreach (var item in nwkReassembler._retransmitFragments)
            {
                _retransmitFragments.Add(item.Key, item.Value);
            }
        }

        private bool IsRetransmit(byte lln, byte ns, ReadOnlyMemory<byte> fragment, bool moreData)
        {
            if (!_retransmits.ContainsKey(lln))
            {
                _retransmits.Add(lln, new Queue<Fragment>(3));
            }

            var buffer = _retransmits[lln];
            if (buffer.Any(x => x.Ns == ns))
            {
                var previous = buffer.First(x => x.Ns == ns);
                if (previous.Data.Span.SequenceEqual(fragment.Span))
                {
                    if (_fragments.TryGetValue(lln, out var fragments))
                    {
                        //remove future fragments
                        while (fragments.Count > 0 && fragments[fragments.Count - 1].Ns != ns)
                        {
                            fragments.RemoveAt(fragments.Count - 1);
                        }
                        if (fragments.Count > 0)
                            fragments.RemoveAt(fragments.Count - 1);
                    }
                    if (lln != 1)
                    {
                        //remove future retransmits
                        _retransmits[lln] = buffer = new Queue<Fragment>(buffer.Reverse().SkipWhile(x => x.Ns != ns).Reverse());
                        if (!_fragments.ContainsKey(lln))
                        {
                            //readd fragments
                            fragments = new List<Fragment>();
                            for (int i = 0; i < buffer.Count-1; i++)
                            {
                                previous = buffer.Dequeue();
                                if (previous.MoreData)
                                {
                                    fragments.Add(previous);
                                }
                                else
                                {
                                    fragments.Clear();
                                }
                                buffer.Enqueue(previous);
                            }
                            previous = buffer.Dequeue();
                            buffer.Enqueue(previous);
                            if (_retransmitFragments.TryGetValue(lln, out var additional))
                            {
                                if (additional.Count > 0)
                                {
                                    var prevNs = ns;
                                    bool allFragmented = true;
                                    for (int i = fragments.Count - 1; i >= 0; i--)
                                    {
                                        if (prevNs == 0)
                                            prevNs = 7;
                                        else
                                            prevNs -= 1;
                                        if (!fragments[i].MoreData)
                                        {
                                            allFragmented = false;
                                            break;
                                        }
                                    }
                                    if (allFragmented)
                                    {
                                        for (int i = additional.Count - 1; i >= 0; i--)
                                        {
                                            if (prevNs == 0)
                                                prevNs = 7;
                                            else
                                                prevNs -= 1;
                                            if (additional[i].Ns == prevNs)
                                                fragments.Insert(0, additional[i]);
                                            else
                                                break;
                                        }
                                    }
                                }
                            }
                            _fragments.Add(lln, fragments);
                        }
                    }
                    return true;
                }
                var current = buffer.Dequeue();
                while (current.Ns != ns)
                {
                    buffer.Enqueue(current);
                    current = buffer.Dequeue();
                }
                for (int i = ns + 1; true; i++)
                {
                    i = lln == 1 ? i % 2 : i % 7;
                    if(!buffer.TryPeek(out current)) break;
                    if (current.Ns != i) break;
                    buffer.Dequeue();
                }
                if (_fragments.ContainsKey(lln))
                {
                    //todo cleanup _fragments
                    if (Debugger.IsAttached)
                        Debugger.Break();
                }

                current = new Fragment(ns, moreData, fragment);
                buffer.Enqueue(current);
                return true;
            }
            buffer.Enqueue(new Fragment(ns, moreData, fragment));
            if ((lln == 1 && buffer.Count > 1) || buffer.Count > 3)
            {
                var previous = buffer.Dequeue();
                if (previous.MoreData)
                {
                    if (!_retransmitFragments.TryGetValue(lln, out var fragments))
                    {
                        fragments = new List<Fragment>();
                        _retransmitFragments.Add(lln, fragments);
                    }
                    fragments.Add(previous);
                }
                else
                {
                    _retransmitFragments.Remove(lln);
                }
            }
            return false;
        }

        public void AddFragment(byte lln, byte ns, ReadOnlyMemory<byte> fragment, out bool retransmit)
        {
            retransmit = IsRetransmit(lln, ns, fragment, true);
            if (!_fragments.TryGetValue(lln, out var fragments))
            {
                fragments = new List<Fragment>();
                _fragments.Add(lln, fragments);
            }
            fragments.Add(new Fragment(ns, true, fragment));
        }

        public void RemoveFragment(byte lln, byte ns)
        {
            if (_fragments.TryGetValue(lln, out var fragments))
            {
                foreach (var fragment in fragments)
                {
                    if (fragment.Ns == ns)
                    {
                        fragments.Remove(fragment);
                        break;
                    }
                }
            }
            if (_retransmits.TryGetValue(lln, out var retransmits))
            {
                for (int i = 0; i < retransmits.Count; i++)
                {
                    var retransmit = retransmits.Dequeue();
                    if (retransmit.Ns == ns)
                    {
                        i++;
                        continue;
                    }
                    retransmits.Enqueue(retransmit);
                }
            }
        }

        public ReadOnlyMemory<byte> Reassemble(byte lln, byte ns, in ReadOnlyMemory<byte> fragment, out bool retransmit)
        {
            retransmit = IsRetransmit(lln, ns, fragment, false);
            if (!_fragments.TryGetValue(lln, out var fragments))
                return fragment;
            fragments.Add(new Fragment(ns, false, fragment));
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