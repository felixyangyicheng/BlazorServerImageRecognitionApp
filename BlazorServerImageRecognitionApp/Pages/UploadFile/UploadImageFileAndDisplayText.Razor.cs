﻿using ImagePrintedTextRecognitionShared;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace BlazorServerImageRecognitionApp.Pages.UploadFile
{
    public partial class UploadImageFileAndDisplayText
    {
        private ImageRecognitionOutput imageRecognitionOutput = new ImageRecognitionOutput();
        private bool anyFileUploaded = false;
        private bool currentFileUploaded = false;

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs e)
        {
            imageRecognitionOutput = new ImageRecognitionOutput();
            anyFileUploaded = true;
            currentFileUploaded = false;

            long maxFileSize = 1024 * 1024 * 15;

            if (e.File.Size > maxFileSize)
            {
                imageRecognitionOutput = new ImageRecognitionOutput()
                {
                    ErrorMessage = $"The file size is {e.File.Size} bytes, this is more than the allowed limit of {maxFileSize} bytes.",
                };

                currentFileUploaded = true;

                return;
            }
            else if (e.File == null || !e.File.ContentType.Contains("image"))
            {
                imageRecognitionOutput = new ImageRecognitionOutput()
                {
                    ErrorMessage = "Please uplaod a valid image file",
                };

                currentFileUploaded = true;

                return;
            }

            try
            {
                var fileContent = new StreamContent(e.File.OpenReadStream(maxFileSize));

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(e.File.ContentType);

                var uploadIamgeFileStream = await fileContent.ReadAsStreamAsync();

                var imageRecognitionInput = new ImageRecognitionInput()
                {
                    SubscriptionKey = configuration["SubscriptionKey"].ToString(),
                    AzureEndpointURL = configuration["AzureEndpointURL"].ToString(),
                    UploadImageFileStream = uploadIamgeFileStream,
                };

                imageRecognitionOutput = await imagePrintedTextRecognitionService.UploadFileAndConvertToText(imageRecognitionInput);

                var uploadFile = e.File;

                var imageFile = await uploadFile.RequestImageFileAsync("image/jpeg", 700, 500);
                using var fileStream = imageFile.OpenReadStream(maxFileSize);

                var uploadImageFileStream = await fileContent.ReadAsStreamAsync();

                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);

                imageRecognitionOutput.ImageUrl = string.Concat("data:image/png;base64,", Convert.ToBase64String(memoryStream.ToArray()));

                currentFileUploaded = true;
            }
            catch (Exception ex)
            {
                imageRecognitionOutput = new ImageRecognitionOutput()
                {
                    ErrorMessage = ex.Message,
                };

                currentFileUploaded = true;
            }
        }
    }
}
