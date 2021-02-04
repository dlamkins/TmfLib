using System.Threading.Tasks;

namespace TmfLib {
    public interface IPackResourceManager {

        bool ResourceExists(string resourcePath);

        Task<byte[]> LoadResourceAsync(string resourcePath);

        byte[] LoadResource(string resourcePath);

        void Unload();

    }
}
