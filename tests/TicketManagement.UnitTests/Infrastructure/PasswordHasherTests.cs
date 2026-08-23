using TicketManagement.Infrastructure.Services;

namespace TicketManagement.UnitTests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var hash = _sut.Hash("Sup3rSecret!");
        _sut.Verify("Sup3rSecret!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_ReturnsFalse_ForIncorrectPassword()
    {
        var hash = _sut.Hash("Sup3rSecret!");
        _sut.Verify("WrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_ProducesDifferentHashes_ForSamePassword_DueToRandomSalt()
    {
        var hash1 = _sut.Hash("Sup3rSecret!");
        var hash2 = _sut.Hash("Sup3rSecret!");
        hash1.Should().NotBe(hash2);
    }
}
