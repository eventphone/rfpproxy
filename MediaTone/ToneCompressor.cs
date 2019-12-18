using System;
using System.Collections.Generic;
using System.Linq;
using RfpProxy.AaMiDe.Media;

namespace RfpProxy.MediaTone
{
    public class ToneCompressor
    {
        private readonly MediaToneMessage.Tone[] _tones;
        private readonly int _limit;

        public ToneCompressor(MediaToneMessage.Tone[] tones, int limit = Int32.MaxValue)
        {
            _tones = tones;
            _limit = limit;
        }

        public static IEnumerable<MediaToneMessage.Tone> Decompress(MediaToneMessage.Tone[] tones)
        {
            var relativeTones = tones.Select(x=>new RelativeTone(x)).ToArray();
            int index = 0;
            ushort next = 1;
            do
            {
                var tone = relativeTones[index];
                var last = tone.Tone(0, 0, next++);
                yield return last;
                if (tone.CycleCount != 0)
                {
                    tone.CycleCount--;
                    index = tone.CycleTo;
                }
                else
                {
                    index = tone.Next;
                }
            } while (index < tones.Length);
        }

        public MediaToneMessage.Tone[] Compress()
        {
            var tones = _tones.AsSpan();
            var relative = Relative(tones);
            relative = Merge(relative);
            int maxMatchSize = Int32.MaxValue;
            while (maxMatchSize > 0)
            {
                var indexed = CountDuplicates(relative);
                var sequences = FindSequences(indexed);
                var match = FindMatch(sequences, maxMatchSize);
                if (match.Count == 0)
                {
                    break;
                }
                var result = ReplaceMatch(match, indexed);
                if (result == null)
                {
                    maxMatchSize = match.Max(x => x.Length) - 1;
                }
                else
                {
                    relative = result;
                }
            }
            if (relative.Length > _limit)
            {
                var indexed = CountDuplicates(relative);
                relative = Limit(indexed);
            }
            var maximum = relative;
            if (_limit < Int32.MaxValue)
            {
                var length = Decompress(Absolute(relative)).Count();
                do
                {
                    maximum = relative;
                    if (length > tones.Length)
                    {
                        break;
                    }
                    relative = Relative(tones.Slice(0, length));
                    relative = Merge(relative);
                    maxMatchSize = Int32.MaxValue;
                    while (maxMatchSize > 0)
                    {
                        var indexed = CountDuplicates(relative);
                        var sequences = FindSequences(indexed);
                        var match = FindMatch(sequences, maxMatchSize);
                        if (match.Count == 0)
                        {
                            break;
                        }
                        var result = ReplaceMatch(match, indexed);
                        if (result == null)
                        {
                            maxMatchSize = match.Max(x => x.Length) - 1;
                        }
                        else
                        {
                            relative = result;
                        }
                    }
                    length++;
                } while (relative.Length <= _limit);
            }
            return Absolute(maximum);
        }

        private RelativeTone[] Limit(IndexedTone[] tones)
        {
            var indexed = tones.AsMemory(0, _limit);
            while (HasCycleIntoBoundary(indexed, indexed.Length+1, indexed.Length+1))
            {
                indexed = indexed.Slice(0, indexed.Length - 2);
            }
            var result = new RelativeTone[indexed.Length];
            for (int i = 0; i < indexed.Length; i++)
            {
                result[i] = indexed.Span[i].Tone;
            }
            return result;
        }

