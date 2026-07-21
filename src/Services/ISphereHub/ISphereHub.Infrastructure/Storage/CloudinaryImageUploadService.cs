using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Infrastructure.Storage
{
    public class CloudinaryImageUploadService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryImageUploadService> _logger;

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };
        private const long MaxFileSizeBytes = 8 * 1024 * 1024; // 8 MB

        public CloudinaryImageUploadService(Cloudinary cloudinary, ILogger<CloudinaryImageUploadService> logger)
        {
            _cloudinary = cloudinary;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
        {
            if (!AllowedContentTypes.Contains(contentType))
                throw new ValidationAppException($"Unsupported image type '{contentType}'. Allowed: JPEG, PNG, WEBP, GIF.");

            if (fileStream.Length > MaxFileSizeBytes)
                throw new ValidationAppException("Image is too large. Maximum size is 8 MB.");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "isphere-hub/products",
                PublicId = Guid.NewGuid().ToString("N"),
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            if (result.Error != null)
            {
                _logger.LogWarning("Cloudinary upload failed for {FileName}: {Error}", fileName, result.Error.Message);
                throw new ValidationAppException($"Image upload failed: {result.Error.Message}");
            }

            _logger.LogInformation("Image uploaded to Cloudinary: {PublicId} -> {Url}", result.PublicId, result.SecureUrl);
            return result.SecureUrl.ToString();
        }
    }
}
