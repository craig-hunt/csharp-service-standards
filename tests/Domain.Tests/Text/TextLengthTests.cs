using ServiceStandards.Domain.Text;
using Xunit;

namespace ServiceStandards.Domain.Tests.Text;

/// <summary>
/// Rune counting, which decides where every length limit falls.
/// </summary>
/// <remarks>
/// The emoji case is the one that matters: it occupies two UTF-16 code units,
/// so a count over Length would report twice what a reader sees and reject text
/// that fits.
/// </remarks>
public sealed class TextLengthTests
{
    private const string Plain = "hello";
    private const int PlainRunes = 5;
    private const string Emoji = "ab🙂";
    private const int EmojiRunes = 3;
    private const int EmojiCodeUnits = 4;
    private const string Accented = "café";
    private const int AccentedRunes = 4;

    [Fact]
    public void CountRunesCountsPlainText() =>
        Assert.Equal(PlainRunes, TextLength.CountRunes(Plain));

    [Fact]
    public void CountRunesCountsAnEmojiOnce()
    {
        Assert.Equal(EmojiRunes, TextLength.CountRunes(Emoji));
        Assert.Equal(EmojiCodeUnits, Emoji.Length);
    }

    [Fact]
    public void CountRunesCountsAnAccentedLetterOnce() =>
        Assert.Equal(AccentedRunes, TextLength.CountRunes(Accented));

    [Fact]
    public void CountRunesReportsNothingForEmptyText() =>
        Assert.Equal(0, TextLength.CountRunes(string.Empty));
}
