using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            {
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
                        header = new OmmConfHeader(previous.Split('|'));
                    }
                    else if (!(header is null))
                    {
                        var values = current.Split('|');
                        var data = new OmmConfEntry(header, values);
                        var section = AddSection(data.Type);
                        section.Add(data);
                    }
                    previous = current;
                }
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