using Microsoft.EntityFrameworkCore;
using SocialMediaSimulator.Server.Domain.Entities;
using SocialMediaSimulator.Server.Infrastructure.Persistence;

namespace SocialMediaSimulator.Server.Application.Services;

public class PersistenceTestService : IPersistenceTestService
{
    private readonly AppDbContext _context;

    public PersistenceTestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PersistenceTest> CreateAsync(string value)
    {
        var entity = new PersistenceTest
        {
            Value = value,
            CreatedAt = DateTime.UtcNow
        };

        _context.PersistenceTests.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<PersistenceTest?> GetByIdAsync(int id)
    {
        return await _context.PersistenceTests.FindAsync(id);
    }

    public async Task<List<PersistenceTest>> GetAllAsync()
    {
        return await _context.PersistenceTests.ToListAsync();
    }
}
