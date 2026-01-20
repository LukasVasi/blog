using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Blog.Domain.Enums;
using Blog.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Blog.Infrastructure.Persistence.Seeding
{
    public static class BlogDbContextSeeder
    {
        public static async Task SeedDataAsync(BlogDbContext dbContext)
        {
            await SeedUserRolesAsync(dbContext);
            await SeedUsersAsync(dbContext);
            await SeedArticlesAsync(dbContext);
            await SeedArticleImagesAsync(dbContext);
            await SeedArticleRatingsAsync(dbContext);
            await SeedArticleCommentsAsync(dbContext);
        }

        /// <summary>
        /// Seeds the critic, editor and admin user roles.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        private static async Task SeedUserRolesAsync(BlogDbContext dbContext)
        {
            var allUserRoles = Enum.GetValues<UserRoleEnum>();
            var addedCount = 0;

            foreach (var userRole in allUserRoles)
            {
                if (!await dbContext.UserRoles.AnyAsync(role => role.Id == userRole))
                {
                    await dbContext.UserRoles.AddAsync(new UserRole { Id = userRole });
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds a few users: one commentator, one critic who is also a commentator, 
        /// one editor, one editor that is also a critic and an admin.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        private static async Task SeedUsersAsync(BlogDbContext dbContext)
        {
            var passwordHasher = new PasswordHasher<object>();
            var addedCount = 0;
            var userRoles = await dbContext.UserRoles.ToDictionaryAsync(userRole => userRole.Id, userRole => userRole);

            if (!await dbContext.Users.AnyAsync(user => user.Id == SeedData.AdminUserId))
            {
                var adminUser = new User
                {
                    Id = SeedData.AdminUserId,
                    Username = "BlogAdmin",
                    NormalizedUsername = "blogadmin",
                    EmailAddress = "admin@blog.com",
                    NormalizedEmailAddress = "admin@blog.com",
                    PasswordHash = passwordHasher.HashPassword(null, "!AdminPassword123")
                };
                adminUser.Roles.Add(userRoles[UserRoleEnum.Admin]);
                dbContext.Users.Add(adminUser);
                addedCount++;
            }

            if (!await dbContext.Users.AnyAsync(user => user.Id == SeedData.CommentatorUserId))
            {
                var commentatorUser = new User
                {
                    Id = SeedData.CommentatorUserId,
                    Username = "SteveCommentator",
                    NormalizedUsername = "stevecommentator",
                    EmailAddress = "steveharr@blog.com",
                    NormalizedEmailAddress = "steveharr@blog.com",
                    PasswordHash = passwordHasher.HashPassword(null, "#Password123")
                };
                commentatorUser.Roles.Add(userRoles[UserRoleEnum.Commentator]);
                dbContext.Users.Add(commentatorUser);
                addedCount++;
            }

            if (!await dbContext.Users.AnyAsync(user => user.Id == SeedData.CriticUserId))
            {
                var criticUser = new User
                {
                    Id = SeedData.CriticUserId,
                    Username = "NancyW",
                    NormalizedUsername = "nancyw",
                    EmailAddress = "nwheeler@mail.com",
                    NormalizedEmailAddress = "nwheeler@mail.com",
                    PasswordHash = passwordHasher.HashPassword(null, "#Password123")
                };
                criticUser.Roles.Add(userRoles[UserRoleEnum.Critic]);
                criticUser.Roles.Add(userRoles[UserRoleEnum.Commentator]);
                dbContext.Users.Add(criticUser);
                addedCount++;
            }

            if (!await dbContext.Users.AnyAsync(user => user.Id == SeedData.EditorUserId))
            {
                var editor = new User
                {
                    Id = SeedData.EditorUserId,
                    Username = "JohnAuthoriston1992",
                    NormalizedUsername = "johnauthoriston1992",
                    EmailAddress = "johnauthoriston@blog.com",
                    NormalizedEmailAddress = "johnauthoriston@blog.com",
                    PasswordHash = passwordHasher.HashPassword(null, "#Password123")
                };
                editor.Roles.Add(userRoles[UserRoleEnum.Editor]);
                dbContext.Users.Add(editor);
                addedCount++;
            }

            if (!await dbContext.Users.AnyAsync(user => user.Id == SeedData.CriticEditorUserId))
            {
                var criticEditor = new User
                {
                    Id = SeedData.CriticEditorUserId,
                    Username = "TobiasNYC",
                    NormalizedUsername = "tobiasnyc",
                    EmailAddress = "tobias@blog.com",
                    NormalizedEmailAddress = "tobias@blog.com",
                    PasswordHash = passwordHasher.HashPassword(null, "#Password123")
                };
                criticEditor.Roles.Add(userRoles[UserRoleEnum.Editor]);
                criticEditor.Roles.Add(userRoles[UserRoleEnum.Critic]);
                dbContext.Users.Add(criticEditor);
                addedCount++;
            }

            if (addedCount > 0)
            {
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds a few articles that belong to the previously seeded editors
        /// (relationships established via <see cref="FirstEditorUserId"/> and <see cref="SecondEditorUserId"/>).
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        private static async Task SeedArticlesAsync(BlogDbContext dbContext)
        {
            var seedArticles = new[]
            {
                new Article
                {
                    Id = SeedData.Article1Id,
                    Title = "Welcome to the Blog",
                    Text = "A quick introduction to what we'll be sharing here and how you can follow along.",
                    CreatedAt = new DateTime(2025, 12, 1, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.EditorUserId
                },
                new Article
                {
                    Id = SeedData.Article2Id,
                    Title = "Getting Started with ASP.NET Core",
                    Text = "A beginner-friendly overview of projects, middleware, and useful tooling.",
                    CreatedAt = new DateTime(2025, 12, 7, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.EditorUserId
                },
                new Article
                {
                    Id = SeedData.Article3Id,
                    Title = "Bootstrap Layout Tips",
                    Text = "Practical advice for grids, spacing, and responsive images in Bootstrap.",
                    CreatedAt = new DateTime(2025, 12, 12, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.EditorUserId
                },
                new Article
                {
                    Id = SeedData.Article4Id,
                    Title = "Entity Framework Core Basics",
                    Text = "Understanding DbContext, migrations, and querying data efficiently.",
                    CreatedAt = new DateTime(2025, 12, 12, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.EditorUserId
                },
                new Article
                {
                    Id = SeedData.Article5Id,
                    Title = "Improving Page Performance",
                    Text = "Simple optimizations for faster load times: caching, bundling, and images.",
                    CreatedAt = new DateTime(2025, 12, 17, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.EditorUserId
                },
                new Article
                {
                    Id = SeedData.Article6Id,
                    Title = "Routing in Modern Web Apps",
                    Text = "How routing works in ASP.NET Core and client-side frameworks like React.",
                    CreatedAt = new DateTime(2025, 12, 15, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.EditorUserId
                },
                new Article
                {
                    Id = SeedData.Article7Id,
                    Title = "Handling Configuration Safely",
                    Text = "Best practices for secrets, environments, and appsettings.json.",
                    CreatedAt = new DateTime(2025, 12, 19, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.EditorUserId
                },
                new Article
                {
                    Id = SeedData.Article8Id,
                    Title = "Logging That Helps, Not Hurts",
                    Text = "Structured logging, levels, and when to log to track real issues.",
                    CreatedAt = new DateTime(2025, 12, 20, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.CriticEditorUserId
                },
                new Article
                {
                    Id = SeedData.Article9Id,
                    Title = "Deploying to the Cloud",
                    Text = "From local builds to production: CI/CD, environments, and monitoring.",
                    CreatedAt = new DateTime(2025, 12, 21, 14, 43, 12, DateTimeKind.Utc),
                    AuthorId = SeedData.CriticEditorUserId
                },
                new Article
                {
                    Id = SeedData.Article10Id,
                    Title = "Testing Your Application",
                    Text = "Unit, integration, and UI testing strategies to keep your app stable.",
                    CreatedAt = new DateTime(2025, 12, 23, 16, 56, 1, DateTimeKind.Utc),
                    AuthorId = SeedData.CriticEditorUserId
                }
            };

            var existingIds = await dbContext.Articles
                .Where(article => seedArticles.Select(seededArticle => seededArticle.Id).Contains(article.Id))
                .Select(article => article.Id)
                .ToListAsync();

            var newArticles = seedArticles.Where(article => !existingIds.Contains(article.Id)).ToList();

            if (newArticles.Any())
            {
                dbContext.Articles.AddRange(newArticles);
                await dbContext.SaveChangesAsync();
            }
        }

        private static async Task SeedArticleImagesAsync(BlogDbContext dbContext)
        {
            var seedImages = new[]
            {
            new ArticleImage
            {
                Id = SeedData.ArticleImage1Id,
                FileName = "images/seed-welcome.png",
                OriginalFileName = "welcome-banner.png",
                CreatedAt = new DateTime(2025, 12, 1, 14, 0, 0, DateTimeKind.Utc),
                UserId = SeedData.EditorUserId,
                ArticleId = SeedData.Article1Id
            },
            new ArticleImage
            {
                Id = SeedData.ArticleImage2Id,
                FileName = "images/seed-aspnet.png",
                OriginalFileName = "dotnet-core.png",
                CreatedAt = new DateTime(2025, 12, 7, 13, 30, 0, DateTimeKind.Utc),
                UserId = SeedData.EditorUserId,
                ArticleId = SeedData.Article2Id
            },
            new ArticleImage
            {
                Id = SeedData.ArticleImage3Id,
                FileName = "images/seed-cloud.jpg",
                OriginalFileName = "cloud-deployment.jpg",
                CreatedAt = new DateTime(2025, 12, 21, 10, 15, 0, DateTimeKind.Utc),
                UserId = SeedData.CriticEditorUserId,
                ArticleId = SeedData.Article9Id
            }
        };

            var existingIds = await dbContext.ArticleImages
                .Where(img => seedImages.Select(s => s.Id).Contains(img.Id))
                .Select(img => img.Id)
                .ToListAsync();

            var newImages = seedImages.Where(img => !existingIds.Contains(img.Id)).ToList();

            if (newImages.Any())
            {
                dbContext.ArticleImages.AddRange(newImages);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds a few article ratings from the critic and editor-critic users.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        private static async Task SeedArticleRatingsAsync(BlogDbContext dbContext)
        {
            var seedRatings = new[]
            {
            // Article 1: +2 (two positive ratings)
            new ArticleRating
            {
                Value = ArticleRatingValue.Positive,
                ArticleId = SeedData.Article1Id,
                UserId = SeedData.CriticUserId
            },
            new ArticleRating
            {
                Value = ArticleRatingValue.Positive,
                ArticleId = SeedData.Article1Id,
                UserId = SeedData.CriticEditorUserId
            },

            // Article 2: +1 (one positive rating)
            new ArticleRating
            {
                Value = ArticleRatingValue.Positive,
                ArticleId = SeedData.Article2Id,
                UserId = SeedData.CriticUserId
            },

            // Article 3: +1 (one positive rating)
            new ArticleRating
            {
                Value = ArticleRatingValue.Positive,
                ArticleId = SeedData.Article3Id,
                UserId = SeedData.CriticEditorUserId
            },

            // Article 4: 0 (one positive and one negative)
            new ArticleRating
            {
                Value = ArticleRatingValue.Positive,
                ArticleId = SeedData.Article4Id,
                UserId = SeedData.CriticUserId
            },
            new ArticleRating
            {
                Value = ArticleRatingValue.Negative,
                ArticleId = SeedData.Article4Id,
                UserId = SeedData.CriticEditorUserId
            },

            // Article 5: -1 (one negative rating)
            new ArticleRating
            {
                Value = ArticleRatingValue.Negative,
                ArticleId = SeedData.Article5Id,
                UserId = SeedData.CriticUserId
            },

            // Article 6: -2 (two negative ratings)
            new ArticleRating
            {
                Value = ArticleRatingValue.Negative,
                ArticleId = SeedData.Article6Id,
                UserId = SeedData.CriticUserId
            },
            new ArticleRating
            {
                Value = ArticleRatingValue.Negative,
                ArticleId = SeedData.Article6Id,
                UserId = SeedData.CriticEditorUserId
            },

            // Article 7, 8, 9, 10: No ratings
        };

            var addedCount = 0;

            foreach (var rating in seedRatings)
            {
                var exists = await dbContext.ArticleRatings
                    .AnyAsync(r => r.ArticleId == rating.ArticleId && r.UserId == rating.UserId);

                if (!exists)
                {
                    dbContext.ArticleRatings.Add(rating);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds a few article comments from the critic and commentator users.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        private static async Task SeedArticleCommentsAsync(BlogDbContext dbContext)
        {
            var seedComments = new[]
            {
                // Article 1: Two comments from different commentators
                new ArticleComment
                {
                    Text = "Great introduction! Looking forward to more articles like this.",
                    ArticleId = SeedData.Article1Id,
                    UserId = SeedData.CriticUserId,
                    CreatedAt = new DateTime(2025, 12, 2, 14, 15, 0, DateTimeKind.Utc)
                },
                new ArticleComment
                {
                    Text = "This really helped me understand the basics. Thanks for sharing!",
                    ArticleId = SeedData.Article1Id,
                    UserId = SeedData.CommentatorUserId,
                    CreatedAt = new DateTime(2025, 12, 3, 9, 30, 0, DateTimeKind.Utc)
                },

                // Article 2: Two comments from the same commentator (Steve)
                new ArticleComment
                {
                    Text = "I wish this article existed when I first started with ASP.NET Core!",
                    ArticleId = SeedData.Article2Id,
                    UserId = SeedData.CommentatorUserId,
                    CreatedAt = new DateTime(2025, 12, 8, 10, 20, 0, DateTimeKind.Utc)
                },
                new ArticleComment
                {
                    Text = "Quick follow-up: any chance you'll cover middleware in more detail?",
                    ArticleId = SeedData.Article2Id,
                    UserId = SeedData.CommentatorUserId,
                    CreatedAt = new DateTime(2025, 12, 8, 16, 45, 0, DateTimeKind.Utc)
                },

                // Article 8: One comment from Nancy
                new ArticleComment
                {
                    Text = "Finally someone who explains structured logging properly! This should be required reading.",
                    ArticleId = SeedData.Article8Id,
                    UserId = SeedData.CriticUserId,
                    CreatedAt = new DateTime(2025, 12, 21, 11, 30, 0, DateTimeKind.Utc)
                },

                // Article 10: One comment from Steve
                new ArticleComment
                {
                    Text = "The section on integration tests was especially helpful. Could you cover mocking in a future article?",
                    ArticleId = SeedData.Article10Id,
                    UserId = SeedData.CommentatorUserId,
                    CreatedAt = new DateTime(2025, 12, 24, 15, 10, 0, DateTimeKind.Utc)
                }
            };

            var addedCount = 0;

            foreach (var comment in seedComments)
            {
                var exists = await dbContext.ArticleComments
                    .AnyAsync(c => c.ArticleId == comment.ArticleId
                                && c.UserId == comment.UserId
                                && c.Text == comment.Text);

                if (!exists)
                {
                    dbContext.ArticleComments.Add(comment);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await dbContext.SaveChangesAsync();
            }
        }

        private static class SeedData
        {
            // User IDs
            public static readonly Guid AdminUserId = Guid.Parse("a1b2c3d4-1111-2222-3333-444455556666");
            public static readonly Guid CriticUserId = Guid.Parse("b2c3d4e5-7777-8888-9999-aaaaaaaaaaaa");
            public static readonly Guid EditorUserId = Guid.Parse("c3d4e5f6-aaaa-bbbb-cccc-ddddeeeeffff");
            public static readonly Guid CriticEditorUserId = Guid.Parse("d4e5f607-1234-5678-90ab-cdef12345678");
            public static readonly Guid CommentatorUserId = Guid.Parse("e5f60718-1234-abcd-1234-cdef12345678");

            // Article IDs
            public static readonly Guid Article1Id = Guid.Parse("e1e2e3e4-1111-2222-3333-444455556666");
            public static readonly Guid Article2Id = Guid.Parse("e2e3e4e5-1111-2222-3333-444455556666");
            public static readonly Guid Article3Id = Guid.Parse("e3e4e5e6-1111-2222-3333-444455556666");
            public static readonly Guid Article4Id = Guid.Parse("e4e5e6e7-1111-2222-3333-444455556666");
            public static readonly Guid Article5Id = Guid.Parse("e5e6e7e8-1111-2222-3333-444455556666");
            public static readonly Guid Article6Id = Guid.Parse("e6e7e8e9-1111-2222-3333-444455556666");
            public static readonly Guid Article7Id = Guid.Parse("f1f2f3f4-1111-2222-3333-444455556666");
            public static readonly Guid Article8Id = Guid.Parse("f2f3f4f5-1111-2222-3333-444455556666");
            public static readonly Guid Article9Id = Guid.Parse("f3f4f5f6-1111-2222-3333-444455556666");
            public static readonly Guid Article10Id = Guid.Parse("f4f5f6f7-1111-2222-3333-444455556666");

            // Image IDs
            public static readonly Guid ArticleImage1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            public static readonly Guid ArticleImage2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
            public static readonly Guid ArticleImage3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        }
    }
}
