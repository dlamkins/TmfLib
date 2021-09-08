using System.Threading.Tasks;

namespace TmfLib {
    public interface IPackResource {

        Task<byte[]> GetDataAsync();

    }
}
