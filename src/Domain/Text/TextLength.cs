namespace ServiceStandards.Domain.Text;

/// <summary>
/// Counts the characters a length limit governs.
/// </summary>
/// <remarks>
/// A limit expressed in UTF-16 code units would reject a title an emoji makes
/// two units longer while accepting a longer one made of Latin letters, so the
/// count runs over runes. This matches the sibling Go project, which counts the
/// same way, keeping the boundary identical across both implementations.
/// </remarks>
public static class TextLength
{
    public static int CountRunes(string value)
    {
        var runes = value.EnumerateRunes();
        var count = 0;
        while (runes.MoveNext())
        {
            count++;
        }

        return count;
    }
}
