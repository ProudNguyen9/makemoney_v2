using System.Text.Json;
using ScrapWebsite.Areas.Admin.ViewModels.Forms;

namespace ScrapWebsite.Services.Admin;

public sealed record PostAutosavePayload(
    string? Title,
    string? Slug,
    int? PostCategoryId,
    string? Excerpt,
    string? Content,
    string? SeoKeywords,
    string? AuthorName,
    int SortOrder,
    bool IsFeatured,
    DateTime? PublishedAt,
    List<int>? LinkedProductIds);

public static class PostAutosavePayloadMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new();

    public static PostAutosavePayload FromForm(PostFormViewModel form)
    {
        return new PostAutosavePayload(
            form.Title,
            form.Slug,
            form.PostCategoryId,
            form.Excerpt,
            form.Content,
            form.SeoKeywords,
            form.AuthorName,
            form.SortOrder,
            form.IsFeatured,
            form.PublishedAt,
            form.LinkedProductIds);
    }

    public static string Serialize(PostAutosavePayload payload)
        => JsonSerializer.Serialize(payload, SerializerOptions);

    public static PostAutosavePayload? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<PostAutosavePayload>(json, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Đè nội dung bản nháp tự lưu lên form hiện tại.</summary>
    public static void ApplyTo(PostAutosavePayload payload, PostFormViewModel form)
    {
        if (!string.IsNullOrWhiteSpace(payload.Title))
        {
            form.Title = payload.Title;
        }

        form.Slug = payload.Slug;
        form.PostCategoryId = payload.PostCategoryId;
        form.Excerpt = payload.Excerpt;
        form.Content = payload.Content;
        form.SeoKeywords = payload.SeoKeywords;
        if (!string.IsNullOrWhiteSpace(payload.AuthorName))
        {
            form.AuthorName = payload.AuthorName;
        }

        form.SortOrder = payload.SortOrder;
        form.IsFeatured = payload.IsFeatured;
        if (payload.PublishedAt.HasValue)
        {
            form.PublishedAt = payload.PublishedAt;
        }

        if (payload.LinkedProductIds is { Count: > 0 })
        {
            form.LinkedProductIds = payload.LinkedProductIds;
        }
    }
}
