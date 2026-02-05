using Cysharp.Threading.Tasks;
using ProjectRuntime.Gameplay;

public class DropInterfaces 
{
    public interface IDroppableTile
    {
        UniTaskVoid Drop(BathSlideTile bathSlideTile);

        void CancelDrop();
    }
}
