using Google.Authenticator;
using HospitalManagement.Data;
using HospitalManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HospitalManagement.Services
{
    public class TwoFactorAuthService
    {
        private readonly TwoFactorAuthenticator _twoFactorAuthenticator;

        private readonly ApplicationDbContext _context;

        public TwoFactorAuthService(ApplicationDbContext context)
        {
            _twoFactorAuthenticator = new TwoFactorAuthenticator();
            _context = context;
        }




        public string GenerateDeviceId(string userAgent, string ipAddress)
        {
            var deviceString = $"{userAgent}_{ipAddress}";
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceString));
                return System.Convert.ToBase64String(hash);
            }
        }

        public async Task<bool> IsDeviceTrusted(int employeeId, string deviceId)
        {
            var trustedDevice = await _context.TrustedDevices
                .FirstOrDefaultAsync(td => td.EmployeeId == employeeId &&
                                         td.DeviceId == deviceId &&
                                         td.ExpiryDate > DateTime.Now); // Use ExpiryDate directly

            return trustedDevice != null;
        }

        public async Task AddTrustedDevice(int employeeId, string deviceId, string deviceName, int daysToRemember = 30)
        {
            var trustedDevice = new TrustedDevice
            {
                EmployeeId = employeeId,
                DeviceId = deviceId,
                DeviceName = deviceName,
                CreatedDate = DateTime.Now,
                LastUsed = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(daysToRemember)
            };

            _context.TrustedDevices.Add(trustedDevice);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDeviceUsage(int employeeId, string deviceId)
        {
            var trustedDevice = await _context.TrustedDevices
                .FirstOrDefaultAsync(td => td.EmployeeId == employeeId && td.DeviceId == deviceId);

            if (trustedDevice != null)
            {
                trustedDevice.LastUsed = DateTime.Now;
                _context.Update(trustedDevice);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<TrustedDevice>> GetTrustedDevices(int employeeId)
        {
            return await _context.TrustedDevices
                .Where(td => td.EmployeeId == employeeId && td.ExpiryDate > DateTime.Now)
                .OrderByDescending(td => td.LastUsed)
                .ToListAsync();
        }

        public async Task RemoveTrustedDevice(int deviceId, int employeeId)
        {
            var device = await _context.TrustedDevices
                .FirstOrDefaultAsync(td => td.Id == deviceId && td.EmployeeId == employeeId);

            if (device != null)
            {
                _context.TrustedDevices.Remove(device);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAllTrustedDevices(int employeeId)
        {
            var devices = await _context.TrustedDevices
                .Where(td => td.EmployeeId == employeeId)
                .ToListAsync();

            _context.TrustedDevices.RemoveRange(devices);
            await _context.SaveChangesAsync();
        }








        public string GenerateSecretKey()
        {
            var key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase32String(key);
        }

        public SetupCode GenerateQrCode(string email, string secretKey, string appName = "SchoolProject")
        {
            return _twoFactorAuthenticator.GenerateSetupCode(appName, email, secretKey, false);
        }

        public bool ValidatePin(string secretKey, string pin)
        {
            return _twoFactorAuthenticator.ValidateTwoFactorPIN(secretKey, pin);
        }

        public List<string> GenerateRecoveryCodes(int count = 10)
        {
            var recoveryCodes = new List<string>();
            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < count; i++)
                {
                    var bytes = new byte[4];
                    rng.GetBytes(bytes);
                    var code = Convert.ToBase32String(bytes).Replace("=", "");
                    recoveryCodes.Add(code);
                }
            }
            return recoveryCodes;
        }
    }

    // Helper class for Base32 encoding
    public static class Convert
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string ToBase32String(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            int buffer = bytes[0];
            int next = 1;
            int bitsLeft = 8;

            while (bitsLeft > 0 || next < bytes.Length)
            {
                if (bitsLeft < 5)
                {
                    if (next < bytes.Length)
                    {
                        buffer <<= 8;
                        buffer |= bytes[next++] & 0xFF;
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = 5 - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }

                int index = 0x1F & (buffer >> (bitsLeft - 5));
                bitsLeft -= 5;
                result.Append(Base32Alphabet[index]);
            }

            return result.ToString();
        }
    }



}
