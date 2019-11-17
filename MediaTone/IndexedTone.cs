
namespace RfpProxy.MediaTone
{
    public class IndexedTone
    {
        public RelativeTone Tone { get; }

        public int Index { get; }

        public int Count { get; set; } = 0;

        public IndexedTone(RelativeTone tone, int index)
        {
            Tone = tone;
            Index = index;
        }
    }
}