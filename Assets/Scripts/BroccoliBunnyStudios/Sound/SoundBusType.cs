namespace BroccoliBunnyStudios.Sound
{
    [System.Flags]
    public enum SoundBusType
    {
        None = 0,
        Music1 = 1,
        Music2 = 1 << 1,
        Generic = 1 << 2,
        Buttons = 1 << 3,
        UiFx = 1 << 4,
        Sfx = Buttons | UiFx,
        Bgm = Music1 | Music2,
    }
}