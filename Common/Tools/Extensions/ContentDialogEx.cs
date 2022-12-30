namespace WinUICommunity.Common.Extensions;

public static class ContentDialogEx
{
    private static TaskCompletionSource<ContentDialog> _contentDialogShowRequest;

    public static async Task<ContentDialogResult> ShowAsyncQueue(this ContentDialog dialog)
    {
        while (_contentDialogShowRequest != null)
        {
            await _contentDialogShowRequest.Task;
        }

        var request = _contentDialogShowRequest = new TaskCompletionSource<ContentDialog>();
        var result = await dialog.ShowAsync();
        _contentDialogShowRequest = null;
        request.SetResult(dialog);

        return result;
    }
}
