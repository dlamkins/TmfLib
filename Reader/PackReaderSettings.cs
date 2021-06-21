using System;
using System.Collections.Generic;

namespace TmfLib.Reader {
    public class PackReaderSettings {

        public IList<string> VenderPrefixes { get; } = new List<string>(0);

        public static readonly PackReaderSettings DefaultPackReaderSettings = new();

    }
}
