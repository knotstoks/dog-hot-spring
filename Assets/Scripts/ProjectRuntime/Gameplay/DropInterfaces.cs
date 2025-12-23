using ProjectRuntime.Gameplay;

public class DropInterfaces 
{
    public interface IDroppableTile
    {
        void Drop(BathSlideTile bathSlideTile);

        void CancelDrop();
    }
}
