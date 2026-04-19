using System.Globalization;
using Microsoft.AspNetCore.Components.Forms;
namespace AHS.Web.Hive.Services; internal static class ThermalDataParser { public static async Task<float[]> ParseThermalLogAsync(IBrowserFile file) { var result = new List<float>(); using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 5); using var reader = new StreamReader(stream); while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line) { if (float.TryParse(line.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float val)) result.Add(val); } return result.ToArray(); } }
