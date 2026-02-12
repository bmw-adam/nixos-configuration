using MudBlazor;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;

namespace TpvVyber.Client.Extensions;

public interface IInteractiveGrid
{
    void StateHasChanged();
    Task InvokeAsync(Func<Task> work);
    Task InvokeAsync(Action work);
    bool ReThrowError { get; set; }
    IAdminService AdminService { get; }
    public async Task ReloadDataAsync<T>(
        IAsyncEnumerable<T> data,
        List<T> gridItems,
        int currentPage,
        int elementsPerPage
    )
        where T : class, IEntityId
    {
        {
            var set = new HashSet<int>(gridItems.Select(i => i.Id));
            var stillExsistingIds = new HashSet<int>();
            int i = 0;

            await foreach (var item in data)
            {
                if (item == null)
                {
                    continue;
                }

                stillExsistingIds.Add(item.Id);

                if (set.Contains(item.Id))
                {
                    // Update existing item in gridItems
                    var index = gridItems.FindIndex(i => i.Id == item.Id);
                    gridItems[index] = item;
                }
                else
                {
                    gridItems.Add(item);
                }

                // Rerender?
                var maxIndex = elementsPerPage + (currentPage * elementsPerPage) + 1;
                var minIndex = (currentPage * elementsPerPage) - 1;

                if (i <= maxIndex && i >= minIndex)
                {
                    if (ReThrowError || true)
                    {
                        await InvokeAsync(() => StateHasChanged());
                        await Task.Delay(1);
                    }
                    else
                    {
#pragma warning disable CS1998
                        _ = InvokeAsync(async () => StateHasChanged());
#pragma warning restore CS1998
                    }
                }

                i++;
            }

            // Remove items that no longer exist
            gridItems.RemoveAll(item => !stillExsistingIds.Contains(item.Id));
            if (ReThrowError)
            {
                StateHasChanged();
                await Task.Delay(1);
            }
            else
            {
                StateHasChanged();
            }
        }
    }
}
