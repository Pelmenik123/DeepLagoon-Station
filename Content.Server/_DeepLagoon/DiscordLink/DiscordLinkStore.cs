using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Content.Server._DeepLagoon.DiscordLink;

/// <summary>
/// A dedicated durable identity database in the server's data directory.
/// All operations are serialized on the game thread. Raw codes are never stored.
/// </summary>
public sealed class DiscordLinkStore : IDisposable
{
    private readonly SqliteConnection _db;
    private readonly Func<long> _now;

    public DiscordLinkStore(string path, Func<long>? now = null)
    {
        _now = now ?? (() => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
#if USE_SYSTEM_SQLITE
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
#endif
        _db = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path }.ToString());
        _db.Open();
        Execute("""
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS discord_links (
                discord_id TEXT PRIMARY KEY, ss14_uid TEXT NOT NULL UNIQUE,
                username TEXT NOT NULL, linked_at INTEGER NOT NULL);
            CREATE TABLE IF NOT EXISTS discord_launcher_enrollments (discord_id TEXT PRIMARY KEY, ss14_uid TEXT NOT NULL UNIQUE);
            CREATE TABLE IF NOT EXISTS discord_link_history (discord_id TEXT PRIMARY KEY, ss14_uid TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS discord_link_codes (
                ss14_uid TEXT PRIMARY KEY, code_hash TEXT NOT NULL UNIQUE,
                username TEXT NOT NULL, expires_at INTEGER NOT NULL, issued_at INTEGER NOT NULL);
            CREATE TABLE IF NOT EXISTS discord_link_attempts (
                discord_id TEXT PRIMARY KEY, window_start INTEGER NOT NULL, attempts INTEGER NOT NULL);
            """);
    }

    private SqliteCommand Command(string sql, params (string Name, object Value)[] parameters)
    {
        var command = _db.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);
        return command;
    }

    private void Execute(string sql, params (string Name, object Value)[] parameters)
    {
        using var command = Command(sql, parameters);
        command.ExecuteNonQuery();
    }

    public Link? FindDiscord(string discordId)
    {
        using var command = Command("SELECT ss14_uid, username FROM discord_links WHERE discord_id=$id", ("$id", discordId));
        using var reader = command.ExecuteReader();
        return reader.Read() ? new Link(Guid.Parse(reader.GetString(0)), reader.GetString(1)) : null;
    }

    /// <summary>Trusted OAuth backend only. Never grants whitelist or replaces an existing UID.</summary>
    public Link EnrollLauncher(string discordId)
    {
        var existing = FindDiscord(discordId);
        if (existing != null) return existing;
        using var transaction = _db.BeginTransaction();
        using (var check = Command("SELECT 1 FROM discord_launcher_enrollments WHERE discord_id=$id UNION ALL SELECT 1 FROM discord_link_history WHERE discord_id=$id", ("$id", discordId)))
        {
            check.Transaction = transaction;
            if (check.ExecuteScalar() != null) throw new LinkException("enrollment_revoked");
        }
        var uid = Guid.NewGuid();
        var username = "Lagoon_" + discordId;
        using (var insert = Command("INSERT INTO discord_links VALUES($id,$uid,$name,$now)", ("$id", discordId), ("$uid", uid.ToString()), ("$name", username), ("$now", _now())))
        {
            insert.Transaction = transaction;
            insert.ExecuteNonQuery();
        }
        using (var record = Command("INSERT INTO discord_launcher_enrollments VALUES($id,$uid)", ("$id", discordId), ("$uid", uid.ToString())))
        {
            record.Transaction = transaction;
            record.ExecuteNonQuery();
        }
        transaction.Commit();
        return new Link(uid, username);
    }

    public void Unlink(string discordId, Guid uid)
    {
        using var transaction = _db.BeginTransaction();
        using (var history = Command("INSERT OR IGNORE INTO discord_link_history VALUES($id,$uid)", ("$id", discordId), ("$uid", uid.ToString())))
        {
            history.Transaction = transaction;
            history.ExecuteNonQuery();
        }
        using (var record = Command("INSERT OR IGNORE INTO discord_launcher_enrollments VALUES($id,$uid)", ("$id", discordId), ("$uid", uid.ToString())))
        {
            record.Transaction = transaction;
            record.ExecuteNonQuery();
        }
        using (var command = Command("DELETE FROM discord_links WHERE discord_id=$id AND ss14_uid=$uid", ("$id", discordId), ("$uid", uid.ToString())))
        {
            command.Transaction = transaction;
            if (command.ExecuteNonQuery() != 1)
                throw new LinkException("not_linked");
        }
        using (var command = Command("DELETE FROM discord_link_codes WHERE ss14_uid=$uid", ("$uid", uid.ToString())))
        {
            command.Transaction = transaction;
            command.ExecuteNonQuery();
        }
        transaction.Commit();
    }

    public void AssertCurrent(string discordId, Guid uid, long linkedAt)
    {
        using var command = Command("SELECT 1 FROM discord_links WHERE discord_id=$id AND ss14_uid=$uid AND linked_at=$at",
            ("$id", discordId), ("$uid", uid.ToString()), ("$at", linkedAt));
        if (command.ExecuteScalar() == null) throw new LinkException("stale_link");
    }

    public Link ReassignDiscord(string source, string target, Guid expectedUid, long expectedLinkedAt)
    {
        using var transaction = _db.BeginTransaction();
        AssertCurrent(source, expectedUid, expectedLinkedAt);
        var link = FindDiscord(source)!;
        if (source == target) return link;
        if (FindDiscord(target) != null) throw new LinkException("already_linked");
        using (var prior = Command("SELECT ss14_uid FROM discord_link_history WHERE discord_id=$id UNION ALL SELECT ss14_uid FROM discord_launcher_enrollments WHERE discord_id=$id", ("$id", target)))
        {
            prior.Transaction = transaction;
            using var reader = prior.ExecuteReader();
            while (reader.Read())
                if (Guid.Parse(reader.GetString(0)) != expectedUid) throw new LinkException("identity_conflict");
        }
        foreach (var id in new[] { source, target })
        {
            using var history = Command("INSERT OR IGNORE INTO discord_link_history VALUES($id,$uid)", ("$id", id), ("$uid", expectedUid.ToString()));
            history.Transaction = transaction;
            history.ExecuteNonQuery();
        }
        using (var change = Command("UPDATE discord_links SET discord_id=$target,linked_at=$now WHERE discord_id=$source",
                   ("$target", target), ("$source", source), ("$now", Math.Max(_now(), expectedLinkedAt + 1))))
        {
            change.Transaction = transaction;
            change.ExecuteNonQuery();
        }
        using (var codes = Command("DELETE FROM discord_link_codes WHERE ss14_uid=$uid", ("$uid", expectedUid.ToString())))
        {
            codes.Transaction = transaction;
            codes.ExecuteNonQuery();
        }
        transaction.Commit();
        return link;
    }

    public Link RestoreDiscord(string discordId, Guid uid, string username)
    {
        using var transaction = _db.BeginTransaction();
        if (FindDiscord(discordId) != null || IsLinked(uid)) throw new LinkException("already_linked");
        var found = false;
        using (var prior = Command("SELECT ss14_uid FROM discord_link_history WHERE discord_id=$id UNION ALL SELECT ss14_uid FROM discord_launcher_enrollments WHERE discord_id=$id", ("$id", discordId)))
        {
            prior.Transaction = transaction;
            using var reader = prior.ExecuteReader();
            while (reader.Read())
            {
                if (Guid.Parse(reader.GetString(0)) != uid) throw new LinkException("identity_conflict");
                found = true;
            }
        }
        if (!found) throw new LinkException("not_linked");
        using var insert = Command("INSERT INTO discord_links VALUES($id,$uid,$name,$at)",
            ("$id", discordId), ("$uid", uid.ToString()), ("$name", username), ("$at", _now()));
        insert.Transaction = transaction;
        insert.ExecuteNonQuery();
        transaction.Commit();
        return new Link(uid, username);
    }

    public bool IsLinked(Guid uid)
    {
        using var command = Command("SELECT 1 FROM discord_links WHERE ss14_uid=$uid", ("$uid", uid.ToString()));
        return command.ExecuteScalar() != null;
    }

    public string Issue(Guid uid, string username)
    {
        if (IsLinked(uid))
            throw new LinkException("already_linked");
        var now = _now();
        using (var command = Command("SELECT issued_at FROM discord_link_codes WHERE ss14_uid=$uid", ("$uid", uid.ToString())))
        {
            if (command.ExecuteScalar() is long issued && now - issued < 30)
                throw new LinkException("cooldown");
        }
        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
        Execute("DELETE FROM discord_link_codes WHERE expires_at <= $now", ("$now", now));
        Execute("""
            INSERT INTO discord_link_codes VALUES($uid,$hash,$name,$expires,$now)
            ON CONFLICT(ss14_uid) DO UPDATE SET code_hash=excluded.code_hash,
              username=excluded.username,expires_at=excluded.expires_at,issued_at=excluded.issued_at
            """, ("$uid", uid.ToString()), ("$hash", Hash(code)), ("$name", username), ("$expires", now + 600), ("$now", now));
        return code;
    }

    public Link Consume(string discordId, string code)
    {
        if (FindDiscord(discordId) != null)
            throw new LinkException("already_linked");
        var now = _now();
        // Count failed attempts durably so restarting the game does not reset the limit.
        Execute("DELETE FROM discord_link_attempts WHERE window_start <= $cutoff", ("$cutoff", now - 600));
        Execute("""
            INSERT INTO discord_link_attempts VALUES($id,$now,1)
            ON CONFLICT(discord_id) DO UPDATE SET attempts=attempts+1
            """, ("$id", discordId), ("$now", now));
        using (var command = Command("SELECT attempts FROM discord_link_attempts WHERE discord_id=$id", ("$id", discordId)))
        {
            if ((long) command.ExecuteScalar()! > 10)
                throw new LinkException("rate_limited");
        }
        using var transaction = _db.BeginTransaction();
        Link link;
        using (var command = Command("SELECT ss14_uid, username FROM discord_link_codes WHERE code_hash=$hash AND expires_at>$now",
                   ("$hash", Hash(code)), ("$now", now)))
        {
            command.Transaction = transaction;
            using var reader = command.ExecuteReader();
            if (!reader.Read())
                throw new LinkException("invalid_code");
            link = new Link(Guid.Parse(reader.GetString(0)), reader.GetString(1));
        }
        using (var history = Command("SELECT ss14_uid FROM discord_link_history WHERE discord_id=$id UNION ALL SELECT ss14_uid FROM discord_launcher_enrollments WHERE discord_id=$id", ("$id", discordId)))
        {
            history.Transaction = transaction;
            using var reader = history.ExecuteReader();
            while (reader.Read())
                if (Guid.Parse(reader.GetString(0)) != link.Uid) throw new LinkException("identity_conflict");
        }
        using (var command = Command("INSERT INTO discord_links VALUES($id,$uid,$name,$now)",
                   ("$id", discordId), ("$uid", link.Uid.ToString()), ("$name", link.Username), ("$now", now)))
        {
            command.Transaction = transaction;
            command.ExecuteNonQuery();
        }
        using (var command = Command("DELETE FROM discord_link_codes WHERE ss14_uid=$uid", ("$uid", link.Uid.ToString())))
        {
            command.Transaction = transaction;
            command.ExecuteNonQuery();
        }
        transaction.Commit();
        return link;
    }

    private static string Hash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
    public void Dispose() => _db.Dispose();
    public sealed record Link(Guid Uid, string Username);
    public sealed class LinkException(string code) : Exception(code);
}
