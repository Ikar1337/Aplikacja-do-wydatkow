using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace ProjektPWSW.Wpf.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    // Zmieniony konstruktor - wymusza adres na sztywno
    public ApiClient(string baseUrl = "http://localhost:5000/")
    {
        _http = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") };
    }

    public async Task<T?> GetOneAsync<T>(string url)
        => await _http.GetFromJsonAsync<T>(url);

    public async Task<List<T>> GetListAsync<T>(string url)
        => await _http.GetFromJsonAsync<List<T>>(url) ?? new();

    public async Task<T> GetAsync<T>(string url)
    {
        var res = await _http.GetAsync(url);
        res.EnsureSuccessStatusCode();

        var json = await res.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (obj == null)
            throw new Exception("API zwróciło pustą odpowiedź.");

        return obj;
    }

    public async Task<T?> PostAsync<T>(string url, object body)
    {
        var res = await _http.PostAsJsonAsync(url, body);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<T>();
    }

    public async Task PutAsync(string url, object body)
    {
        var res = await _http.PutAsJsonAsync(url, body);
        res.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string url)
    {
        var res = await _http.DeleteAsync(url);
        res.EnsureSuccessStatusCode();
    }

    // Nowa metoda wrzucona poprawnie DO ŚRODKA klasy
    public async Task<T?> PostFileAsync<T>(string url, string filePath)
    {
        using var content = new MultipartFormDataContent();

        // otwieranie pliku z dysku
        using var fileStream = System.IO.File.OpenRead(filePath);
        using var streamContent = new StreamContent(fileStream);

        // Poprawiona wielkość liter: streamContent zamiast StreamContent
        content.Add(streamContent, "file", System.IO.Path.GetFileName(filePath));

        // Poprawiona zmienna: wysyłamy całego 'content' z plikiem
        var res = await _http.PostAsync(url, content);
        res.EnsureSuccessStatusCode();

        return await res.Content.ReadFromJsonAsync<T>();
    }
}