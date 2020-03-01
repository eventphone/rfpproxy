using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxyLib
{
    public class OmmConfReader:IDisposable
    {
        private readonly Stream _config;
        private readonly Dictionary<string, List<List<(string, string)>>> _sections;

        public OmmConfReader(string filename):this(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
        }

        public OmmConfReader(Stream config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sections = new Dictionary<string, List<List<(string, string)>>>();
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
                string[] header = null;
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
                        header = previous.Split('|');
                        for (int i = 0; i < header.Length; i++)
                        {
                            header[i] = header[i].TrimEnd();
                        }
                    }
                    else if (!(header is null))
                    {
                        var values = current.Split('|');
                        if (values.Length != header.Length)
                            throw new InvalidDataException("number of fields differs");
                        var data = new List<(string, string)>(values.Length - 1);
                        for (int i = 1; i < values.Length; i++)
                        {
                            data.Add((header[i], values[i].TrimEnd()));
                        }
                        var section = AddSection(values[0]);
                        section.Add(data);
                    }
                    previous = current;
                }
            }
        }

        public async Task<IEnumerable<IList<(string, string)>>> GetSectionAsync(string section, CancellationToken cancellationToken)
        {
            if (_sections.Count == 0)
            {
                await ParseAsync(cancellationToken).ConfigureAwait(false);
            }
            if (!_sections.TryGetValue(section, out var values))
                return Array.Empty<IList<(string, string)>>();
            return values;
        }

        public async Task<IList<(string, string)>> GetValueAsync(string section, string field, string value, CancellationToken cancellationToken)
        {
            var values = await GetSectionAsync(section, cancellationToken).ConfigureAwait(false);
            foreach (var entry in values)
            {
                bool keyFound = false;
                foreach (var (k,v) in entry)
                {
                    if (k == field)
                    {
                        if (v == value) return entry;
                        keyFound = true;
                    }
                }
                if (!keyFound) break;
            }
            return null;
        }

        private List<List<(string, string)>> AddSection(string section)
        {
            if (!_sections.TryGetValue(section, out var result))
            {
                result = new List<List<(string, string)>>();
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