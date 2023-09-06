using Ascon.Pilot.DataClasses;
using Ascon.Pilot.Server.Api.Contracts;

namespace PilotIfcBmService
{
    class RemoteFileStream : Stream
    {
        private readonly IFileArchiveApi _fileArchiveApi;
        private readonly INFile _file;

        public RemoteFileStream(IFileArchiveApi fileArchiveApi, INFile file)
        {
            _fileArchiveApi = fileArchiveApi;
            _file = file;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var chunk = _fileArchiveApi.GetFileChunk(_file.Id, Position, count);
            Array.Copy(chunk, buffer, chunk.Length);
            Position += chunk.Length;
            return chunk.Length;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _file.Size;
        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
