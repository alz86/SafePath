using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SafePath.Services
{
    public interface IStorageProviderService
    {
        Task SaveContents(byte[] content, params string[] keys);

        Task<byte[]> ReadContents(params string[] keys);

        Task<bool> Exists(params string[] keys);

        Task<bool> Delete(params string[] keys);

        Stream OpenRead(params string[] keys);
        
        Stream OpenWrite(params string[] keys);
    }

    public class HardDiskStorageProviderService : IStorageProviderService
    {
        private readonly IBaseFolderProviderService baseFolderProvider;

        public HardDiskStorageProviderService(IBaseFolderProviderService baseFolderProvider)
        {
            this.baseFolderProvider = baseFolderProvider;
        }

        public async Task<bool> Delete(params string[] keys)
        {
            if (!(await Exists(keys))) return false;
            File.Delete(GetFullPath(keys));
            return true;
        }

        public Task<bool> Exists(params string[] keys) => Task.FromResult(File.Exists(GetFullPath(keys)));

        public async Task<byte[]> ReadContents(params string[] keys)
        {
            if (!(await Exists(keys)))
            {
                throw new FileNotFoundException("The specified file does not exist.", GetFullPath(keys));
            }

            return await File.ReadAllBytesAsync(GetFullPath(keys));
        }

        public async Task SaveContents(byte[] content, params string[] keys)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var fullPath = GetFullPath(keys);
            EnsureDirectoryExists(fullPath);
            await File.WriteAllBytesAsync(fullPath, content);
        }

        public Stream OpenRead(params string[] keys)
        {
            var fullPath = GetFullPath(keys);
            EnsureDirectoryExists(fullPath);
            return File.OpenRead(fullPath);
        }

        public Stream OpenWrite(params string[] keys)
        {
            var fullPath = GetFullPath(keys);
            EnsureDirectoryExists(fullPath);
            return File.OpenWrite(fullPath);
        }

        private string GetFullPath(params string[] keys) =>
            Path.Combine(keys.Prepend(baseFolderProvider.BaseFolder).ToArray());

        private static void EnsureDirectoryExists(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
        }
    }
}