namespace Core.SaveSystem
{
    public interface ISaveData
    {
        string GetFileName();
        void OnBeforeSave();
        void OnAfterLoad();
    }
}
