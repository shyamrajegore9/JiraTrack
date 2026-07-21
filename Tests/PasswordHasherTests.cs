using JiraTrack.BusinessLogic;
using JiraTrack.Models.Enums;

namespace JiraTrack.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ProducesDistinctHashesForSamePassword()
    {
        var hash1 = PasswordHasher.HashPassword("Test@12345");
        var hash2 = PasswordHasher.HashPassword("Test@12345");

        Assert.NotEqual(hash1, hash2);
        Assert.True(PasswordHasher.VerifyPassword("Test@12345", hash1));
        Assert.True(PasswordHasher.VerifyPassword("Test@12345", hash2));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var hash = PasswordHasher.HashPassword("CorrectPassword1!");

        Assert.False(PasswordHasher.VerifyPassword("WrongPassword1!", hash));
    }

    [Fact]
    public void HashToken_IsDeterministic()
    {
        var first = PasswordHasher.HashToken("sample-refresh-token");
        var second = PasswordHasher.HashToken("sample-refresh-token");

        Assert.Equal(first, second);
        Assert.NotEqual(PasswordHasher.HashToken("other-token"), first);
    }
}

public class FileValidationHelperTests
{
    [Theory]
    [InlineData("photo.jpg", FileType.Image)]
    [InlineData("doc.PDF", FileType.Document)]
    [InlineData("clip.webm", FileType.Video)]
    public void TryGetFileType_AcceptsAllowedExtensions(string fileName, FileType expected)
    {
        var result = FileValidationHelper.TryGetFileType(fileName, out var fileType);

        Assert.True(result);
        Assert.Equal(expected, fileType);
    }

    [Theory]
    [InlineData("script.exe")]
    [InlineData("archive.zip")]
    public void TryGetFileType_RejectsDisallowedExtensions(string fileName)
    {
        var result = FileValidationHelper.TryGetFileType(fileName, out _);

        Assert.False(result);
    }

    [Fact]
    public void SanitizeFileName_RemovesInvalidCharacters()
    {
        var sanitized = FileValidationHelper.SanitizeFileName(@"..\evil\report.pdf");

        Assert.Equal("report.pdf", sanitized);
    }

    [Fact]
    public async Task ValidateMagicBytes_AcceptsPdfHeader()
    {
        await using var stream = new MemoryStream("%PDF-1.7 sample"u8.ToArray());

        await FileValidationHelper.ValidateMagicBytesAsync(FileType.Document, stream, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateMagicBytes_RejectsMismatch()
    {
        await using var stream = new MemoryStream("not-a-real-image"u8.ToArray());

        await Assert.ThrowsAsync<BusinessException>(() =>
            FileValidationHelper.ValidateMagicBytesAsync(FileType.Image, stream, CancellationToken.None));
    }
}
