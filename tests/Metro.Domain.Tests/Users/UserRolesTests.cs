using Metro.Domain.Users;
using Shouldly;

namespace Metro.Domain.Tests.Users;

public class UserRolesTests
{
    [Theory]
    [Trait("Requirement", "FR-ACC-02")]
    [Trait("Requirement", "NFR-SEC-02")]
    [InlineData(UserRoles.Passenger)]
    [InlineData(UserRoles.Staff)]
    public void IsValid_WhenCanonicalRole_ReturnsTrue(string role)
    {
        UserRoles.IsValid(role).ShouldBeTrue();
    }

    [Theory]
    [Trait("Requirement", "FR-ACC-02")]
    [Trait("Requirement", "NFR-SEC-02")]
    [InlineData("master")]
    [InlineData("client")]
    [InlineData("admin")]
    [InlineData("user")]
    [InlineData("PASSENGER")]
    [InlineData("STAFF")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_WhenLegacyOrUnknownRole_ReturnsFalse(string? role)
    {
        UserRoles.IsValid(role).ShouldBeFalse();
    }
}
