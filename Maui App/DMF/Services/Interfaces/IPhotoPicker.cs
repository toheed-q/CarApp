namespace DMF.Services.Interfaces
{
    /// <summary>
    /// Opens the device photo gallery (not the file explorer) for multi-select and
    /// returns local file paths of the chosen images. Implemented per-platform so the
    /// native gallery picker with numbered multi-select is used.
    /// </summary>
    public interface IPhotoPicker
    {
        Task<IReadOnlyList<string>> PickImagesAsync(int max);
    }
}
