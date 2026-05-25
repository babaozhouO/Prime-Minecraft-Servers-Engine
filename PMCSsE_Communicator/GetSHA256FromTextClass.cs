namespace PMCSsE_Communicator
{
    internal static class GetSHA256FromTextClass
    {
        /// <exception cref="System.Text.EncoderFallbackException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static string GetSHA256FromText(string Text)
        {
            try
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(Text);
                byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
            catch
            {
                throw;
            }
        }
    }
}
