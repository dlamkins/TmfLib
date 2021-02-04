namespace TmfLib.Writer {
    public class PackWriterSettings {

        public enum OutputMethod {
            /// <summary>
            /// Exported as an archive (.zip, .taco, .tmf file).
            /// </summary>
            Archive,

            /// <summary>
            /// Exported to a directory.
            /// </summary>
            Directory
        }

        private const OutputMethod DEFAULT_PACKOUTPUTMETHOD = OutputMethod.Archive;
        private const bool         DEFAULT_SKIPMAPID        = false;
        private const bool         DEFAULT_INCLUDEMARKERS   = true;
        private const bool         DEFAULT_INCLUDETRAILS    = true;
        private const bool         DEFAULT_INCLUDEROUTES    = true;

        /// <summary>
        /// [Default: Archive]
        /// Determines the format that the pack should be exported as.
        /// </summary>
        public OutputMethod PackOutputMethod { get; set; } = DEFAULT_PACKOUTPUTMETHOD;

        /// <summary>
        /// [Default: <c>false</c>, Not TacO compatible if <c>true</c>]
        /// Indicates if the MapId should be skipped in optimized packs (as it is redundant).
        /// If enabled, packs will not be compatible with TacO.
        /// </summary>
        public bool SkipMapId { get; set; } = DEFAULT_SKIPMAPID;

        /// <summary>
        /// [Default: <c>true</c>]
        /// Indicates if markers should be included when writing the optimized pack.
        /// </summary>
        public bool IncludeMarkers { get; set; } = DEFAULT_INCLUDEMARKERS;

        /// <summary>
        /// [Default: <c>true</c>]
        /// Indicates if trails should be included when writing the optimized pack.
        /// </summary>
        public bool IncludeTrails { get; set; } = DEFAULT_INCLUDETRAILS;

        /// <summary>
        /// [Default: <c>true</c>]
        /// Indicates if routes should be included when writing the optimized pack.
        /// </summary>
        public bool IncludeRoutes { get; set; } = DEFAULT_INCLUDEROUTES;

        public static readonly PackWriterSettings DefaultPackWriterSettings = new PackWriterSettings();

    }
}
