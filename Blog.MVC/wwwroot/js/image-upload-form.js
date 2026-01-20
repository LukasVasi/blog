// This script works together with the ArticleImageUploadForm partial.

const init = () => {
    const imageUpload = $('#imageUpload');
    const imageIdField = $('#imageIdField');
    const originalImageUrlField = $('#originalImageUrlField');
    const imagePreview = $('#imagePreview');
    const imagePreviewContainer = $('#imagePreviewContainer');
    const uploadStatus = $('#uploadStatus');
    const removeImageBtn = $('#removeImageBtn');
    const submitBtn = $('#submitBtn');

    const uploadUrl = imageUpload.data('uploadUrl');
    const originalImageId = imageIdField.val();
    const originalImageUrl = originalImageUrlField.val();

    const showSuccess = () => {
        if (originalImageId !== null && originalImageId !== undefined) {
            uploadStatus.html('<div class="alert alert-success alert-dismissible fade show" role="alert">' +
                'New image uploaded successfully! Save to apply changes.' +
                '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                '</div>');
        }
        else {
            uploadStatus.html('<div class="alert alert-success alert-dismissible fade show" role="alert">' +
                'Image uploaded successfully!' +
                '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                '</div>');
        }
        
    }

    const showError = (message) => {
        uploadStatus.html('<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            message +
            '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
            '</div>');
    }

    const uploadImage = async (file) => {
        submitBtn.prop('disabled', true);

        const formData = new FormData();
        formData.append('articleImage', file);

        uploadStatus.html('<div class="spinner-border spinner-border-sm me-2" role="status"></div>Uploading...');

        try {
            const response = await fetch(uploadUrl, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(error || 'An unexpected error has occured.');
            }

            const result = await response.json();

            imageIdField.val(result.id);

            imagePreview.attr('src', result.url);
            imagePreviewContainer.show();

            showSuccess();
        }
        catch (error) {
            console.error('Upload error:', error);
            showError('Failed to upload image: ' + error.message);
            imageUpload.val('');
        }
        finally {
            submitBtn.prop('disabled', false);
        }
    }

    const onImageUploadChanged = async (event) => {
        const file = event.target.files[0];

        if (!file) {
            return;
        }

        if (file.size > 5 * 1024 * 1024) {
            showError('File size must be less than 5MB.');
            imageUpload.val('');
            return;
        }

        const fileName = file.name;
        const extension = fileName.split('.').pop().toLowerCase();
        const allowedExtensions = ["jpg", "jpeg", "png", "gif"];

        if (!allowedExtensions.includes(extension) || !file.type.startsWith('image/')) {
            showError('The file must be a valid image file. Supported formats: JPG, PNG, GIF.');
            imageUpload.val('');
            return;
        }

        await uploadImage(file);
    }

    imageUpload.on('change', onImageUploadChanged);

    const onResetOriginalImageClicked = () => {
        imageIdField.val(originalImageId);

        imagePreview.attr('src', originalImageUrl);
        imagePreviewContainer.show();

        uploadStatus.empty();
        console.log("Alio? " + originalImageUrl + originalImageId);
    }

    uploadStatus.on('click', '[data-action="reset-original-image"]', onResetOriginalImageClicked);

    const onRemoveButtonClicked = () => {
        imageIdField.val('');
        imagePreview.attr('src', '');
        imagePreviewContainer.hide();
        imageUpload.val('');
        
        if (originalImageId !== null && originalImageId !== undefined) {
            uploadStatus.html('<div class="alert alert-info">Image will be removed when you save.' +
                '<button type="button" class="btn btn-link" data-action="reset-original-image">Reset original image</button>' +
                '</div> ');
        }
        else {
            uploadStatus.html('<div class="alert alert-info">Image successfully removed.' +
                '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                '</div> ');
        }
    }

    removeImageBtn.on('click', onRemoveButtonClicked);
}

$(document).ready(init);