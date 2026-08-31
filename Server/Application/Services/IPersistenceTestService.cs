using SocialMediaSimulator.Server.Domain.Entities;

namespace SocialMediaSimulator.Server.Application.Services;

public interface IPersistenceTestService
{
    Task<PersistenceTest> CreateAsync(string value);
    Task<PersistenceTest?> GetByIdAsync(int id);
    Task<List<PersistenceTest>> GetAllAsync();
}
