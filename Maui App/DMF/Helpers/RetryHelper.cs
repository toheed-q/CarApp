namespace DMF.Helpers
{
    public class RetryHelper
    {
        public static async Task<T> RetryAsync<T>(Func<Task<T>> action, int retries = 3)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    return await action();
                }
                catch
                {
                    if (i == retries - 1)
                        throw;

                    await Task.Delay(1000);
                }
            }

            throw new Exception("Retry failed");
        }
    }
}
