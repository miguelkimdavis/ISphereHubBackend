using System;
using System.Collections.Generic;
using System.Text;

namespace ISphereHub.Application.Interfaces
{
    public interface IImageUploadService
    {
        // Returns a relative URL (e.g. "/uploads/products/xxxx.jpg") that the
        // frontend combines with the API's origin to display the image.
        Task<string> SaveImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    }
}
