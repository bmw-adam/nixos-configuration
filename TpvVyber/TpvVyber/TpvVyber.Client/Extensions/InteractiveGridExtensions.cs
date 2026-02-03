using MudBlazor;
using TpvVyber.Client.Classes;
using TpvVyber.Client.Services.Admin;

namespace TpvVyber.Client.Extensions;

public interface IInteractiveGrid
{
    void StateHasChanged();
    Task InvokeAsync(Func<Task> work);
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
                var maxIndex = currentPage * elementsPerPage;
                var minIndex = (currentPage == 0 ? 0 : currentPage - 1) * elementsPerPage;

                if ((i <= maxIndex && i >= minIndex) || (currentPage == 0 && i <= elementsPerPage))
                {
                    if (ReThrowError)
                    {
                        _ = InvokeAsync(async () =>
                        {
                            StateHasChanged();
                            await Task.Delay(1);
                        });
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
