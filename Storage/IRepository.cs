namespace RestaurantManager.Storage;

public interface IRepository
{
    AppData Load();
    void Save(AppData data);
}