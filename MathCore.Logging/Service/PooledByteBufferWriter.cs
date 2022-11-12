using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MathCore.Logging
{
    internal sealed class PooledByteBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private byte[] _RentedBuffer;
        private int _Index;

        public PooledByteBufferWriter(int InitialCapacity)
        {
            _RentedBuffer = ArrayPool<byte>.Shared.Rent(InitialCapacity);
            _Index = 0;
        }

        public ReadOnlyMemory<byte> WrittenMemory => _RentedBuffer.AsMemory(0, _Index);

        public int WrittenCount => _Index;

        public int Capacity => _RentedBuffer.Length;

        public int FreeCapacity => _RentedBuffer.Length - _Index;

        public void Clear() => ClearHelper();

        private void ClearHelper()
        {
            _RentedBuffer.AsSpan(0, _Index).Clear();
            _Index = 0;
        }

        public void Dispose()
        {
            if (_RentedBuffer == null)
                return;
            ClearHelper();
            ArrayPool<byte>.Shared.Return(_RentedBuffer);
            _RentedBuffer = null;
        }

        public void Advance(int Count) => _Index += Count;

        public Memory<byte> GetMemory(int SizeHint = 0)
        {
            CheckAndResizeBuffer(SizeHint);
            return _RentedBuffer.AsMemory(_Index);
        }

        public Span<byte> GetSpan(int SizeHint = 0)
        {
            CheckAndResizeBuffer(SizeHint);
            return _RentedBuffer.AsSpan(_Index);
        }

        internal Task WriteToStreamAsync(Stream Destination, CancellationToken Cancel) => 
            Destination.WriteAsync(_RentedBuffer, 0, _Index, Cancel);

        private void CheckAndResizeBuffer(int SizeHint)
        {
            if (SizeHint == 0)
                SizeHint = 256;
            if (SizeHint <= _RentedBuffer.Length - _Index)
                return;
            var length = _RentedBuffer.Length;
            var num2 = Math.Max(SizeHint, length);
            var minimum_length = length + num2;
            if ((uint)minimum_length > int.MaxValue)
            {
                minimum_length = length + SizeHint;
                if ((uint)minimum_length > int.MaxValue)
                    throw new OutOfMemoryException($"Переполнение буфера {(uint)minimum_length}");
            }
            var rented_buffer = _RentedBuffer;
            _RentedBuffer = ArrayPool<byte>.Shared.Rent(minimum_length);
            var span = rented_buffer.AsSpan(0, _Index);
            span.CopyTo(_RentedBuffer);
            span.Clear();
            ArrayPool<byte>.Shared.Return(rented_buffer);
        }
    }
}
