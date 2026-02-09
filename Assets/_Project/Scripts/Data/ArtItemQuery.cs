using System;
using System.Collections.Generic;
using System.Linq;

namespace BA.Data
{
    public static class ArtItemQuery
    {
        public static bool HasTag(this ArtItemSO item, string tag)
        {
            if (item == null || string.IsNullOrWhiteSpace(tag)) return false;
            var tags = item.Tags;
            if (tags == null) return false;

            for (int i = 0; i < tags.Count; i++)
            {
                if (string.Equals(tags[i], tag, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static bool MatchesAuthor(this ArtItemSO item, string author) =>
            item != null && !string.IsNullOrWhiteSpace(author) &&
            string.Equals(item.Author, author, StringComparison.OrdinalIgnoreCase);

        public static bool MatchesStyle(this ArtItemSO item, string style) =>
            item != null && !string.IsNullOrWhiteSpace(style) &&
            string.Equals(item.Style, style, StringComparison.OrdinalIgnoreCase);

        public static List<ArtItemSO> Where(this IReadOnlyList<ArtItemSO> items, Func<ArtItemSO, bool> predicate)
        {
            var result = new List<ArtItemSO>();
            if (items == null || predicate == null) return result;

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it != null && predicate(it)) result.Add(it);
            }
            return result;
        }
    }
}
