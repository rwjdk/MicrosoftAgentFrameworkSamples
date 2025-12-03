using Microsoft.Extensions.DependencyInjection;

namespace Playground;

public class ToolWithDiNeed
{
    public async Task<string> GetNews(IServiceProvider serviceProvider)
    {
        HttpClient httpClient = serviceProvider.GetRequiredService<HttpClient>();

        return await httpClient.GetStringAsync("http://rss.cnn.com/rss/cnn_topstories.rss");
    }
}