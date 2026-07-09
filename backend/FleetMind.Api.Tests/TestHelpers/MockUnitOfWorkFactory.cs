using FleetMind.Api.Models.Common;
using FleetMind.Api.Repositories;
using Moq;

namespace FleetMind.Api.Tests.TestHelpers;

public static class MockUnitOfWorkFactory
{
    public static Mock<IUnitOfWork> CreateDefault()
    {
        var mockUow = new Mock<IUnitOfWork>();

        // Default permissive behavior
        mockUow.Setup(u => u.SaveChangesAsync())
               .ReturnsAsync(1);

        // Individual repositories could be set up similarly here
        mockUow.Setup(u => u.Voyages).Returns(new Mock<IVoyageRepository>().Object);

        return mockUow;
    }
}
