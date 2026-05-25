namespace PMCSsE_Backend.Modules
{
    internal static class ComputeSHA256Class
    {
        internal static string? ComputeSHA256(string input)
        {
            string result;
            try
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(inputBytes);
                result= Convert.ToHexString(hashBytes).ToLower();
            }
            catch (Exception ex)
            {
                StaticTools.HandleLog("计算文本的SHA256值失败");
                StaticTools.HandleLog($"异常:{ex.Message}");
                StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                return null;
            }
            return result;
        }
    }
}
