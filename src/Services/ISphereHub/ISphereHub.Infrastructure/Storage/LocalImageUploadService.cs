using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Exceptions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Storage
{
    public class LocalImageUploadService : IImageUploadService
    {
        private readonly IHostEnvironment _env;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };

        private const long MaxFileSizeBytes = 8 * 1024 * 1024; // 8 MB

        public LocalImageUploadService(IHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
        {
            if (!AllowedContentTypes.Contains(contentType))
                throw new ValidationAppException($"Unsupported image type '{contentType}'. Allowed: JPEG, PNG, WEBP, GIF.");

            if (fileStream.Length > MaxFileSizeBytes)
                throw new ValidationAppException("Image is too large. Maximum size is 8 MB.");

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = contentType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    _ => ".bin"
                };
            }

            var uploadsFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            // Never trust the original file name - generate our own to avoid
            // path traversal / collisions / executable-extension tricks.
            var safeFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsFolder, safeFileName);

            await using (var output = File.Create(fullPath))
            {
                await fileStream.CopyToAsync(output, ct);
            }

            return $"/uploads/products/{safeFileName}";
        }
    }
}
