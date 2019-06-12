using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clonogram.Repositories
{
    public interface IAmazonS3Repository
    {
        Task Upload(IFormFile file, string name);
    }
}
