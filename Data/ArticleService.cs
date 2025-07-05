using System.Text.Json;
using Microsoft.JSInterop;

namespace FoodBlog.Data;

public class ArticleService
{
    private readonly IJSRuntime _js;
    private const string StorageKey = "foodblog_articles";
    private List<Article> _articles = new();

    public ArticleService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var items = JsonSerializer.Deserialize<List<Article>>(json);
                if (items != null)
                    _articles = items;
            }
        }
        catch
        {
            // ignore errors reading from storage
        }
    }

    public Task<List<Article>> GetArticlesAsync()
    {
        return Task.FromResult(_articles.OrderByDescending(a => a.Created).ToList());
    }

    public async Task AddArticleAsync(Article article)
    {
        _articles.Add(article);
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_articles);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}
