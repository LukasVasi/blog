window.Comments = (function () {
    const toggleEdit = (commentId, editing) => {
        const commentDisplay = document.getElementById(`comment-display-${commentId}-div`);
        const commentEditForm = document.getElementById(`comment-edit-${commentId}`)
        const commentEditTextarea = document.getElementById(`comment-edit-${commentId}-textarea`);

        if (editing) {
            commentEditTextarea.value = commentDisplay.textContent.trim();
            commentDisplay.classList.add('d-none');
            commentEditForm.classList.remove('d-none');
        } else {
            commentEditTextarea.value = commentDisplay.textContent.trim();
            commentEditForm.classList.add('d-none');
            commentDisplay.classList.remove('d-none');
        }
    }

    const toggleReport = (commentId, reporting) => {
        const commentReportForm = document.getElementById(`comment-report-${commentId}`)

        if (reporting) {
            commentReportForm.classList.remove('d-none');
        } else {
            commentReportForm.classList.add('d-none');
        }
    }

    const confirmDelete = (commentDeleteForm) => {
        const commentText = commentDeleteForm.dataset.commentText;
        return confirm(
            `Are you sure you want to delete this comment?\n"${commentText}"`
        );
    }

    return {
        toggleEdit,
        toggleReport,
        confirmDelete
    };
}) ();