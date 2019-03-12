using System;
using System.IO;
using RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe;
using RfpProxyLib;
using Xunit;
using Xunit.Abstractions;

namespace RfpProxy.Test
{
    public class DeTeWeProprietaryTest
    {
        private readonly ITestOutputHelper _output;

        public DeTeWeProprietaryTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanDecodeDatetime()
        {
            var element = Decode<DateTimeDeTeWeElement>(DeTeWeType.DateTime, "c019031121500000");
            Log(element);
            Assert.Equal(new DateTime(2019, 03, 11, 21, 50, 00), element.DateTime);
        }

        [Fact]
        public void CanDecodeHomeScreenText()
        {
            var element = Decode<HomeScreenTextDeTeWeElement>(DeTeWeType.HomeScreenText, "900d506f43207a6976696c6c69616e0a4556454e5450484f4e450434353032");
            Log(element);
            Assert.Contains("PoC zivillian", element.Values);
            Assert.Contains("EVENTPHONE", element.Values);
        }

        [Fact]
        public void CanDecodeDisplay()
        {
            var element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "8110 04 4e616d6500");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "a4ff 0b 54656c65666f6e62756368 0a 5a656974204c696d6974");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "a0ff 00  0f 4e696368742076657266c3bc67622e");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "a0ff 00  0a 416267656d656c646574");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "b2ff 0c 53797374656d204d656ec3bc 41 506167696e670a764361726420656d7066616e67656e0a52c3bc636b52756620622e48616c74656e0a416e6b6c6f7066656e0a41646d696e697374726174696f6e");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "8940 0f 45696e6761626520642e5a69656c73 04 34353032");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "8940 0c 52c3bc636b727566204e722e 04 34353032");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "b2ff 06 506167696e67 10 74656c3a343530320a63623a34353032");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "b2ff 0a 41757377c3a4686c656e 08 6162636465762c20");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "b2ff 08 6162636465762c20 07 54203a33373130");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "a4ff 06 506167696e67 09 476573746172746574");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "b3ff 0a 41757377c3a4686c656e 58 506f6361686f6e6465726963682c200a506f43204265462c200a506f43204265726e69652c200a506f43204472346b332c200a506f43204641582c200a506f432047617277696e2c200a506f432048656c706465736b2c20");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "b8ff000d506f4320486f746c696e652c20");
            Log(element);
            element = Decode<DisplayDeTeWeElement>(DeTeWeType.Display, "b7ff0015506f43205365727669636520486f746c696e652c20");
            Log(element);
        }

        private static T Decode<T>(DeTeWeType type, string hex) where T:DeTeWeElement
        {
            var data = HexEncoding.HexToByte(hex.Replace(" ", String.Empty));
            var element = DeTeWeElement.Create(type, data);
            return Assert.IsType<T>(element);
        }

        private void Log(DeTeWeElement message)
        {
            using (var writer = new StringWriter())
            {
                message.Log(writer);
                _output.WriteLine(writer.ToString());
            }
        }
    }
}