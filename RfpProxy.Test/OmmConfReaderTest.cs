using System;
using System.Buffers.Binary;
using System.Text;
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
                var id = rfp["id"];
                Assert.Equal("RFP: id:000, mac:0030421B1737, sit:1, location:zivillian, fl:0900,", rfp.ToString().Substring(0,66));
                var rfpa = await reader.GetValueAsync("RFPA", "id", id, CancellationToken.None);
                var key = rfpa[1];
                Assert.Equal("RFPA", rfpa.Type);
                Assert.Equal("5E00AFDF3295C6080FDD462B90AFF987D6726CC1B89DC64F7C1FA7AB747A997BFDD835D698C95D7F083105457755232581AA7F8AD5F41509E7BE7EBF1A1CD342", key);
                Assert.Equal("000", rfpa[0]);
                Assert.Throws<IndexOutOfRangeException>(()=>rfpa[2]);
            }
        }

        [Fact]
        public async Task NonExistentRfpDoesNotThrow()
        {
            using (var reader = new OmmConfReader("omm_conf.txt"))
            {
                var rfp = await reader.GetValueAsync("RFP", "mac", "abcdef012345", CancellationToken.None);
                Assert.Null(rfp);
            }
        }

        [Fact]
        public async Task CanDecryptAxiPassword()
        {
            using (var reader = new OmmConfReader("omm_conf.txt"))
            {
                var user = await reader.GetValueAsync("XAC", "name", "axi", CancellationToken.None);
                var passwd = user["passwd"];
                var bf_key = new byte[] {0x09, 0x6B, 0xA9, 0x87, 0x98, 0x90, 0xB6, 0x7A, 0x18, 0x93, 0xE3, 0x97, 0xB9, 0x77, 0xF3,};
                var bf = new BlowFish(bf_key);
                var crypted = HexEncoding.HexToByte(passwd);
                
                HexEncoding.SwapEndianess(crypted);

                var plain = bf.Decrypt_ECB(crypted);

                HexEncoding.SwapEndianess(plain.Span);

                
                var eos = plain.Span.IndexOf((byte) 0);
                if (eos >= 0) 
                    passwd = Encoding.UTF8.GetString(plain.Slice(0, eos).Span);

                _output.WriteLine(plain.ToHex());
                Assert.Equal("axi", passwd);
            }
        }

        [Fact]
        public async Task CanReadRootPassword()
        {
            using (var reader = new OmmConfReader("omm_conf.txt"))
            {
                var user = await reader.GetValueAsync("UA", "user", "root", CancellationToken.None); 
                Assert.Equal("$1$$juPq1oleiGg7WHdZ5itlC/", user["password"]);
            }
        }
    }
}