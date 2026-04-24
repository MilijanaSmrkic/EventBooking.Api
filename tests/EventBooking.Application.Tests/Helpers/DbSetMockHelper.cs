using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace EventBooking.Application.Tests.Helpers;

internal static class DbSetMockHelper
{
    internal static Mock<DbSet<T>> BuildMock<T>(IEnumerable<T> data) where T : class
        => data.AsQueryable().BuildMockDbSet();
}
