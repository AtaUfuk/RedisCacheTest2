namespace RedisCacheTest2.Lib.Cache
{
    public interface IRedisCacheMem
    {
        Task<T?> GetRecordAsync<T>(string recordId);
        Task SetRecordAsync<T>(string recordId, T data);
        Task SetorUpdateRecordAsync<T>(string recordId, T data);
        void ClearAll();
    }
}
