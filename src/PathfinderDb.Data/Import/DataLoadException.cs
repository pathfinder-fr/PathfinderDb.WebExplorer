namespace PathfinderDb.Data.Import;

public sealed class DataLoadException(string message, Exception? innerException = null)
    : Exception(message, innerException);
