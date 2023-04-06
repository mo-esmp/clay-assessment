using System.ComponentModel.DataAnnotations;

namespace SmartLock.WebApi.Models;

public record TokenRequest(
    [Required][MaxLength(128)] string Username,
    [Required][MaxLength(128)] string Password
);