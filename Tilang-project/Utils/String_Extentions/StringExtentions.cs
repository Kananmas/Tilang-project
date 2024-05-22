namespace Tilang_project.Utils.String_Extentions
{
    public static class StringExtentions
    {
        public static string GetStringContent(this string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2) return value;
            return value.Substring(1, value.Length - 2).Trim();
        }
    }
}
