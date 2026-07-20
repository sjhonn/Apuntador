using Microsoft.JSInterop;

namespace Apuntador.Services;

public sealed class ThemeService(IJSRuntime js, AppDataService dataService)
{
    public async Task ApplyAsync()
    {
        await dataService.InitializeAsync();
        await js.InvokeVoidAsync("apuntadorTheme.apply", dataService.Data.Theme);
    }

    public async Task ToggleAsync()
    {
        await dataService.InitializeAsync();
        dataService.Data.Theme = dataService.Data.Theme == "dark" ? "light" : "dark";
        await js.InvokeVoidAsync("apuntadorTheme.apply", dataService.Data.Theme);
        await dataService.SaveAsync();
    }
}
