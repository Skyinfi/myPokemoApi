using Xunit;
using MyPokemoApi.Services;

namespace MyPokemoApi.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashForSamePassword()
    {
        // Arrange
        var password = "TestPassword123";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(password, hash1);
        Assert.NotEqual(password, hash2);
    }

    [Fact]
    public void HashPassword_ShouldThrowException_WhenPasswordIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordService.HashPassword(null!));
    }

    [Fact]
    public void HashPassword_ShouldThrowException_WhenPasswordIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordService.HashPassword(string.Empty));
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
    {
        // Arrange
        var password = "TestPassword123";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var password = "TestPassword123";
        var wrongPassword = "WrongPassword123";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsNull()
    {
        // Arrange
        var hash = _passwordService.HashPassword("TestPassword123");

        // Act
        var result = _passwordService.VerifyPassword(null!, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsNull()
    {
        // Act
        var result = _passwordService.VerifyPassword("TestPassword123", null!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Password123")]
    [InlineData("AnotherPassword456")]
    [InlineData("ComplexP@ssw0rd!")]
    public void HashPassword_ShouldBeIrreversible(string password)
    {
        // **Validates: Requirements 3.1**
        // Property: All passwords must be hashed and cannot be reversed
        
        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        Assert.NotEqual(password, hash);
        Assert.True(hash.Length > password.Length);
        Assert.True(_passwordService.VerifyPassword(password, hash));
    }

    [Theory]
    [InlineData("SamePassword123")]
    [InlineData("AnotherSame456")]
    public void HashPassword_ShouldProduceDifferentHashesForSameInput(string password)
    {
        // **Validates: Requirements 3.1**
        // Property: Same password should produce different hashes due to salt
        
        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);
        var hash3 = _passwordService.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash2, hash3);
        Assert.NotEqual(hash1, hash3);
        
        // But all should verify correctly
        Assert.True(_passwordService.VerifyPassword(password, hash1));
        Assert.True(_passwordService.VerifyPassword(password, hash2));
        Assert.True(_passwordService.VerifyPassword(password, hash3));
    }

    [Theory]
    [InlineData("CorrectPassword123", "WrongPassword123")]
    [InlineData("TestPass456", "TestPass457")]
    [InlineData("MyPassword", "mypassword")]
    public void VerifyPassword_ShouldBeAccurate(string correctPassword, string wrongPassword)
    {
        // **Validates: Requirements 3.2**
        // Property: Password verification must be accurate
        
        // Arrange
        var hash = _passwordService.HashPassword(correctPassword);

        // Act & Assert
        Assert.True(_passwordService.VerifyPassword(correctPassword, hash));
        Assert.False(_passwordService.VerifyPassword(wrongPassword, hash));
    }
}