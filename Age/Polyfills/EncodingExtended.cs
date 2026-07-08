using System.Text;

namespace Age.Polyfills
{
    static class EncodingExtended
    {
        public static readonly UTF8Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
}
