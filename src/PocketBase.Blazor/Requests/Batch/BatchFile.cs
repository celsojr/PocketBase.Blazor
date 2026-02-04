using System.IO;

namespace PocketBase.Blazor.Requests.Batch
{
    /// <summary>
    /// Represents a file to be uploaded in a batch request.
    /// </summary>
    public sealed class BatchFile
    {
        /// <summary>
        /// The field name in the record where the file will be attached.
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// File name sent in multipart.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Optional content type.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Stream of file content.
        /// </summary>
        public Stream Content { get; }

        private BatchFile(string field, Stream content, string fileName, string contentType)
        {
            Field = field;
            Content = content;
            FileName = fileName;
            ContentType = contentType;
        }

        /// <summary>
        /// Create a file from byte array.
        /// </summary>
        public static BatchFile FromBytes(byte[] bytes, string field, string fileName, string contentType)
        {
            return new BatchFile(field, new MemoryStream(bytes), fileName, contentType);
        }

        /// <summary>
        /// Create a file from an existing stream.
        /// </summary>
        public static BatchFile FromStream(Stream stream, string field, string fileName, string contentType)
        {
            return new BatchFile(field, stream, fileName, contentType);
        }
    }
}
