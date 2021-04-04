using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxyLib
{
    public class OmmConfHeader
    {
        private readonly string[] _header;
        private readonly IDictionary<string, int> _indexed;

        public OmmConfHeader(string[] header)
        {
            _header = header;
            _indexed = new Dictionary<string, int>();
            for (int i = 1; i < header.Length; i++)
            {
                header[i] = header[i].TrimEnd();
                if (!_indexed.ContainsKey(header[i]))
                    _indexed.Add(header[i], i-1);
            }
        }

        public int IndexOf(string name)
        {
            return _indexed[name];
        }

        public bool TryIndexOf(string name, out int index)
        {
            return _indexed.TryGetValue(name, out index);
        }

        public string NameOf(int index)
        {
            return _header[index + 1];
        }
    }

    public class OmmConfEntry
    {
        private readonly OmmConfHeader _header;
        private readonly string[] _values;

        public OmmConfEntry(OmmConfHeader header, string[] values)
        {
            _header = header;
            _values = values;
        }

        public string Type => _values[0];

        public string this[string field]
        {
            get
            {
                if (!_header.TryIndexOf(field, out var i)) return null;
                return this[i].Trim();
            }
        }

        public string this[int i] => _values[i + 1].Trim();

        public override string ToString()
        {
            return $"{Type}: " + String.Join(", ", _values.Skip(1).Select((x, i) => $"{_header.NameOf(i)}:{this[i]}"));
        }
    }

    public class OmmConfReader:IDisposable
    {
        private static readonly byte[] HiddenMd5Data = {
            0x16, 0xFF, 0x50, 1, 0x13, 0xC0, 0x73, 0x34, 0x93,
            0x37, 0x70, 0x14, 0xFF, 0x4C, 0x20, 0x2E, 0xB, 0x28,
            0x21, 0xC, 0xBC, 0xC2, 0x60, 0xC0, 0x7F, 0x21, 0x3B,
            0xD6, 0x15, 0x38, 0x83, 5, 0xA0, 0, 0xFF, 0x11, 0x97,
            0x57, 0x18, 0xC9, 0x27, 0x8F, 0xF8, 0xFF, 0xA5, 0x72,
            0x89, 0x29, 0x12, 0x16, 0xE9, 0x34, 0xFF, 0xCD, 0x8B,
            0xFF, 0xF4, 0xB6, 0x10, 0x9B, 0, 0x8C, 3, 0x96, 0x32,
            0xD, 0x7F, 0x60, 0xFE, 0xFF, 0xDE, 0x72, 0x2E, 0x16,
            0xA6, 0xBF, 0xA0, 0x10, 0x83, 0xF0, 0xAC, 0x6A, 0x4B,
            0xC, 0xFF, 0xFF, 0x5F, 0xFE, 0xCB, 7, 0xB9, 0x5F, 0x53,
            0x1D, 0x48, 0x3C
        };
        private static readonly byte[] ByteOrderMark = {0xef, 0xbb, 0xbf};
        private static readonly byte[] LineBreak = {13, 10};

        private readonly Stream _config;
        private readonly Dictionary<string, List<OmmConfEntry>> _sections;

        public OmmConfReader(string filename):this(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
        }

        public OmmConfReader(Stream config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sections = new Dictionary<string, List<OmmConfEntry>>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _config.Dispose();
                _disposed = true;
            }
        }

        public async Task ParseAsync(CancellationToken cancellation)
        {
            _disposed = true;
            using (var sr = new StreamReader(_config, Encoding.UTF8))
            using (var md5 = MD5.Create())
            {
                md5.TransformBlock(ByteOrderMark, 0, ByteOrderMark.Length, null, 0);
                string previous = null;
                OmmConfHeader header = null;
                while (!sr.EndOfStream)
                {
                    var current = await sr.ReadLineAsync().ConfigureAwait(false);
                    if (current.Length == 0)
                    {
                        //new section
                        header = null;
                    }
                    else if (current.AsSpan().TrimStart('-').IsEmpty)
                    {
                        if (previous is null)
                            throw new InvalidDataException("omm_conf cannot start with separator line ---");
                        header = new OmmConfHeader(previous.Split('|'));
                    }
                    else if (!(header is null))
                    {
                        var values = current.Split('|');
                        var data = new OmmConfEntry(header, values);
                        var section = AddSection(data.Type);
                        section.Add(data);
                    }
                    if (previous != null )
                    {
                        var bytes = Encoding.UTF8.GetBytes(previous);
                        md5.TransformBlock(bytes, 0, bytes.Length, null, 0);
                        md5.TransformBlock(LineBreak, 0, LineBreak.Length, null, 0);
                    }
                    previous = current;
                }
                md5.TransformFinalBlock(HiddenMd5Data, 0, HiddenMd5Data.Length);
                var checksum = HexEncoding.ByteToHex(md5.Hash);
                if (previous != checksum)
                    throw new InvalidDataException("invalid checksum");
            }
        }

        public async Task<IEnumerable<OmmConfEntry>> GetSectionAsync(string section, CancellationToken cancellationToken)
        {
            if (_sections.Count == 0)
            {
                await ParseAsync(cancellationToken).ConfigureAwait(false);
            }
            if (!_sections.TryGetValue(section, out var values))
                return Array.Empty<OmmConfEntry>();
            return values;
        }

        public async Task<OmmConfEntry> GetValueAsync(string section, string field, string value, CancellationToken cancellationToken)
        {
            var values = await GetSectionAsync(section, cancellationToken).ConfigureAwait(false);
            foreach (var entry in values)
            {
                var current = entry[field];
                if (current == value) return entry;
                if (current is null) break;
            }
            return null;
        }

        private List<OmmConfEntry> AddSection(string section)
        {
            if (!_sections.TryGetValue(section, out var result))
            {
                result = new List<OmmConfEntry>();
                _sections.Add(section, result);
            }
            return result;
        }

        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}