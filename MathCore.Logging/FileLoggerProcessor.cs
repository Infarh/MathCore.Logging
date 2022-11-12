using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace MathCore.Logging
{
    public class FileLoggerProcessor : IDisposable
    {
        private const int __MaxQueuedMessages = 1024;

        private readonly BlockingCollection<string> _MessageQueue = new(__MaxQueuedMessages);
        private readonly Thread _OutputThread;

        private string _FilePath;
        private StreamWriter _Writer;
        public string FilePath
        {
            get => _FilePath;
            set
            {
                if (string.Equals(_FilePath, value, StringComparison.OrdinalIgnoreCase)) return;
                var old_writer = _Writer;

                if (new FileInfo(value) is { Directory: { Exists: false } log_dir })
                    log_dir.Create();

                _Writer = new StreamWriter(_FilePath = value, true) { AutoFlush = true };
                old_writer?.Dispose();
            }
        }



        public FileLoggerProcessor(string FilePath)
        {
            this.FilePath = FilePath;
            _OutputThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "Console logger queue processing thread"
            };
            _OutputThread.Start();
        }

        public virtual void EnqueueMessage(string message)
        {
            if (!_MessageQueue.IsAddingCompleted)
            {
                try
                {
                    _MessageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException) { }
            }

            try
            {
                _Writer.Write(message);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ProcessLogQueue()
        {
            try
            {
                foreach (var message in _MessageQueue.GetConsumingEnumerable())
                    _Writer.Write(message);
            }
            catch
            {
                try
                {
                    _MessageQueue.CompleteAdding();
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void Dispose()
        {
            _MessageQueue.CompleteAdding();

            try
            {
                _OutputThread.Join(1500);
            }
            catch (ThreadStateException) { }
            _Writer?.Dispose();
        }
    }
}
