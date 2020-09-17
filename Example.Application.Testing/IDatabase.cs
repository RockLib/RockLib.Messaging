using System.Threading.Tasks;

namespace Example
{
    public interface IDatabase
    {
        Task CreateAsync(string payload);

        Task UpdateAsync(string payload);

        Task DeleteAsync(string payload);
    }
}