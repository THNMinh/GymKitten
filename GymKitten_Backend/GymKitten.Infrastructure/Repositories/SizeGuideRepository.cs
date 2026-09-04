using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class SizeGuideRepository : ISizeGuideRepository
{
    private readonly GymkittenContext _context;

    public SizeGuideRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<List<Sizeguide>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Sizeguides
            .AsNoTracking()
            .Where(sg => sg.Productid == productId && sg.Deletedat == null)
            .OrderBy(sg => sg.Size)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sizeguide?> GetByIdAsync(Guid guideId, CancellationToken cancellationToken = default)
    {
        return await _context.Sizeguides
            .FirstOrDefaultAsync(sg => sg.Guideid == guideId && sg.Deletedat == null, cancellationToken);
    }

    public async Task AddAsync(Sizeguide sizeGuide, CancellationToken cancellationToken = default)
    {
        await _context.Sizeguides.AddAsync(sizeGuide, cancellationToken);
    }

    public void Update(Sizeguide sizeGuide)
    {
        _context.Sizeguides.Update(sizeGuide);
    }
}