        private static RelativeTone[] ReplaceMatch(List<SequenceMatch> matches, IndexedTone[] indexed)
        {
            foreach (var match in matches)
            {
                if (!CanBeReplaced(match, indexed, out var adjustBeforeCycles, out var adjustAfterCycles, out var cycleIntoBefore))
                {
                    continue;
                }
                var leftEnd = match.Left.Span[match.Length - 1];
                if (leftEnd.Tone.Next != 1)
                {
                    throw new NotImplementedException();
                }
                if (leftEnd.Tone.CycleCount != 0)
                {
                    if (!match.IsGapless)
                    {
                        throw new NotImplementedException();
                    }
                }

                var rightEnd = match.Right.Span[match.Length - 1];
                if (rightEnd.Tone.Next != 1 || rightEnd.Tone.CycleCount != 0)
                {
                    throw new NotImplementedException();
                }

                var leftStart = match.Left.Span[0];
                if (leftStart.Tone.Next != 1 || leftStart.Tone.CycleCount != 0)
                {
                    throw new NotImplementedException();
                }

                var rightStart = match.Right.Span[0];
                if (rightStart.Tone.Next != 1 || rightStart.Tone.CycleCount != 0)
                {
                    throw new NotImplementedException();
                }
                
                var before = indexed.AsMemory(0, match.Start);
                var after = indexed.AsMemory(match.End+1);
                Memory<IndexedTone> between;
                if (match.IsGapless)
                {
                    between = Memory<IndexedTone>.Empty;
                    leftEnd.Tone.CycleTo = 1-match.Length;
                    leftEnd.Tone.CycleCount++;
                }
                else
                {
                    between = indexed.AsMemory(match.Start + match.Length, match.Between);
                    leftEnd.Tone.CycleTo = 1;
                    leftEnd.Tone.CycleCount = 1;
                    leftEnd.Tone.Next = between.Length + 1;
                    var lastBetween = between.Span[between.Length - 1];
                    lastBetween.Tone.CycleTo = 1; //marker
                    lastBetween.Tone.Next = 1 - between.Length - match.Length;
                }
                var result = new List<RelativeTone>();
                foreach (var tone in before.Span)
                {
                    if (adjustBeforeCycles)
                    {
                        var next = tone.Tone.Next + tone.Index;
                        if (next > before.Length)
                        {
                            tone.Tone.Next -= match.Length;
                        }
                        var cycle = tone.Tone.CycleTo + tone.Index;
                        if (cycle > before.Length)
                        {
                            tone.Tone.CycleTo -= match.Length;
                        }
                    }
                    result.Add(tone.Tone);
                }
                foreach (var tone in match.Left.Span)
                {
                    result.Add(tone.Tone);
                }
                if (cycleIntoBefore)
                {
                    var injected = new RelativeTone(leftEnd.Tone.Tone(), 1, -match.Length, 1 + between.Length);
                    leftEnd.Tone.Next++;
                    var beforeLeftEnd = match.Left.Span[match.Length - 2];
                    beforeLeftEnd.Tone.CycleCount = 2;
                    beforeLeftEnd.Tone.CycleTo = 2;
                    if (!match.IsGapless)
                    {
                        injected.CycleTo = 1; //between
                        injected.CycleCount = 1;
                        injected.Next = 1 + between.Length; //after between
                        var lastBetween = between.Span[between.Length - 1];
                        lastBetween.Tone.Next--;
                        leftEnd.Tone.CycleTo++;
                    }
                    result.Add(injected);
                }
                foreach (var tone in between.Span)
                {
                    result.Add(tone.Tone);
                }
                foreach (var tone in after.Span)
                {
                    if (adjustAfterCycles)
                    {
                        var next = tone.Tone.Next + tone.Index;
                        if (next < before.Length)
                        {
                            //pointed to before
                            if (cycleIntoBefore)
                            {
                                tone.Tone.Next += match.Length - 1;
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        else if (next < before.Length + match.Length + between.Length)
                        {
                            //pointed to between
                            tone.Tone.Next += match.Length;
                        }
                        else if (next < before.Length + 2 * match.Length + between.Length)
                        {
                            //pointed to right
                            tone.Tone.Next -= between.Length;
                        }
                        var cycle = tone.Tone.CycleTo + tone.Index;
                        if (cycle < before.Length)
                        {
                            //pointed to before
                            if (cycleIntoBefore)
                            {
                                tone.Tone.CycleTo += match.Length - 1;
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        else if (cycle < before.Length + match.Length + between.Length)
                        {
                            //pointed to between
                            tone.Tone.CycleTo += match.Length;
                        }
                        else if (cycle < before.Length + 2 * match.Length + between.Length)
                        {
                            //pointed to right
                            tone.Tone.CycleTo -= between.Length;
                        }
                    }
                    result.Add(tone.Tone);
                }
                return result.ToArray();
            }
            return null;
        }

        private static bool CanBeReplaced(SequenceMatch match, IndexedTone[] indexed, out bool adjustBefore,  out bool adjustAfter, out bool cycleIntoBefore)
        {
            adjustBefore = false;
            adjustAfter = false;
            cycleIntoBefore = false;
            if (HasCycle(match.Left.Span))
            {
                if (!match.IsGapless)
                {
                    return false;
                }
                if (match.Length < 2)
                {
                    return false;
                }
                if (HasCycle(match.Left.Span.Slice(0, match.Length - 2)))
                {
                    return false;
                }
                var last = match.Left.Span[match.Length - 1];
                if (last.Tone.CycleTo != 1 - match.Length)
                {
                    return false;
                }
                if (last.Tone.Next != 1)
                {
                    return false;
                }
            }
            if (HasCycle(match.Right.Span))
                return false;
            var after = indexed.AsMemory(match.End+1);
            if (!IsContained(after.Span))
            {
                //cannot point to before match or in left part of match
                var boundary = match.Start + match.Length;
                if (!HasCycleIntoBoundary(after, 0, boundary))
                {
                    adjustAfter = true;
                }
                else
                {
                    //cannot point into match
                    if (HasCycleIntoBoundary(after, match.Start, match.End))
                    {
                        return false;
                    }
                    if (match.Length < 2)
                    {
                        return false;
                    }
                    var cycleCountIntoBefore = CountCycleIntoBoundary(after, 0, match.Start);
                    if (cycleCountIntoBefore > 1)
                    {
                        return false;
                    }
                    cycleIntoBefore = true;
                    adjustAfter = true;
                }
            }
            var before = indexed.AsMemory(0, match.Start);
            if (!IsContained(before.Span))
            {
                //cannot point inside match
                var upper = match.End;
                var lower = match.Start;
                if (HasCycleIntoBoundary(before, lower, upper))
                    return false;
                adjustBefore = true;
            }
            if (!match.IsGapless)
            {
                var between = indexed.AsMemory(match.Start + match.Length, match.Between);
                if (!IsContained(between.Span))
                    return false;
                if (HasCycle(between.Span))
                    return false;
            }

            return true;
        }

        private static int CountCycleIntoBoundary(Memory<IndexedTone> tones, int lower, int upper)
        {
            var result = 0;
            for (int i = 0; i < tones.Length; i++)
            {
                var tone = tones.Span[i];
                var cycle = tone.Tone.CycleTo + tone.Index;
                if (cycle >= lower && cycle <= upper)
                {
                    result += tone.Tone.CycleCount;
                }
                var next = tone.Tone.Next + tone.Index;
                if (next >= lower && next <= upper)
                {
                    result++;
                }
            }
            return result;
        }

        private static bool HasCycleIntoBoundary(Memory<IndexedTone> tones, int lower, int upper)
        {
            for (int i = 0; i < tones.Length; i++)
            {
                var tone = tones.Span[i];
                var cycle = tone.Tone.CycleTo + tone.Index;
                if (cycle >= lower && cycle <= upper)
                {
                    return true;
                }
                var next = tone.Tone.Next + tone.Index;
                if (next >= lower && next <= upper)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasCycle(Span<IndexedTone> tones)
        {
            for (var i = 0; i < tones.Length; i++)
            {
                var tone = tones[i];
                if (tone.Tone.CycleCount != 0) return true;
                if (tone.Tone.Next != 1) return true;
            }
            return false;
        }

        private static bool IsContained(Span<IndexedTone> tones)
        {
            for (int i = 0; i < tones.Length; i++)
            {
                var tone = tones[i].Tone;
                var next = tone.Next + i;
                if (next < 0 || next > tones.Length)
                {
                    return false;
                }
                var cycle = tone.CycleTo + i;
                if (cycle < 0 || cycle > tones.Length)
                {
                    return false;
                }
            }
            return true;
        }

        private List<SequenceMatch> FindMatch(List<Memory<IndexedTone>> sequences, int maxMatchSize)
        {
            var result = new List<SequenceMatch>();
            sequences.Sort((a, b) => 0-a.Length.CompareTo(b.Length));
            int max = 1;
            foreach (var smaller in sequences)
            {
                if (smaller.Length < max) break;
                foreach (var larger in sequences)
                {
                    int length = smaller.Length;
                    if (smaller.Span[0].Index == larger.Span[0].Index)
                    {
                        length = length / 2;
                    }
                    if (length > maxMatchSize)
                        length = maxMatchSize;
                    for (; length >= max; length--)
                    {
                        for (int largerOffset = 0; largerOffset <= larger.Length-length; largerOffset++)
                        {
                            for (int smallerOffset = 0; smallerOffset <= smaller.Length-length; smallerOffset++)
                            {
                                var left = larger.Slice(largerOffset, length);
                                var right = smaller.Slice(smallerOffset, length);
                                if (left.Span[0].Index > right.Span[0].Index)
                                    continue;
                                if (right.Span[0].Index < left.Span[0].Index + length)
                                {
                                    smallerOffset += (left.Span[0].Index + length) - right.Span[0].Index - 1;
                                    continue;
                                }
                                if (Matches(left, right))
                                {
                                    max = length;
                                    if (result.Count == 0 || result[result.Count-1].Length <= length)
                                    {
                                        result.Add(new SequenceMatch(left, right));
                                    }
                                }
                            }
                        }
                    }
                    if (smaller.Span[0].Index == larger.Span[0].Index) break;
                }
            }
            return result.OrderByDescending(x => x.Length)
                .ThenBy(x => Math.Abs(x.Left.Span[0].Index - x.Right.Span[0].Index))
                .ToList();
        }

        private static bool Matches(Memory<IndexedTone> left, Memory<IndexedTone> right)
        {
            if (left.Length != right.Length) return false;
            if (!ToneEqual(left.Span[0].Tone, right.Span[0].Tone)) return false;
            for (int i = 1; i < left.Length-1; i++)
            {
                if (!Equal(left.Span[i].Tone, right.Span[i].Tone)) return false;
            }
            if (!ToneEqual(left.Span[left.Length - 1].Tone, right.Span[right.Length - 1].Tone)) return false;
            return true;
        }
        
        private static List<Memory<IndexedTone>> FindSequences(IndexedTone[] indexed)
        {
            var result = new List<Memory<IndexedTone>>();
            var start = 0;
            for (int i = 0; i < indexed.Length; i++)
            {
                if (indexed[i].Count == 0)
                {
                    if (start != i)
                    {
                        result.Add(indexed.AsMemory(start, i - start));
                    }
                    start = i + 1;
                }
            }
            if (start < indexed.Length)
            {
                result.Add(indexed.AsMemory(start));
            }
            return result;
        }

        private static MediaToneMessage.Tone[] Absolute(RelativeTone[] tones)
        {
            var result = new MediaToneMessage.Tone[tones.Length];
            for (int i = 0; i < tones.Length; i++)
            {
                result[i] = tones[i].Tone(i);
            }
            return result;
        }

        private static RelativeTone[] Relative(Span<MediaToneMessage.Tone> tones)
        {
            var result = new RelativeTone[tones.Length];
            for (int i = 0; i < tones.Length; i++)
            {
                result[i] = new RelativeTone(tones[i],
                    tones[i].CycleCount,
                    tones[i].CycleCount == 0 ? 0 : tones[i].CycleTo - i,
                    (tones[i].Next - i));
            }
            return result;
        }

        public static MediaToneMessage.Tone[] Merge(MediaToneMessage.Tone[] tones)
        {
            return Absolute(Merge(Relative(tones)));
        }

        private static RelativeTone[] Merge(RelativeTone[] tones)
        {
            RelativeTone last = tones[0];
            var result = new List<RelativeTone> {last};
            for (int i = 1; i < tones.Length; i++)
            {
                if (Equal(last, tones[i]))
                {
                    //merge
                    last.Duration += tones[i].Duration;
                }
                else
                {
                    last = tones[i];
                    result.Add(last);
                }
            }
            return result.ToArray();
        }

        private static IndexedTone[] CountDuplicates(RelativeTone[] tones)
        {
            var result = tones.Select((x, i) => new IndexedTone(x, i)).ToArray();
            foreach (var left in result)
            {
                foreach (var right in result)
                {
                    if (left == right) break;
                    if (ToneEqual(left.Tone, right.Tone))
                    {
                        left.Count++;
                        right.Count++;
                    }
                }
            }
            return result;
        }

        private static bool Equal(RelativeTone left, RelativeTone right)
        {
            return ToneEqual(left, right) &&
                   left.CycleTo == right.CycleTo &&
                   left.CycleCount == right.CycleCount &&
                   left.Next == right.Next;
        }

        private static bool ToneEqual(RelativeTone left, RelativeTone right)
        {
            if (left == null) return false;
            if (right == null) return false;
            return left.CB1 == right.CB1 &&
                   left.CB2 == right.CB2 &&
                   left.CB3 == right.CB3 &&
                   left.CB4 == right.CB4 &&
                   left.Frequency1 == right.Frequency1 &&
                   left.Frequency2 == right.Frequency2 &&
                   left.Frequency3 == right.Frequency3 &&
                   left.Frequency4 == right.Frequency4 &&
                   left.Duration == right.Duration;
        }
    }
}