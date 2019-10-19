using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using RfpProxyLib.AaMiDe.Media;

namespace SuperMarioBrothers
{
    public class MidiReader
    {
        private readonly MidiFile _midi;
        public MidiReader(string name)
        {
            var assembly = typeof(MidiReader).Assembly;
            var stream = assembly.GetManifestResourceStream("SuperMarioBrothers." + name + ".mid");
            _midi = MidiFile.Read(stream);
        }

        public IEnumerable<MediaToneMessage.Tone> GetTones()
        {
            return GetTones(ExtractChords(), _midi.GetTempoMap());
        }
        
        private IEnumerable<ILengthedObject[]> ExtractChords()
        {
            var result = new List<Note>();
            var position = 0L;
            foreach (var entry in _midi.GetNotesAndRests(RestSeparationPolicy.NoSeparation))
            {
                if (entry is Rest rest)
                {
                    if (position == 0)
                    {
                        position = rest.Length;
                        continue;
                    }
                    foreach (var chords in ExtractUntil(result, rest.Time))
                    {
                        yield return chords;   
                    }
                    yield return new ILengthedObject[]{rest};
                    position = rest.Time + rest.Length;
                    continue;
                }
                if (entry is Chord)
                    throw new NotSupportedException();

                var note = (Note) entry;
                if (note.Time != position)
                {
                    foreach (var chords in ExtractUntil(result, note.Time))
                    {
                        yield return chords;   
                    }
                    position = note.Time;
                }
                var existing = result.FirstOrDefault(x => x.NoteNumber == note.NoteNumber);
                if (existing != null)
                {
                    if (existing.Velocity == note.Velocity && existing.Time + existing.Length == note.Time)
                    {
                        existing.Length += note.Length;
                        continue;
                    }
                    foreach (var chords in ExtractUntil(result, note.Time))
                    {
                        yield return chords;   
                    }
                    position = note.Time;
                }
                result.Add(note);
            }
        }
        
        private IEnumerable<Note[]> ExtractUntil(List<Note> notes, long end)
        {
            while (notes.Any(x=>x.Time < end))
            {
                var grouped = notes.GroupBy(x => x.Time).OrderBy(x => x.Key).First().OrderByDescending(x=>x.NoteNumber).ToArray();
                var length = grouped.Min(x => x.Length);
                if (grouped[0].Time + length > end)
                    length = end - grouped[0].Time;
                for (int i = 0; i < grouped.Length; i++)
                {
                    var note = grouped[i];
                    if (note.Length != length)
                    {
                        var split = note.Split(note.Time + length);
                        grouped[i] = split.LeftPart;
                        notes.Add(split.RightPart);
                    }
                    notes.Remove(note);
                }
                yield return grouped;
            }
        }

        private IEnumerable<MediaToneMessage.Tone> GetTones(IEnumerable<ILengthedObject[]> chords, TempoMap tempoMap)
        {
            ushort i = 1;
            foreach (var chord in chords)
            {
                var result = new RelativeTone();
                if (chord[0] is Rest rest)
                {
                    if (i == 0) continue;
                    result.Duration = GetDuration(rest.Length, tempoMap);
                }
                else
                {
                    //todo if (chord.Length > 4)
                    //    throw new ArgumentOutOfRangeException();
                    if (chord[0] is Note note1)
                    {
                        result.Frequency1 = GetFrequency(note1);
                        result.CB1 = GetVolume(note1.Velocity);
                        result.Duration = GetDuration(note1.Length, tempoMap);
                    }
                    if (chord.Length > 1 && chord[1] is Note note2)
                    {
                        result.Frequency2 = GetFrequency(note2);
                        result.CB2 = GetVolume(note2.Velocity);
                    }
                    if (chord.Length > 2 && chord[2] is Note note3)
                    {
                        result.Frequency3 = GetFrequency(note3);
                        result.CB3 = GetVolume(note3.Velocity);
                    }
                    if (chord.Length > 3 && chord[3] is Note note4)
                    {
                        result.Frequency4 = GetFrequency(note4);
                        result.CB4 = GetVolume(note4.Velocity);
                    }
                }
                if (result.Duration == 0)
                    continue;
                result.Next = i++;
                yield return result.Tone();
            }
        }

        private static short GetVolume(SevenBitNumber velocity)
        {
            return (short) (-800 + (830 / 127d) * velocity);
        }

        private static ushort GetDuration(long length, TempoMap tempoMap)
        {
            return (ushort) (LengthConverter.ConvertTo<MetricTimeSpan>(length, 0, tempoMap).TotalMicroseconds / 1000);
        }

        private static ushort GetFrequency(Note note)
        {
            //https://en.wikipedia.org/wiki/MIDI_tuning_standard#Frequency_values
            var f = 440 * Math.Pow(2, (note.NoteNumber - 57) / 12d);
            return (ushort) f;
        }
    }
}