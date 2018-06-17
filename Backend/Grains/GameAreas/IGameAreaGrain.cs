using System.Threading.Tasks;
using Orleans;

namespace Grains.GameAreas
{
    public interface IGameAreaGrain : IGrainWithGuidKey
    {
        Task Initialize();

        Task PatchArea(GameAreaPatchRequest patchRequest);
    }
}