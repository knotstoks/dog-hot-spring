using Cysharp.Threading.Tasks;

namespace BroccoliBunnyStudios.Panel
{
    public static class BasePanelExtensions
    {
        public static async UniTask WaitWhilePanelIsAlive(this BasePanel panel)
        {
            while (panel)
            {
                await UniTask.Yield();
            }
        }
    }
}