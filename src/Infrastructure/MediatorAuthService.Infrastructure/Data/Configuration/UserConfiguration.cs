using MediatorAuthService.Domain.Core.Extensions;
using MediatorAuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediatorAuthService.Infrastructure.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Surname)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Password)
            .IsRequired();

        builder.HasData(new User
        {
            Id = Guid.Parse("d0bfa391-a604-4049-a868-359091461e46"),
            Email = "admin@gmail.com",
            Password = HashingManager.HashValue("qwe123"),
            Name = "Admin",
            Surname = "Admin",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedUserId = Guid.Parse("d0bfa391-a604-4049-a868-359091461e46"),
            RefreshToken = HashingManager.HashValue(Guid.NewGuid().ToString())
        });
    }
}