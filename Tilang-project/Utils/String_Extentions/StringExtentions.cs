namespace Tilang_project.Utils.String_Extentions
{
    public static class StringExtentions
    {
        public static string GetStringContent(this string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2) return value;
            return value.Substring(1, value.Length - 2).Trim();
        }

        public static string Slice(this string value ,int start , int end)
        {
            var len = end - start;
            return value.Substring(start, len).Trim();
        }

        public static string Slice(this string value , int start)
        {
            return value.Substring (start).Trim();
        }
    }
}
