using Blog.Application.Validation;
using Blog.Domain.Validation.Article;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Articles
{
    public class CreateArticleViewModel
    {
        [Required(ErrorMessage = TitleSpecifications.REQUIRED_ERROR_MESSAGE)]
        [MaxLength(TitleSpecifications.MAX_LENGTH, ErrorMessage = TitleSpecifications.MAX_LENGTH_ERROR_MESSAGE)]
        public string Title { get; set; } = string.Empty;

        public ArticleImageUploadFormViewModel Image { get; set; } = new();

        [Required(ErrorMessage = TextSpecifications.REQUIRED_ERROR_MESSAGE)]
        public string Text { get; set; } = string.Empty;
    }
}
