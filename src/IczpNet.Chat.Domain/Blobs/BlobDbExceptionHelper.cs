using System;

namespace IczpNet.Chat.Blobs;

public static class BlobDbExceptionHelper
{
    public static bool IsUniqueConstraintViolation(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            var type = current.GetType();
            if (type.FullName is "Microsoft.Data.SqlClient.SqlException" or "System.Data.SqlClient.SqlException")
            {
                if (type.GetProperty("Number")?.GetValue(current) is int number && (number == 2601 || number == 2627))
                {
                    return true;
                }
            }
            else if (type.FullName == "Microsoft.Data.Sqlite.SqliteException")
            {
                if (type.GetProperty("SqliteErrorCode")?.GetValue(current) is int code &&
                    code == 19 &&
                    current.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
