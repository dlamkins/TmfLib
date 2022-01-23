using System.IO;
using System.Threading.Tasks;

namespace TmfLib {

    public interface IPackResourceManager {

        bool ResourceExists(string resourcePath);

        Task<byte[]> LoadResourceAsync(string resourcePath);

        Task<Stream> LoadResourceStreamAsync(string resourcePath);

    }

}
