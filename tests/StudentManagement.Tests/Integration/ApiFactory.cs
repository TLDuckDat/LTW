using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Data;

namespace StudentManagement.Tests.Integration;

/// <summary>
/// Chạy app thật (môi trường Development: tự migrate + seed) trên một CSDL LocalDB riêng, tên ngẫu nhiên,
/// xóa khi chạy xong. Cần SQL Server LocalDB (đi kèm Visual Studio). Đổi máy chủ qua biến môi trường
/// TEST_SQL_SERVER, ví dụ "localhost,1433;User Id=sa;Password=..." nếu dùng SQL Server trong Docker.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _database = $"StudentManagement_Test_{Guid.NewGuid():N}";

    private string ConnectionString
    {
        get
        {
            var server = Environment.GetEnvironmentVariable("TEST_SQL_SERVER") ?? @"(localdb)\MSSQLLocalDB;Trusted_Connection=True";
            return $"Server={server};Database={_database};TrustServerCertificate=True;MultipleActiveResultSets=true";
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
    }

    // Tạo server (chạy migration + seed) một lần trước khi các test trong collection chạy
    public Task InitializeAsync()
    {
        _ = Server;
        return Task.CompletedTask;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        using (var scope = Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }

    public async Task<ApiClient> LoginAsync(string tenDangNhap, string matKhau)
    {
        var client = new ApiClient(CreateClient());
        var (status, body) = await client.SendAsync(HttpMethod.Post, "/api/auth/login", new { tenDangNhap, matKhau });
        Assert.True(status == HttpStatusCode.OK, $"Đăng nhập {tenDangNhap} thất bại: {status} {body}");
        client.Http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body.GetProperty("data").GetProperty("token").GetString());
        return client;
    }

    public Task<ApiClient> AdminAsync() => LoginAsync("admin", DbSeeder.MatKhauAdmin);
    public Task<ApiClient> PhongDaoTaoAsync() => LoginAsync("pdt", DbSeeder.MatKhauPhongDaoTao);
    public Task<ApiClient> GiangVienAsync() => LoginAsync("gv001", DbSeeder.MatKhauGiangVien);
    public ApiClient Anonymous() => new(CreateClient());
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}

/// <summary>Gọi API và đọc phản hồi ApiResponse dưới dạng JSON.</summary>
public class ApiClient(HttpClient http)
{
    public HttpClient Http { get; } = http;

    public async Task<(HttpStatusCode Status, JsonElement Body)> SendAsync(HttpMethod method, string url, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await Http.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        var json = string.IsNullOrEmpty(text) ? default : JsonDocument.Parse(text).RootElement.Clone();
        return (response.StatusCode, json);
    }

    public Task<(HttpStatusCode Status, JsonElement Body)> GetAsync(string url) => SendAsync(HttpMethod.Get, url);
    public Task<(HttpStatusCode Status, JsonElement Body)> PostAsync(string url, object? body = null) => SendAsync(HttpMethod.Post, url, body);
    public Task<(HttpStatusCode Status, JsonElement Body)> PutAsync(string url, object? body = null) => SendAsync(HttpMethod.Put, url, body);

    /// <summary>Gọi API, yêu cầu thành công với đúng mã, trả về phần "data".</summary>
    public async Task<JsonElement> DataAsync(HttpMethod method, string url, object? body = null,
        HttpStatusCode expected = HttpStatusCode.OK)
    {
        var (status, json) = await SendAsync(method, url, body);
        Assert.True(status == expected, $"{method} {url}: mong đợi {(int)expected}, nhận {(int)status} {json}");
        return json.GetProperty("data");
    }
}

public static class JsonExtensions
{
    public static string Message(this JsonElement body) => body.GetProperty("message").GetString() ?? "";
    public static int Id(this JsonElement data) => data.GetProperty("id").GetInt32();
    public static JsonElement.ArrayEnumerator Items(this JsonElement data) => data.GetProperty("items").EnumerateArray();
}
