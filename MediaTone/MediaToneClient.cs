using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RfpProxy.AaMiDe.Media;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.MediaTone
{
    public class MediaToneClient : ProxyClient
    {
        private readonly List<ValueTuple<string, MediaToneMessage.Tone[]>> _audio;
        private readonly ConcurrentDictionary<ushort, string> _mediaHandles;

        public MediaToneClient(string socket):base(socket)
        {
            _audio = new List<ValueTuple<string, MediaToneMessage.Tone[]>>();
            _mediaHandles = new ConcurrentDictionary<ushort, string>();
        }

        public async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            ReadMidiFiles(cancellationToken);
            await AddListenAsync("000000000000", "000000000000", "0202", "ffff", cancellationToken);//MEDIA_CLOSE
            await AddListenAsync("000000000000", "000000000000", "020900000000000000030000", "ffff00000000000000ff0000", cancellationToken);//MEDIA_DTMF
            Console.WriteLine("up & running");
        }

        protected override Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            if (data.Length < 6) return Task.CompletedTask;
            var hdl = BinaryPrimitives.ReadUInt16LittleEndian(data.Span.Slice(4));
            if (data.Span[1] == 2)
            {
                //MEDIA_CLOSE
                OnClose(hdl);
            }
            else if (data.Span[1] == 9)
            {
                //MEDIA_DTMF
                return OnDtmfAsync(rfp, hdl, data, cancellationToken);
            }
            return Task.CompletedTask;
        }

        private Task OnDtmfAsync(RfpIdentifier rfp, ushort handle, Memory<byte> data, CancellationToken cancellationToken)
        {
            if (data.Length < 7) return Task.CompletedTask;
            var key = (char) data.Span[6 + 2];
            Console.WriteLine($"{handle:X4}: key {key} pressed");
            var state = _mediaHandles.AddOrUpdate(handle, x => key.ToString(), (k, v) => v + key);
            return InjectAsync(rfp, handle, state, cancellationToken);
        }

        private Task InjectAsync(RfpIdentifier rfp, ushort handle, string state, CancellationToken cancellationToken)
        {
            foreach (var (number, tones) in _audio)
            {
                if (state.EndsWith(number))
                {
                    Console.WriteLine($"{handle:X4}: injecting {number}");
                    return InjectAsync(rfp, handle, tones, cancellationToken);
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
            Console.WriteLine($"{handle:X4}: state {state} not found");
            return Task.CompletedTask;
        }

        private async Task InjectAsync(RfpIdentifier rfp, ushort handle, MediaToneMessage.Tone[] tones, CancellationToken cancellationToken)
        {
            var message = new MediaToneMessage(handle, MediaDirection.TxRx, 0, Array.Empty<MediaToneMessage.Tone>());
            var data = new byte[message.Length];
            message.Serialize(data);
            cancellationToken.ThrowIfCancellationRequested();
            await WriteAsync(MessageDirection.ToRfp, 0, rfp, data, cancellationToken);
            message = new MediaToneMessage(handle, MediaDirection.TxRx, 0, tones);
            data = new byte[message.Length];
            message.Serialize(data);
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(1000, cancellationToken);
            await WriteAsync(MessageDirection.ToRfp, 0, rfp, data, cancellationToken);
        }

        private void OnClose(ushort handle)
        {
            _mediaHandles.TryRemove(handle, out _);
            Console.WriteLine($"{handle:X4}: closed");
        }

        private void ReadMidiFiles(CancellationToken cancellationToken = default(CancellationToken))
        {
            ReadMidiFile("amelie", "263543");
            cancellationToken.ThrowIfCancellationRequested();
            ReadMidiFile("bach", "2224");
            cancellationToken.ThrowIfCancellationRequested();
            ReadMidiFile("elise", "35473");
            cancellationToken.ThrowIfCancellationRequested();
            ReadMidiFile("portal", "767825");
            cancellationToken.ThrowIfCancellationRequested();
            ReadMidiFile("tetris", "838747");
            cancellationToken.ThrowIfCancellationRequested();
            ReadMidiFile("smb", "762");
            _audio.Add(("**##",
                new[]
                {
                    new MediaToneMessage.Tone(34, 34, 34, 34, 19000, 17000, 20000, 18000, UInt16.MaxValue, 0, 0, 0)
                }));
        }

        private void ReadMidiFile(string name, string number)
        {
            Console.WriteLine($"preparing {name}");
            var reader = new MidiReader(name);
            var tones = reader.GetTones().ToArray();
            var compressor = new ToneCompressor(tones, 253);
            tones = compressor.Compress();
            tones = tones
                .Concat(new[]
                {
                    new MediaToneMessage.Tone(34, 34, 34, 34, 0, 0, 0, 0, UInt16.MaxValue, 0, 0,
                        (ushort) (tones.Length + 1)),
                    new MediaToneMessage.Tone(34, 34, 34, 34, 0, 0, 0, 0, UInt16.MaxValue, 0, 0, (ushort) tones.Length)
                })
                .ToArray();
            _audio.Add((number, tones));
        }
    }
}