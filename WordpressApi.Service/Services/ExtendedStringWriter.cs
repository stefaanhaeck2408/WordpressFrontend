using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordpressApi.Service
{
    //https://blog.jsinh.in/use-utf-8-encoding-for-stringwriter-in-c/#.XqRHOsCxVhE
    public sealed class ExtendedStringWriter: StringWriter
    {
        private readonly Encoding stringWriterEncoding;
        public ExtendedStringWriter(StringBuilder builder, Encoding desiredEncoding)
            : base(builder)
        {
            this.stringWriterEncoding = desiredEncoding;
        }

        public override Encoding Encoding
        {
            get
            {
                return this.stringWriterEncoding;
            }
        }
    }
}
