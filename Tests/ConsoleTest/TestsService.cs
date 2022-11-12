using Microsoft.Extensions.Logging;

namespace ConsoleTest
{
    public class TestsService
    {
        private readonly ILogger<TestsService> _Logger;

        public TestsService(ILogger<TestsService> Logger) => _Logger = Logger;

        public void Invoke()
        {
            for (var i = 0; i < 5; i++) _Logger.LogInformation(i, "Info {0}", i);
            for (var i = 0; i < 5; i++) _Logger.LogWarning(i, "Warning {0}", i);
            for (var i = 0; i < 5; i++) _Logger.LogError(i, "Error {0}", i);
        }
    }
}
