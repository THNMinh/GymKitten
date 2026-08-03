using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Refreshtoken> Refreshtokens { get; }

    DbSet<Product> Products { get; }

    DbSet<Productimage> Productimages { get; }
}
