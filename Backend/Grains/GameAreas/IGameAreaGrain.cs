using System.Threading.Tasks;
using Orleans;

namespace GameAreas
{
    public interface IGameAreaGrain : IGrainWithGuidKey
    {
        Task Initialize();
    }
}