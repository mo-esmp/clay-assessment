using System.ComponentModel.DataAnnotations;

namespace SmartLock.WebApi.Models;

public record LockAccessRequest(
    [Required][MaxLength(64)] string EmployeeId
);