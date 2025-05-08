namespace TaskManagement_BE.utils
{
    public static class GuidUtil
    {
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
