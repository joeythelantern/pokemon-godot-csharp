using System.Threading.Tasks;
using Godot;
using HttpClient = System.Net.Http.HttpClient;

namespace Game.Core;

public static class Modules
{
    public static bool IsActionJustPressed()
    {
        return Input.IsActionJustPressed("ui_up") || Input.IsActionJustPressed("ui_down") || Input.IsActionJustPressed("ui_left") || Input.IsActionJustPressed("ui_right");
    }

    public static bool IsActionPressed()
    {
        return Input.IsActionPressed("ui_up") || Input.IsActionPressed("ui_down") || Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_right");
    }

    public static bool IsActionJustReleased()
    {
        return Input.IsActionJustReleased("ui_up") || Input.IsActionJustReleased("ui_down") || Input.IsActionJustReleased("ui_left") || Input.IsActionJustReleased("ui_right");
    }

    public static Vector2I ConvertVector2ToVector2I(Vector2 vector)
    {
        return new Vector2I((int)vector.X / Globals.GRID_SIZE, (int)vector.Y / Globals.GRID_SIZE);
    }

    public static Vector2 ConvertVector2IToVector2(Vector2I vector)
    {
        return new Vector2I(vector.X * Globals.GRID_SIZE, vector.Y * Globals.GRID_SIZE);
    }

    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<Variant> FetchDataFromPokeApi(string url)
    {
        try
        {
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Api Error: failed fetching {url} -> {response.StatusCode}");
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();
            return Json.ParseString(json);
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Api Error: failed fetching {url} -> {ex.Message}");
            return default;
        }
    }
}