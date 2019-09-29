using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RfpProxyLib.AaMiDe.Media;

namespace SuperMarioBrothers
{
    public class ToneCompressor
    {
        private readonly MediaToneMessage.Tone[] _tones;

        public ToneCompressor(MediaToneMessage.Tone[] tones)
        {
            _tones = tones;
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
            var relative = Relative(_tones);
            int i = 0;
            while (i < 5)
            {
                var indexed = CountDuplicates(relative);
                var sequences = FindSequences(indexed);
                var match = FindMatch(sequences);
                if (match == null)
                {
                    break;
                }
                var result = ReplaceMatch(match, indexed);
                if (result == null)
                {
                    break;
                }
                relative = result;
                i++;
            }
            return Absolute(relative);
        }

        private RelativeTone[] ReplaceMatch(List<SequenceMatch> matches, IndexedTone[] indexed)
        {
            foreach (var match in matches)
            {
                var left = match.Left;
                var leftEnd = left.Span[match.Length - 1];
                if (leftEnd.Tone.Next == 1)
                {
                    var right = match.Right;
                    var rightStart = right.Span[0];
                    if (rightStart.Index <= leftEnd.Index)
                    {
                        left = left.Slice(leftEnd.Index - rightStart.Index + 1);
                        right = right.Slice(leftEnd.Index - rightStart.Index + 1);
                        rightStart = right.Span[0];
                    }
                    var rightEnd = right.Span[right.Length - 1];
                    if (leftEnd.Index + 1 == rightStart.Index)
                    {
                        if (leftEnd.Tone.CycleCount != 0)
                            throw new NotImplementedException();

                        leftEnd.Tone.CycleCount++;
                        leftEnd.Tone.CycleTo = 1 - right.Length;
                        var leftStart = left.Span[0];
                        var result = new RelativeTone[indexed.Length - right.Length];
                        int i = 0;
                        for (; i < leftEnd.Index; i++)
                        {
                            var tone = indexed[i].Tone;
                            if (tone.CycleTo != 0)
                            {
                                if (tone.CycleTo + i > leftEnd.Index)
                                {
                                    throw new NotImplementedException();
                                }
                            }
                            if (tone.Next != 1)
                            {
                                if (tone.Next + i > rightEnd.Index)
                                {
                                    tone.Next -= right.Length;
                                }
                                else if (tone.Next + i > leftStart.Index)
                                {
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                }
                            }
                            result[i] = tone;
                        }
                        result[i++] = leftEnd.Tone;
                        for (; i < result.Length; i++)
                        {
                            var tone = indexed[i + right.Length].Tone;
                            if (tone.CycleTo != 0)
                            {
                                if (tone.CycleTo + i < rightEnd.Index)
                                {
                                    if (tone.CycleTo + i < leftStart.Index)
                                    {
                                        tone.CycleTo += right.Length;
                                    }
                                    else if (tone.CycleTo + i > leftEnd.Index)
                                    {
                                        tone.CycleTo += right.Length;
                                    }
                                    else
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                            }
                            if (tone.Next < 1)
                            {
                                if (tone.Next + i < rightStart.Index)
                                {
                                    if (tone.Next + i < leftStart.Index)
                                    {
                                        tone.Next += right.Length;
                                    }
                                    else
                                    {
                                        throw new NotImplementedException();
                                    }

                                }
                            }
                            else if (tone.Next > result.Length - i)
                            {
                                throw new NotImplementedException();
                            }
                            result[i] = tone;
                        }
                        return result;
                    }
                    else if (leftEnd.Tone.CycleCount == 0)
                    {
                        var betweenMatch = indexed.Where(x => x.Index > leftEnd.Index)
                            .Where(x => x.Index < rightStart.Index)
                            .OrderBy(x => x.Index)
                            .Select(x => x.Tone)
                            .ToArray();
                        var last = betweenMatch.Last();
                        if (last.CycleCount == 0 && last.Next == 1)
                        {
                            var leftStart = left.Span[0];


                            var result = new RelativeTone[indexed.Length - right.Length];
                            int i = 0;
                            for (; i < leftEnd.Index; i++)
                            {
                                var tone = indexed[i].Tone;
                                if (tone.CycleTo != 0)
                                {
                                    if (tone.CycleTo + i > leftEnd.Index)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                                if (tone.Next != 1)
                                {
                                    if (tone.Next + i > rightEnd.Index)
                                    {
                                        tone.Next -= right.Length;
                                    }
                                    else if (tone.Next + i > leftStart.Index)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                                result[i] = tone;
                            }
                            result[i++] = leftEnd.Tone;
                            for (; i < rightStart.Index; i++)
                            {
                                var tone = indexed[i].Tone;
                                if (tone.CycleTo > 0)
                                {
                                    if (tone.CycleTo + i > rightStart.Index)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                                if (tone.Next > 1)
                                {
                                    if (tone.Next + i > rightStart.Index)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                                result[i] = tone;
                            }
                            for (; i < result.Length; i++)
                            {
                                var tone = indexed[i + right.Length].Tone;
                                if (tone.CycleTo < 0)
                                {
                                    if (tone.CycleTo + i < rightStart.Index)
                                    {
                                        //we inserted a cycle in between, so we can't cycle over this again
                                        break;
                                    }
                                    else if (tone.CycleTo + i < rightEnd.Index)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                                if (tone.Next < 0)
                                {
                                    if (tone.Next + i < rightStart.Index)
                                    {
                                        //we inserted a cycle in between, so we can't cycle over this again
                                        break;
                                    }
                                    else if (tone.Next + i < rightEnd.Index)
                                    {
                                        throw new NotImplementedException();
                                    }
                                }
                                result[i] = tone;
                            }
                            if (i == result.Length)
                            {
                                var next = betweenMatch.Length + 1;
                                leftEnd.Tone.CycleCount = 1;
                                leftEnd.Tone.CycleTo = 1;
                                leftEnd.Tone.Next = next;

                                next = leftStart.Index - rightStart.Index + 1;

                                last.Next = next;
                                last.CycleTo = next;
                                last.CycleCount = 1;
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return null;
        }

        private List<SequenceMatch> FindMatch(List<Memory<IndexedTone>> sequences)
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
                    for (; length >= max; length--)
                    {
                        for (int largerOffset = 0; largerOffset <= larger.Length-length; largerOffset++)
                        {
                            for (int smallerOffset = 0; smallerOffset < smaller.Length-length; smallerOffset++)
                            {
                                var left = larger.Slice(largerOffset, length);
                                var right = smaller.Slice(smallerOffset, length);
                                if (left.IsEmpty)
                                    Debugger.Break();
                                if (right.IsEmpty)
                                    Debugger.Break();
                                if (left.Span[0].Index > right.Span[0].Index)
                                    continue;
                                if (left.Span[0].Index == right.Span[0].Index) continue;
                                if (right.Span[0].Index < left.Span[0].Index + length)
                                {
                                    smallerOffset += (left.Span[0].Index + length) - right.Span[0].Index - 1;
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

        private bool Matches(Memory<IndexedTone> left, Memory<IndexedTone> right)
        {
            if (left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++)
            {
                if (!Equal(left.Span[i].Tone, right.Span[i].Tone)) return false;
            }
            return true;
        }
        
        private List<Memory<IndexedTone>> FindSequences(IndexedTone[] indexed)
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

        private MediaToneMessage.Tone[] Absolute(RelativeTone[] tones)
        {
            var result = new MediaToneMessage.Tone[tones.Length];
            for (int i = 0; i < tones.Length; i++)
            {
                result[i] = tones[i].Tone(i);
            }
            return result;
        }

        private static RelativeTone[] Relative(MediaToneMessage.Tone[] tones)
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

        private static IndexedTone[] CountDuplicates(RelativeTone[] tones)
        {
            var result = tones.Select((x, i) => new IndexedTone(x, i)).ToArray();
            foreach (var left in result)
            {
                foreach (var right in result)
                {
                    if (left == right) break;
                    if (Equal(left.Tone, right.Tone))
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