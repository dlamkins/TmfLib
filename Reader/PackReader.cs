using System;
using System.IO;
using System.Threading.Tasks;
using NanoXml;

namespace TmfLib.Reader {
    public class PackReader {

        private readonly IPackCollection      _packCollection;
        private readonly IPackResourceManager _packResourceManager;

        public PackReaderSettings PackReaderSettings { get; set; }

        public PackReader(IPackCollection packCollection, IPackResourceManager packResourceManager, PackReaderSettings settings = null) {
            _packCollection      = packCollection;
            _packResourceManager = packResourceManager;

            this.PackReaderSettings = settings ?? PackReaderSettings.DefaultPackReaderSettings;
        }

        public async Task<bool> PopulatePackFromStream(Stream xmlPackStream) {
            using (var xmlReader = new StreamReader(xmlPackStream)) {
                return await PopulatePackFromString(await xmlReader.ReadToEndAsync());
            }
        }

        public async Task<bool> PopulatePackFromString(string xmlPackContents) {
            NanoXmlDocument packDocument = null;

            bool packLoaded = false;

            try {
                packDocument = await NanoXmlDocument.LoadFromXmlAsync(xmlPackContents, this.PackReaderSettings);
                packLoaded   = true;
            } catch (NanoXmlParsingException e) {
                //Logger.Warn(e, $"Failed to successfully parse TacO overlay file.");
            } catch (Exception e) {
                //Logger.Warn(e, "Could not load TacO overlay file due to an unexpected exception.");
            }

            if (packLoaded) {
                TryLoadCategories(packDocument);
                TryLoadPois(packDocument);
            }

            return packLoaded;
        }

        private void TryLoadCategories(NanoXmlDocument packDocument) {
            var categoryNodes = packDocument.RootNode.SelectNodes(PackConstImpl.XML_ELEMENT_MARKERCATEGORY);

            for (int i = 0; i < categoryNodes.Length; i++) {
                Builder.PathableCategoryBuilder.UnpackCategory(categoryNodes[i], _packCollection.Categories);
            }
        }

        private void TryLoadPois(NanoXmlDocument packDocument) {
            var poisNodes = packDocument.RootNode.SelectNodes(PackConstImpl.XML_ELEMENT_POIS);

            for (int j = 0; j < poisNodes.Length; j++) {
                var poisNode = poisNodes[j];

                for (int i = 0; i < poisNode.SubNodes.Count; i++) {
                    var nPathable = Builder.PathablePrototypeBuilder.UnpackPathable(poisNode.SubNodes[i], _packResourceManager, _packCollection.Categories);

                    if (nPathable != null) {
                        _packCollection.PointsOfInterest.Add(nPathable);
                    }
                }
            }
        }

    }
}
