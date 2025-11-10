using LibraryApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LibraryApp.Application.Responses
{
    public class BookResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string CoverUrl { get; set; }
        public List<string> Categories { get; set; } = [];
        public int LikesCount { get; set; } = 0;
        public int TotalViews { get; set; } = 0;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    public class BookShortResponse
    {
        public long Id { get; set; }
        public string CoverUrl { get; set; }
        public string Title { get; set; }
    }

    public class BooksPaginatedResponse
    {
        public List<BookShortResponse> Books { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
