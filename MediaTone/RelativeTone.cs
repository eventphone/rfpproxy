using RfpProxy.AaMiDe.Media;

namespace RfpProxy.MediaTone
{
    public class RelativeTone
    {
        public short CB1 { get; set; }
        
        public short CB2 { get; set; }
        
        public short CB3 { get; set; }
        
        public short CB4 { get; set; }

        public ushort Frequency1 { get; set; }

        public ushort Frequency2 { get; set; }

        public ushort Frequency3 { get; set; }

        public ushort Frequency4 { get; set; }

        public ushort Duration { get; set; }

        public ushort CycleCount { get; set; }

        public int CycleTo { get; set; }

        public int Next { get; set; }

        public RelativeTone(ushort frequency1)
        {
            Frequency1 = frequency1;
            Next = 1;
        }

        public RelativeTone()
        {
        }

        public RelativeTone(MediaToneMessage.Tone tone)
            : this(tone, tone.CycleCount, tone.CycleTo, tone.Next)
        {
        }

        public RelativeTone(MediaToneMessage.Tone tone, ushort cycleCount, int cycleTo, int next)
        {
            CB1 = tone.CB1;
            CB2 = tone.CB2;
            CB3 = tone.CB3;
            CB4 = tone.CB4;
            Frequency1 = tone.Frequency1;
            Frequency2 = tone.Frequency2;
            Frequency3 = tone.Frequency3;
            Frequency4 = tone.Frequency4;
            Duration = tone.Duration;
            CycleCount = cycleCount;
            CycleTo = cycleTo;
            Next = next;
        }

        public MediaToneMessage.Tone Tone()
        {
            return new MediaToneMessage.Tone(CB1, CB2, CB3, CB4, Frequency1, Frequency2, Frequency3, Frequency4, Duration, CycleCount, (ushort) CycleTo, (ushort) Next);
        }

        public MediaToneMessage.Tone Tone(int index)
        {
            return new MediaToneMessage.Tone(CB1, CB2, CB3, CB4, Frequency1, Frequency2, Frequency3, Frequency4, Duration, CycleCount, CycleCount==0?(ushort)CycleTo:(ushort) (index + CycleTo), (ushort) (index + Next));
        }

        public MediaToneMessage.Tone Tone(ushort cycleCount, ushort cycleTo, ushort next)
        {
            return new MediaToneMessage.Tone(CB1, CB2, CB3, CB4, Frequency1, Frequency2, Frequency3, Frequency4, Duration, cycleCount, cycleTo, next);
        }
    }
}