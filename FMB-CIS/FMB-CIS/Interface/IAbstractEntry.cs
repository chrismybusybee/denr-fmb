namespace FMB_CIS.Interface
{
    public interface IAbstractEntry<TEntity> where TEntity : class
    {
        Task<List<TEntity>> GetRecords();
        //List<TEntity> GetRecords();
        Task<TEntity> GetRecordById(int id);
        Task InsertRecord(TEntity model, int userId);
        Task UpdateRecord(TEntity model, int userId);
        Task DeleteRecords(List<TEntity> model);
    }
}
