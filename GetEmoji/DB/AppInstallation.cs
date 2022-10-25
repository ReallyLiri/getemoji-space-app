using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GetEmoji.DB;

[Table("Installations")]
public record AppInstallation(
    string ServerUrl,
    [property: Key] string ClientId,
    string ClientSecret
);
