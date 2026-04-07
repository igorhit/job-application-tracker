using ApplicationTracker.Domain.Entities;

namespace ApplicationTracker.Domain.Interfaces;

public interface IApplicationNoteRepository
{
    Task<ApplicationNote?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationNote>> GetByApplicationAsync(Guid jobApplicationId, Guid userId, CancellationToken ct = default);
    Task AddAsync(ApplicationNote note, CancellationToken ct = default);
    void Update(ApplicationNote note);
    void Delete(ApplicationNote note);
    Task SaveChangesAsync(CancellationToken ct = default);
}
