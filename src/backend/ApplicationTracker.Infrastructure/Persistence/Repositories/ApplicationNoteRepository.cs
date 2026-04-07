using ApplicationTracker.Domain.Entities;
using ApplicationTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTracker.Infrastructure.Persistence.Repositories;

public class ApplicationNoteRepository : IApplicationNoteRepository
{
    private readonly AppDbContext _db;

    public ApplicationNoteRepository(AppDbContext db) => _db = db;

    public Task<ApplicationNote?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct) =>
        _db.ApplicationNotes
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);

    public async Task<IReadOnlyList<ApplicationNote>> GetByApplicationAsync(Guid jobApplicationId, Guid userId, CancellationToken ct)
    {
        var result = await _db.ApplicationNotes
            .Where(n => n.JobApplicationId == jobApplicationId && n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
        return result.AsReadOnly();
    }

    public async Task AddAsync(ApplicationNote note, CancellationToken ct) =>
        await _db.ApplicationNotes.AddAsync(note, ct);

    public void Update(ApplicationNote note) => _db.ApplicationNotes.Update(note);

    public void Delete(ApplicationNote note) => _db.ApplicationNotes.Remove(note);

    public Task SaveChangesAsync(CancellationToken ct) =>
        _db.SaveChangesAsync(ct);
}
