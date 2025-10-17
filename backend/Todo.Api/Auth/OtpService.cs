using System.Security.Cryptography;
using System.Text;
using Todo.Api.Data;
using Todo.Api.Models;

namespace Todo.Api.Auth;

public class OtpService
{
    private readonly AppDbContext _db;
    public OtpService(AppDbContext db) { _db = db; }

    // Send OTP (mock): OTP = last 4 characters of contact string
    public async Task<string> SendOtpAsync(string contact)
    {
        var otp = contact.Length >= 4 ? contact[^4..] : contact;
        var hash = Hash(otp);
        var entry = new OtpEntry {
            Id = Guid.NewGuid(),
            Username = contact,
            OtpHash = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Attempts = 0
        };
        _db.Otps.Add(entry);
        await _db.SaveChangesAsync();
        // In dev, return otp so UI/testers can use it. In production, send SMS / email.
        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string contact, string otp)
    {
        var entry = _db.Otps.Where(o => o.Username == contact).OrderByDescending(o => o.ExpiresAt).FirstOrDefault();
        if (entry == null) return false;
        if (entry.ExpiresAt < DateTime.UtcNow) { _db.Otps.Remove(entry); await _db.SaveChangesAsync(); return false; }
        if (entry.Attempts >= 5) { _db.Otps.Remove(entry); await _db.SaveChangesAsync(); return false; }
        if (entry.OtpHash != Hash(otp)) { entry.Attempts++; await _db.SaveChangesAsync(); return false; }
        // valid - consume it
        _db.Otps.Remove(entry);
        await _db.SaveChangesAsync();
        return true;
    }

    private static string Hash(string s) {
        using var sha = SHA256.Create();
        var b = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        return Convert.ToBase64String(b);
    }
}
