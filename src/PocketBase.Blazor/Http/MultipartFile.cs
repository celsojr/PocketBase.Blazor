using System;
using System.IO;

namespace PocketBase.Blazor.Http
{
    /// <summary>
    /// Represents a file to be sent as multipart/form-data.
    /// Mirrors the JS SDK File usage.
    /// </summary>
    public sealed class MultipartFile : IDisposable
    {
        /// <summary>
        /// The form field name.
        /// Defaults to <c>file</c> for PocketBase APIs.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The file name as sent to the server.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The file content stream.
        /// </summary>
        public Stream Content { get; }

        /// <summary>
        /// The content type of the file.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Creates a new <see cref="MultipartFile"/> instance.
        /// </summary>
        /// <param name="content">The file content stream.</param>
        /// <param name="fileName">The file name as sent to the server.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="name">The form field name.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public MultipartFile(Stream content, string fileName, string contentType = "application/octet-stream", string name = "file")
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            FileName = string.IsNullOrWhiteSpace(fileName)
                ? throw new ArgumentException("File name is required.", nameof(fileName))
                : fileName;

            ContentType = contentType;
            Name = string.IsNullOrWhiteSpace(name) ? "file" : name;
        }

        /// <summary>
        /// Creates a <see cref="MultipartFile"/> from a byte array.
        /// </summary>
        public static MultipartFile FromBytes(byte[] bytes, string fileName, string contentType = "application/octet-stream", string name = "file")
        {
            ArgumentNullException.ThrowIfNull(bytes);
            return new MultipartFile(new MemoryStream(bytes), fileName, contentType, name);
        }

        /// <summary>
        /// Creates a <see cref="MultipartFile"/> from a file on disk.
        /// </summary>
        public static MultipartFile FromFile(string path, string contentType = "application/octet-stream", string name = "file")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("File path is required.", nameof(path));
            }

            return new MultipartFile(File.OpenRead(path), Path.GetFileName(path), contentType, name);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Content?.Dispose();
        }
    }
}
