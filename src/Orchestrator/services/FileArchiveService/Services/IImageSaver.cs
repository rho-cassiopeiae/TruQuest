namespace Services;

internal interface IImageSaver
{
    Task<string> SaveLocalCopy(string url);
}