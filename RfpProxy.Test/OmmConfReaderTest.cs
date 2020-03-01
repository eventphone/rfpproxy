using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RfpProxyLib;
using Xunit;
using Xunit.Abstractions;

namespace RfpProxy.Test
{
    public class OmmConfReaderTest
    {
        private readonly ITestOutputHelper _output;

        public OmmConfReaderTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task CanReadOmmConfig()
        {
            using (var reader = new OmmConfReader("omm_conf.txt"))
            {
                await reader.ParseAsync(CancellationToken.None);
            }
        }

        [Fact]
        public async Task CanGetSection()
        {
            using (var reader = new OmmConfReader("omm_conf.txt"))
            {
                var section = await reader.GetSectionAsync("RFPA", CancellationToken.None);
                Assert.NotNull(section);
                Assert.NotEmpty(section);
            }
        }

        [Fact]
        public async Task CanGetEntry()
        {
            using (var reader = new OmmConfReader("omm_conf.txt"))
            {
                var rfp = await reader.GetValueAsync("RFP", "mac", "0030421B1737", CancellationToken.None);
                var id = rfp.Where(x => x.Item1 == "id").Select(x => x.Item2).First();
                var rfpa = await reader.GetValueAsync("RFPA", "id", id, CancellationToken.None);
                var key = rfpa[1].Item2;
                Assert.Equal("5E00AFDF3295C6080FDD462B90AFF987D6726CC1B89DC64F7C1FA7AB747A997BFDD835D698C95D7F083105457755232581AA7F8AD5F41509E7BE7EBF1A1CD342", key);
            }
        }
    }
}